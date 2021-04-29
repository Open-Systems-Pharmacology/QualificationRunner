using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
      ///    Path of the log file associated with the run
      /// </summary>
      public string LogFile { get; set; }

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
      private readonly ILogWatcherFactory _logWatcherFactory;

      public QualificationEngine(
         IOSPSuiteLogger logger,
         IStartableProcessFactory startableProcessFactory,
         IQualificationRunnerConfiguration applicationConfiguration,
         IJsonSerializer jsonSerializer, ILogWatcherFactory logWatcherFactory)
      {
         _logger = logger;
         _startableProcessFactory = startableProcessFactory;
         _applicationConfiguration = applicationConfiguration;
         _jsonSerializer = jsonSerializer;
         _logWatcherFactory = logWatcherFactory;
      }

      public Task<QualificationRunResult> Validate(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken) =>
         execute(qualifcationConfiguration, runOptions, cancellationToken, validate: true);

      public Task<QualificationRunResult> Run(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken) =>
         execute(qualifcationConfiguration, runOptions, cancellationToken, validate: false);

      private async Task<QualificationRunResult> execute(QualifcationConfiguration qualifcationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken, bool validate)
      {
         _logger.AddDebug(Logs.StartingQualificationRunForProject(qualifcationConfiguration.Project));

         var logFile = Path.Combine(qualifcationConfiguration.TempFolder, "log.txt");
         var configFile = Path.Combine(qualifcationConfiguration.TempFolder, "config.json");
         var project = qualifcationConfiguration.Project;
         var qualificationRunResult = new QualificationRunResult
         {
            ConfigFile = configFile,
            LogFile = logFile,
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
            var code = startBatchProcess(configFile, logFile, runOptions.LogLevel, validate, project, pksimCLIPath, cancellationToken);
            qualificationRunResult.Success = (code == ExitCodes.Success);
            return qualificationRunResult;
         }, cancellationToken);
      }

      private ExitCodes startBatchProcess(string configFile, string logFile, LogLevel logLevel, bool validate, string projectId, string pksimCLIPath, CancellationToken cancellationToken)
      {
         var args = new List<string>
         {
            "qualification",
            "-i",
            configFile.InQuotes(),
            "-l",
            logFile.InQuotes(),
            "--logLevel",
            logLevel.ToString(),
            "-r"
         };

         if (validate)
            args.Add("-v");

         var logWatcherOptions = new LogWatcherOptions
         {
            LogFile = logFile,
            Category = projectId,
         };
         using (var process = _startableProcessFactory.CreateStartableProcess(pksimCLIPath, args.ToArray()))
         using (var watcher = _logWatcherFactory.CreateLogWatcher(logWatcherOptions))
         {
            watcher.Watch();
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