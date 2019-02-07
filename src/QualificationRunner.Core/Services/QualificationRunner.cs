using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Extensions;
using OSPSuite.Core.Qualification;
using OSPSuite.Core.Services;
using OSPSuite.Utility;
using OSPSuite.Utility.Extensions;
using QualificationRunner.Core.Domain;
using QualificationRunner.Core.RunOptions;
using static QualificationRunner.Core.Services.ExpandoObjectHelpers;
using static QualificationRunner.Core.Assets.Errors;
using static QualificationRunner.Core.Constants;
using Project = QualificationRunner.Core.Domain.Project;

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
         var qualificationPlan = await _jsonSerializer.Deserialize<ExpandoObject>(runOptions.ConfigurationFile);
         if (!HasProperty(qualificationPlan, Configuration.PROJECTS))
            throw new QualificationRunnerException(ProjectsNotDefinedInQualificationFile);

         IReadOnlyList<Project> projects = GetListFrom<Project>(ByName(qualificationPlan, Configuration.PROJECTS));
         IReadOnlyList<Plot> allPlots = retrieveProjectPlots(qualificationPlan);

         var begin = DateTime.UtcNow;

         //Configurations only need to be created once!
         var projectConfigurations = projects.Select(p => createQualifcationConfigurationFor(p, projects, allPlots)).ToList();

         _logger.AddDebug("Copying static files");
         await copyStaticFiles(qualificationPlan);

         _logger.AddDebug("Starting validation runs");
         var validations = await Task.WhenAll(projectConfigurations.Select(validateProject));

         var invalidConfigurations = validations.Where(x => !x.Success).ToList();
         if (invalidConfigurations.Any())
            throw new QualificationRunnerException(invalidConfigurations);

         //Run all qualification projects
//         _logger.AddDebug("Starting qualification runs");
//         var runResults = await Task.WhenAll(projectConfigurations.Select(runQualification));
//         var invalidRunResults = runResults.Where(x => !x.Success).ToList();
//         if (invalidRunResults.Any())
//            throw new QualificationRunnerException(invalidRunResults);

//         await createReportConfigurationPlan(runResults, qualificationPlan);

         var end = DateTime.UtcNow;
         var timeSpent = end - begin;
         _logger.AddInfo($"Qualification scenario finished in {timeSpent.ToDisplay()}");
      }

      private Task copyStaticFiles(ExpandoObject qualificationPlan)
      {
         //Sections
         var currentFolder = FileHelper.FolderFromFileFullPath(_runOptions.ConfigurationFile);
         var contentFolder = Path.Combine(currentFolder, CONTENT_FOLDER);

         if (DirectoryHelper.DirectoryExists(contentFolder))
            CopyDirectory(contentFolder, _runOptions.OutputFolder);

         //Observed Data
         if (HasProperty(qualificationPlan, Configuration.OBSERVED_DATA_SETS))
         {
            //Path is relative to input file and should be adapted 
            IReadOnlyList<ObservedDataMapping> observedDataSets = GetListFrom<ObservedDataMapping>(ByName(qualificationPlan, Configuration.OBSERVED_DATA_SETS));
            observedDataSets?.Each(copyObservedData);
         }

         return Task.CompletedTask;
      }

      private void copyObservedData(ObservedDataMapping observedDataMapping)
      {
         var observedDataFilePath = Path.Combine(_runOptions.ConfigurationFile, observedDataMapping.Path);
         var fileInfo = new FileInfo(observedDataFilePath);
         if (!fileInfo.Exists)
            throw new QualificationRunnerException(ObservedDataFileNotFound(observedDataFilePath));

         DirectoryHelper.CreateDirectory(_runOptions.ObservedDataFolder);
         fileInfo.CopyTo(Path.Combine(_runOptions.ObservedDataFolder, fileInfo.Name), overwrite: true);
      }

      private Task createReportConfigurationPlan(QualificationRunResult[] runResults, dynamic qualificationPlan)
      {
         dynamic reportConfigurationPlan = new ExpandoObject();
         reportConfigurationPlan.Hello = "xxxx";
         return Task.CompletedTask;
      }

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

      private QualifcationConfiguration createQualifcationConfigurationFor(Project project, IReadOnlyList<Project> projects, IReadOnlyList<Plot> allPlots)
      {
         var projectId = project.Id;

         var tmpFolder = Path.Combine(_runOptions.OutputFolder, "temp", projectId);

         if (DirectoryHelper.DirectoryExists(tmpFolder))
            DirectoryHelper.DeleteDirectory(tmpFolder, true);

         DirectoryHelper.CreateDirectory(tmpFolder);

         return new QualifcationConfiguration
         {
            ProjectId = projectId,
            OutputFolder = _runOptions.OutputFolder,
            ReportConfigurationFile = _runOptions.ConfigurationFile,
            ObservedDataFolder = _runOptions.ObservedDataFolder,
            MappingFile = Path.Combine(tmpFolder, "mapping.json"),
            SnapshotFile = snapshotFileFor(project),
            TempFolder = tmpFolder,
            BuildingBlocks = mapBuildingBlocks(project.BuildingBlocks, projects),
            SimulationPlots = mapPlots(allPlots, projectId)
         };
      }

      private SimulationPlot[] mapPlots(IReadOnlyList<Plot> allPlots, string projectId)
      {
         return allPlots?.Where(x => string.Equals(x.RefProject, projectId)).Select(x => new SimulationPlot
         {
            SectionId = x.SectionId,
            Simulation = x.RefSimulation
         }).ToArray();
      }

      private string snapshotFileFor(Project project) => Path.Combine(_runOptions.ConfigurationFile, project.Path);

      private BuildingBlockSwap[] mapBuildingBlocks(BuildingBlock[] buildingBlocks, IReadOnlyList<Project> projects)
      {
         return buildingBlocks?.Select(bb => mapBuildingBlock(bb, projects)).ToArray();
      }

      private BuildingBlockSwap mapBuildingBlock(BuildingBlock buildingBlock, IReadOnlyList<Project> projects)
      {
         var project = projects.FindById(buildingBlock.RefProject);
         if (project == null)
            throw new QualificationRunnerException(ReferencedProjectNotDefinedInQualificationFile(buildingBlock.RefProject));

         return new BuildingBlockSwap
         {
            Name = buildingBlock.Name,
            Type = buildingBlock.BuildingBlockType,
            SnapshotFile = snapshotFileFor(project)
         };
      }

      private IReadOnlyList<Plot> retrieveProjectPlots(dynamic reportConfiguration)
      {
         if (!HasProperty(reportConfiguration, Configuration.PLOTS))
            return null;

         var plots = ByName(reportConfiguration, Configuration.PLOTS);
         if (!HasProperty(plots, Configuration.ALL_PLOTS))
            return null;

         return GetListFrom<Plot>(ByName(plots, Configuration.ALL_PLOTS));
      }

      public dynamic ByName(ExpandoObject obj, string propertyName)
      {
         var byName = (IDictionary<string, object>) obj;
         return byName[propertyName];
      }

      public T Cast<T>(dynamic obj) where T : class
      {
         var json = _jsonSerializer.SerializeAsString(obj);
         return _jsonSerializer.DeserializeFromString<T>(json);
      }

      public IReadOnlyList<T> GetListFrom<T>(dynamic enumerable) where T : class
      {
         var list = new List<T>();

         foreach (var item in enumerable)
         {
            list.Add(Cast<T>(item));
         }

         return list;
      }

      public static void CopyDirectory(string source, string target, bool createRootDirectory = true)
      {
         CopyDirectory(new DirectoryInfo(source), new DirectoryInfo(target), createRootDirectory);
      }

      public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target, bool createRootDirectory = true)
      {
         if (!target.Exists)
            target.Create();

         var rootTarget = target;
         if (createRootDirectory)
            rootTarget = target.CreateSubdirectory(source.Name);

         foreach (var dir in source.GetDirectories())
         {
            //Do not creaet root directory as it is created in this call
            CopyDirectory(dir, rootTarget.CreateSubdirectory(dir.Name), createRootDirectory: false);
         }

         foreach (var file in source.GetFiles())
         {
            file.CopyTo(Path.Combine(rootTarget.FullName, file.Name), overwrite: true);
         }
      }
   }
}