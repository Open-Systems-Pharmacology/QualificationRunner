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
   public interface IQualificationEngine : IDisposable
   {
      Task<QualificationRunResult> Run(QualificationConfiguration qualificationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken);
      Task<QualificationRunResult> Validate(QualificationConfiguration qualificationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken);
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

      public Task<QualificationRunResult> Validate(QualificationConfiguration qualificationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken) =>
         execute(qualificationConfiguration, runOptions, cancellationToken, validate: true);

      public Task<QualificationRunResult> Run(QualificationConfiguration qualificationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken) =>
         execute(qualificationConfiguration, runOptions, cancellationToken, validate: false);

      private async Task<QualificationRunResult> execute(QualificationConfiguration qualificationConfiguration, QualificationRunOptions runOptions, CancellationToken cancellationToken, bool validate)
      {
         _logger.AddDebug(Logs.StartingQualificationRunForProject(qualificationConfiguration.Project));

         var projectLogFile = Path.Combine(qualificationConfiguration.TempFolder, "log.txt");
         var logFilePaths = new List<string> { projectLogFile, runOptions.LogFile };
         var configFile = Path.Combine(qualificationConfiguration.TempFolder, "config.json");
         var project = qualificationConfiguration.Project;
         var qualificationRunResult = new QualificationRunResult
         {
            ConfigFile = configFile,
            LogFilePath = projectLogFile,
            Project = project,
            MappingFile = qualificationConfiguration.MappingFile
         };

         await _jsonSerializer.Serialize(qualificationConfiguration, configFile);

         _logger.AddDebug(Logs.QualificationConfigurationForProjectExportedTo(project, configFile));

         string cliPath, moBiPKSimStarterPath = string.Empty;
         if (qualificationConfiguration.Application == ApplicationType.PKSim)
            cliPath = _applicationConfiguration.PKSimCLIPathFor(runOptions.PKSimInstallationFolder);
         else
         {
            cliPath = _applicationConfiguration.MoBiCLIPathFor(runOptions.MoBiInstallationFolder);

            // If the PK-Sim folder was specified by command line argument the intent is to inform MoBi which PK-Sim instance
            // should be used for PK-Sim services.
            if (!string.IsNullOrEmpty(runOptions.PKSimInstallationFolder))
               moBiPKSimStarterPath = Path.Combine(runOptions.PKSimInstallationFolder, Constants.Tools.PKSIM);
         }

         if (!FileHelper.FileExists(cliPath))
            throw new QualificationRunException(Errors.CliFileNotFound(cliPath));

         return await Task.Run(() =>
         {
            var args = createArgs(configFile, logFilePaths.ToList(), runOptions.LogLevel, validate, runOptions.Run, runOptions.ExportProjectFiles, moBiPKSimStarterPath);

            var code = startBatchProcess(args, cliPath, cancellationToken);
            qualificationRunResult.Success = (code == ExitCodes.Success);
            return qualificationRunResult;
         }, cancellationToken);
      }

      private static List<string> createArgs(string configFile, List<string> logFilePaths, LogLevel logLevel, bool validate, bool run, bool exportProjectFiles, string pkSimPath)
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
            logLevel.ToString(),
            // The runner owns parallelism: it starts one CLI process per project, so each child runs internally serial
            "--cores",
            "1"
         };

         if (run)
            args.Add("-r");

         if (exportProjectFiles)
            args.Add("-e");

         if (validate)
            args.Add("-v");

         if (!string.IsNullOrEmpty(pkSimPath))
            args.AddRange(new[] { "-p", pkSimPath.InQuotes() });

         return args;
      }

      private ExitCodes startBatchProcess(List<string> args, string cliPath, CancellationToken cancellationToken)
      {
         using (var process = _startableProcessFactory.CreateStartableProcess(cliPath, args.ToArray()))
         {
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.Environment[Constants.DOTNET_PROCESSOR_COUNT] = "1";
            process.Start(ProcessPriorityClass.Idle);
            process.Wait(cancellationToken);
            return (ExitCodes)process.ReturnCode;
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