﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Extensions;
using OSPSuite.Core.Qualification;
using OSPSuite.Core.Services;
using OSPSuite.Utility;
using OSPSuite.Utility.Extensions;
using QualificationRunner.Core.Domain;
using QualificationRunner.Core.RunOptions;
using static QualificationRunner.Core.Assets.Errors;
using static QualificationRunner.Core.Constants;
using BuildingBlockRef = QualificationRunner.Core.Domain.BuildingBlockRef;
using Project = QualificationRunner.Core.Domain.Project;
using SimulationParameterRef = QualificationRunner.Core.Domain.SimulationParameterRef;

namespace QualificationRunner.Core.Services
{
   public class QualificationRunner : IBatchRunner<QualificationRunOptions>
   {
      private readonly IJsonSerializer _jsonSerializer;
      private readonly ILogger _logger;
      private readonly IQualificationEngineFactory _qualificationEngineFactory;
      private QualificationRunOptions _runOptions;

      public QualificationRunner(IJsonSerializer jsonSerializer, ILogger logger, IQualificationEngineFactory qualificationEngineFactory)
      {
         _jsonSerializer = jsonSerializer;
         _logger = logger;
         _qualificationEngineFactory = qualificationEngineFactory;
      }

      public async Task RunBatchAsync(QualificationRunOptions runOptions)
      {
         _runOptions = runOptions;

         setupOutputFolder();

         dynamic qualificationPlan = await _jsonSerializer.Deserialize<dynamic>(runOptions.ConfigurationFile);

         IReadOnlyList<Project> projects = GetListFrom<Project>(qualificationPlan.Projects);

         if (projects == null)
            throw new QualificationRunException(ProjectsNotDefinedInQualificationFile);

         IReadOnlyList<SimulationPlot> allPlots = retrieveProjectPlots(qualificationPlan);
         IReadOnlyList<Input> allInputs = retrieveInputs(qualificationPlan);


         var begin = DateTime.UtcNow;

         await updateProjectsFullPath(projects);

         //Configurations only need to be created once!
         var projectConfigurations = await Task.WhenAll(projects.Select(p => createQualifcationConfigurationFor(p, projects, allPlots, allInputs)));

         _logger.AddDebug("Copying static files");
         StaticFiles staticFiles = await copyStaticFiles(qualificationPlan);

         _logger.AddDebug("Starting validation runs");
         var validations = await Task.WhenAll(projectConfigurations.Select(validateProject));

         var invalidConfigurations = validations.Where(x => !x.Success).ToList();
         if (invalidConfigurations.Any())
            throw new QualificationRunException(errorMessageFrom(invalidConfigurations));

         //Run all qualification projects
         _logger.AddDebug("Starting qualification runs");
         var runResults = await Task.WhenAll(projectConfigurations.Select(runQualification));
         var invalidRunResults = runResults.Where(x => !x.Success).ToList();
         if (invalidRunResults.Any())
            throw new QualificationRunException(errorMessageFrom(invalidRunResults));

         await createReportConfigurationPlan(runResults, staticFiles, qualificationPlan);

         var end = DateTime.UtcNow;
         var timeSpent = end - begin;
         _logger.AddInfo($"Qualification scenario finished in {timeSpent.ToDisplay()}");
      }

      private Task updateProjectsFullPath(IReadOnlyList<Project> projects) => Task.WhenAll(projects.Select(updateProjectFullPath));

      private async Task<string> downloadRemoteFile(string url, string locationInTempFolder, string type)
      {
         _logger.AddDebug($"Downloading {type.ToLower()} file from {url}...");
         var downloadFolder = Path.Combine(_runOptions.TempFolder, locationInTempFolder);
         DirectoryHelper.CreateDirectory(downloadFolder);

         using (var wc = new WebClient())
         {
            try
            {
               var fileName = new Uri(url).Segments.Last();
               var fileFullPath = Path.Combine(downloadFolder, fileName);

               await wc.DownloadFileTaskAsync(url, fileFullPath);
               _logger.AddDebug($"{type} file downloaded from {url}to {fileFullPath}");
               return fileFullPath;
            }
            catch (Exception e)
            {
               //Exception is thrown for example if the given url does not exist or if internet access is not possible etc..
               _logger.AddError(e.Message);
               return url;
            }
         }
      }

      private async Task updateProjectFullPath(Project project) => project.SnapshotFilePath = await snapshotFileFullPathFor(project);

      private async Task<StaticFiles> copyStaticFiles(dynamic qualificationPlan)
      {
         var staticFiles = new StaticFiles();
         //Sections
         var contentFolder = Path.Combine(_runOptions.ConfigurationFolder, CONTENT_FOLDER);

         if (DirectoryHelper.DirectoryExists(contentFolder))
            FileHelper.CopyDirectory(contentFolder, _runOptions.OutputFolder);

         //Observed Data
         IReadOnlyList<ObservedDataMapping> observedDataSets = getStaticObservedDataSetFrom(qualificationPlan);
         staticFiles.ObservedDatSets = await Task.WhenAll(observedDataSets.Select(copyObservedData));

         return staticFiles;
      }

      private IReadOnlyList<ObservedDataMapping> getStaticObservedDataSetFrom(dynamic qualificationPlan) => GetListFrom<ObservedDataMapping>(qualificationPlan.ObservedDataSets);

      private string errorMessageFrom(IEnumerable<QualificationRunResult> invalidResults) => invalidResults.Select(x => ProjectConfigurationNotValid(x.Project, x.LogFile)).ToString("\n");

      private async Task<ObservedDataMapping> copyObservedData(ObservedDataMapping observedDataMapping)
      {
         Task<string> downloadRemoteObservedData() => downloadRemoteFile(observedDataMapping.Path, OBSERVED_DATA_DOWNLOAD_FOLDER, "Observed Data");

         var observedDataAbsolutePath = absolutePathFrom(_runOptions.ConfigurationFolder, observedDataMapping.Path);

         if (!localFileExists(observedDataAbsolutePath))
            observedDataAbsolutePath = await downloadRemoteObservedData();

         if (!localFileExists(observedDataAbsolutePath))
            throw new QualificationRunException(ObservedDataFileNotFound(observedDataAbsolutePath));

         var fileInfo = new FileInfo(observedDataAbsolutePath);

         DirectoryHelper.CreateDirectory(_runOptions.ObservedDataFolder);
         var copiedObservedDataFilePath = absolutePathFrom(_runOptions.ObservedDataFolder, fileInfo.Name);
         fileInfo.CopyTo(copiedObservedDataFilePath, overwrite: true);

         return new ObservedDataMapping
         {
            Id = observedDataMapping.Id,
            Type = observedDataMapping.Type,
            Path = pathRelativeToOutputFolder(copiedObservedDataFilePath)
         };
      }

      private static bool localFileExists(string file)
      {
         try
         {
            return FileHelper.FileExists(file);
         }
         catch (Exception)
         {
            return false;
         }
      }

      private string pathRelativeToOutputFolder(string fullPath) => FileHelper.CreateRelativePath(fullPath, _runOptions.OutputFolder, useUnixPathSeparator: true);

      private string absolutePathFrom(string relatedTo, string relativePath)
      {
         var sanitizeRelativePath = relativePath;
         if (sanitizeRelativePath.StartsWith(Path.DirectorySeparatorChar.ToString()) || sanitizeRelativePath.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
            sanitizeRelativePath = sanitizeRelativePath.Substring(1);

         return Path.Combine(relatedTo, sanitizeRelativePath);
      }

      private async Task createReportConfigurationPlan(QualificationRunResult[] runResults, StaticFiles staticFiles, dynamic qualificationPlan)
      {
         dynamic reportConfigurationPlan = new JObject();

         var mappings = await Task.WhenAll(runResults.Select(x => _jsonSerializer.Deserialize<QualificationMapping>(x.MappingFile)));

         reportConfigurationPlan.SimulationMappings = toJArray(mappings.SelectMany(x => x.SimulationMappings));

         reportConfigurationPlan.ObservedDataSets = toJArray(mappings.SelectMany(x => x.ObservedDataMappings).Union(staticFiles.ObservedDatSets));

         var plots = qualificationPlan.Plots;
         RemoveByName(plots, Configuration.ALL_PLOTS);

         plots.TimeProfile = toJArray(mappings.SelectMany(x => x.Plots));
         reportConfigurationPlan.Plots = plots;

         reportConfigurationPlan.Inputs = toJArray(mappings.SelectMany(x => x.Inputs));

         reportConfigurationPlan.Sections = qualificationPlan.Sections;

         await _jsonSerializer.Serialize(reportConfigurationPlan, _runOptions.ReportConfigurationFile);
      }

      private JArray toJArray(IEnumerable<object> enumerable) => new JArray(enumerable.Select(toJObject));

      private JObject toJObject(object p) => _jsonSerializer.DeserializeFromString<dynamic>(_jsonSerializer.SerializeAsString(p));

      private Task<QualificationRunResult> validateProject(QualifcationConfiguration qualificationConfiguration)
      {
         using (var qualificationEngine = _qualificationEngineFactory.Create())
         {
            return qualificationEngine.Validate(qualificationConfiguration, _runOptions, CancellationToken.None);
         }
      }

      private Task<QualificationRunResult> runQualification(QualifcationConfiguration qualificationConfiguration)
      {
         using (var qualificationEngine = _qualificationEngineFactory.Create())
         {
            return qualificationEngine.Run(qualificationConfiguration, _runOptions, CancellationToken.None);
         }
      }

      private async Task<QualifcationConfiguration> createQualifcationConfigurationFor(Project project, IReadOnlyList<Project> projects, IReadOnlyList<SimulationPlot> allPlots, IReadOnlyList<Input> alInputs)
      {
         var projectId = project.Id;

         var tmpProjectFolder = Path.Combine(_runOptions.TempFolder, projectId);

         DirectoryHelper.CreateDirectory(tmpProjectFolder);

         return new QualifcationConfiguration
         {
            Project = projectId,
            OutputFolder = _runOptions.OutputFolder,
            ReportConfigurationFile = _runOptions.ReportConfigurationFile,
            ObservedDataFolder = _runOptions.ObservedDataFolder,
            InputsFolder = _runOptions.InputsFolder,
            MappingFile = Path.Combine(tmpProjectFolder, "mapping.json"),
            SnapshotFile = project.SnapshotFilePath,
            TempFolder = tmpProjectFolder,
            BuildingBlocks = await mapBuildingBlocks(project.BuildingBlocks, projects),
            SimulationParameters = mapSimulationParameters(project.SimulationParameters, projects),
            SimulationPlots = allPlots.ForProject(projectId),
            Inputs = alInputs.ForProject(projectId)
         };
      }


      private Task<BuildingBlockSwap[]> mapBuildingBlocks(BuildingBlockRef[] buildingBlocks, IReadOnlyList<Project> projects)
      {
         if (buildingBlocks == null)
            return Task.FromResult(Array.Empty<BuildingBlockSwap>());

         return Task.WhenAll(buildingBlocks.Select(bb => mapBuildingBlock(bb, projects)));
      }

      private async Task<BuildingBlockSwap> mapBuildingBlock(BuildingBlockRef buildingBlock, IReadOnlyList<Project> projects)
      {
         string snapshotFilePath;
         // Using a project reference
         if (!string.IsNullOrEmpty(buildingBlock.Project))
         {
            var project = projects.FindById(buildingBlock.Project);
            if (project == null)
               throw new QualificationRunException(ReferencedProjectNotDefinedInQualificationFile(buildingBlock.Project));

            snapshotFilePath = await snapshotFileFullPathFor(project);
         }
         else
            snapshotFilePath = await snapshotFileFullPathFor(buildingBlock.Path);


         return new BuildingBlockSwap
         {
            Name = buildingBlock.Name,
            Type = buildingBlock.Type,
            SnapshotFile = snapshotFilePath
         };
      }

      private Task<string> snapshotFileFullPathFor(Project project) => snapshotFileFullPathFor(project.Path);

      private async Task<string> snapshotFileFullPathFor(string snapshotRelativePathOrUrl)
      {
         Task<string> downloadRemoteSnapshot() => downloadRemoteFile(snapshotRelativePathOrUrl, PROJECT_DOWNLOAD_FOLDER, "Project");

         var snapshotAbsolutePath = absolutePathFrom(_runOptions.ConfigurationFolder, snapshotRelativePathOrUrl);

         if (!localFileExists(snapshotAbsolutePath))
            snapshotAbsolutePath = await downloadRemoteSnapshot();

         if (!localFileExists(snapshotAbsolutePath))
            throw new QualificationRunException(SnapshotFileNotFound(snapshotAbsolutePath));

         return snapshotAbsolutePath;

      }

      private SimulationParameterSwap[] mapSimulationParameters(SimulationParameterRef[] simulationParameters, IReadOnlyList<Project> projects) =>
         simulationParameters?.Select(x => mapSimulationParameter(x, projects)).ToArray();

      private SimulationParameterSwap mapSimulationParameter(SimulationParameterRef simulationParameter, IReadOnlyList<Project> projects)
      {
         var project = projects.FindById(simulationParameter.Project);
         if (project == null)
            throw new QualificationRunException(ReferencedProjectNotDefinedInQualificationFile(simulationParameter.Project));

         return new SimulationParameterSwap
         {
            Simulation = simulationParameter.Simulation,
            Path = simulationParameter.Path,
            TargetSimulations = simulationParameter.TargetSimulations,
            SnapshotFile = project.SnapshotFilePath
         };
      }

      private IReadOnlyList<SimulationPlot> retrieveProjectPlots(dynamic reportConfiguration) =>
         GetListFrom<SimulationPlot>(reportConfiguration.Plots?.AllPlots);

      private IReadOnlyList<Input> retrieveInputs(dynamic reportConfiguration) =>
         GetListFrom<Input>(reportConfiguration.Inputs);

      private void setupOutputFolder()
      {
         clearOutputFolder();

         DirectoryHelper.CreateDirectory(_runOptions.OutputFolder);
         DirectoryHelper.CreateDirectory(_runOptions.TempFolder);
      }

      private void clearOutputFolder()
      {
         if (!DirectoryHelper.DirectoryExists(_runOptions.OutputFolder))
            return;

         var files = Directory.GetFiles(_runOptions.OutputFolder, "*.*", SearchOption.AllDirectories).Where(x => !string.Equals(x, _runOptions.LogFile)).ToList();
         if (files.Count == 0)
            return;

         if (!_runOptions.ForceDelete)
            throw new QualificationRunException(OutputFolderIsNotEmpty);

         try
         {
            _logger.AddDebug($"Force deleting output folder '{_runOptions.OutputFolder}'");
            DirectoryHelper.DeleteDirectory(_runOptions.OutputFolder, true);
         }
         catch
         {
            //Ensure that we do not not throw an exception if one file cannot be deleted
         }
      }

      public void RemoveByName(JObject obj, string propertyName)
      {
         var prop = obj?[propertyName];
         prop?.Parent.Remove();
      }

      public T Cast<T>(dynamic obj) where T : class
      {
         var json = _jsonSerializer.SerializeAsString(obj);
         return _jsonSerializer.DeserializeFromString<T>(json);
      }

      public IReadOnlyList<T> GetListFrom<T>(dynamic enumerable) where T : class
      {
         var list = new List<T>();
         if (enumerable == null)
            return list;

         foreach (var item in enumerable)
         {
            list.Add(Cast<T>(item));
         }

         return list;
      }
   }
}