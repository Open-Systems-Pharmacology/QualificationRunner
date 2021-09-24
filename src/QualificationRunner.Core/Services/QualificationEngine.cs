using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OSPSuite.Core.Qualification;
using OSPSuite.Core.Services;
using OSPSuite.Utility;
using QualificationRunner.Core.Assets;
using QualificationRunner.Core.Domain;
using QualificationRunner.Core.RunOptions;

namespace QualificationRunner.Core.Services
{
   public class QualificationRunResult : IReferencingProject
   {
      /// <summary>
      ///    Path of all the log files associated with the run
      /// </summary>
      public IEnumerable<string> LogFilePaths { get; set; }

      /// <summary>
      ///    Path of the config file associated with the rin
      /// </summary>
      public string ConfigFile { get; set; }

      /// <summary>
      ///    Name of the mapping file created as a result of the qualification run
      /// </summary>
      public string MappingFile { get; set; }

      /// <summary>
      ///    Was the run successful
      /// </summary>
      public bool Success { get; set; }

      public string Project { get; set; }
   }

   public interface IQualificationEngine : IDisposable
   {
      Task<QualificationRunResult> Run(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken);
      Task<QualificationRunResult> Validate(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken);
   }

   public class QualificationEngine : IQualificationEngine
   {
      private readonly IOSPSuiteLogger _logger;
      private readonly IStartableProcessFactory _startableProcessFactory;
      private readonly IQualificationRunnerConfiguration _applicationConfiguration;
      private readonly IJsonSerializer _jsonSerializer;

      public QualificationEngine(
         IOSPSuiteLogger logger,
         IStartableProcessFactory startableProcessFactory,
         IQualificationRunnerConfiguration applicationConfiguration,
         IJsonSerializer jsonSerializer)
      {
         _logger = logger;
         _startableProcessFactory = startableProcessFactory;
         _applicationConfiguration = applicationConfiguration;
         _jsonSerializer = jsonSerializer;
      }

      public Task<QualificationRunResult> Validate(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken) =>
         execute(qualifcationConfiguration, runOptions, cancellationToken, validate: true);

      public Task<QualificationRunResult> Run(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken) =>
         execute(qualifcationConfiguration, runOptions, cancellationToken, validate: false);

      private async Task<QualificationRunResult> execute(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken, bool validate)
      {
         _logger.AddDebug(Logs.StartingQualificationRunForProject(qualifcationConfiguration.Project));

         var logFilePaths = new List<string> {Path.Combine(qualifcationConfiguration.TempFolder, "log.txt"), qualifcationConfiguration.QRSharedLogPath } ;
         var configFile = Path.Combine(qualifcationConfiguration.TempFolder, "config.json");
         var project = qualifcationConfiguration.Project;
         var qualificationRunResult = new QualificationRunResult
         {
            ConfigFile = configFile,
            LogFilePaths = logFilePaths,
            Project = project,
            MappingFile = qualifcationConfiguration.MappingFile
         };

         await _jsonSerializer.Serialize(qualifcationConfiguration, configFile);

         _logger.AddDebug(Logs.QualificationConfigurationForProjectExportedTo(project, configFile));

         var pksimCLIPath = _applicationConfiguration.PKSimCLIPathFor(runOptions.PKSimInstallationFolder);

         if(!FileHelper.FileExists(pksimCLIPath))
            throw new QualificationRunException(Errors.PKSimCLIFileNotFound(pksimCLIPath));

         return await Task.Run(() =>
         {
            var code = startBatchProcess(configFile, logFilePaths.ToList(), runOptions.LogLevel, validate, project, pksimCLIPath, runOptions.Run, cancellationToken);
            qualificationRunResult.Success = (code == ExitCodes.Success);
            return qualificationRunResult;
         }, cancellationToken);
      }

      private ExitCodes startBatchProcess(string configFile, List<string> logFilePaths, LogLevel logLevel, bool validate, string projectId, string pksimCLIPath, bool run,  CancellationToken cancellationToken)
      {
         var quotedPaths = logFilePaths.Select(element => element.InQuotes());

         var args = new List<string>
         {
            "qualification",
            "-i",
            configFile.InQuotes(),
            "-l",
            string.Join(" ", quotedPaths),
            "--logLevel",
            logLevel.ToString()
         };

         if(run)
            args.Add("-r");

         if (validate)
            args.Add("-v");

         using (var process = _startableProcessFactory.CreateStartableProcess(pksimCLIPath, args.ToArray()))
         {
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.Start();
            process.Wait(cancellationToken);
            return (ExitCodes) process.ReturnCode;
         }
      }

      protected virtual void Cleanup()
      {
      }

      #region Disposable properties

      private bool _disposed;

      public void Dispose()
      {
         if (_disposed) return;

         Cleanup();
         GC.SuppressFinalize(this);
         _disposed = true;
      }

      ~QualificationEngine()
      {
         Cleanup();
      }

      #endregion
   }
}