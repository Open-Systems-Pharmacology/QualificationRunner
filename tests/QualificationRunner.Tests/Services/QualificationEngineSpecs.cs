using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using OSPSuite.Core.Qualification;
using OSPSuite.Core.Services;
using QualificationRunner.Core;
using QualificationRunner.Core.RunOptions;
using QualificationRunner.Core.Services;

namespace QualificationRunner.Tests.Services
{
   public abstract class concern_for_QualificationEngine : ContextSpecification<QualificationEngine>
   {
      protected IOSPSuiteLogger _logger;
      protected IStartableProcessFactory _startableProcessFactory;
      protected IQualificationRunnerConfiguration _applicationConfiguration;
      protected IJsonSerializer _jsonSerializer;
      protected QualifcationConfiguration _qualificationConfiguration;
      protected QualificationRunOptions _runOptions;
      protected string _tempFolder;
      protected string _configFile;
      protected string _projectLogFile;
      protected string _mappingFile;
      protected string _runLogFile;
      protected string _pksimInstallationFolder;
      protected string _pksimCliPath;

      protected override void Context()
      {
         _logger = A.Fake<IOSPSuiteLogger>();
         _startableProcessFactory = A.Fake<IStartableProcessFactory>();
         _applicationConfiguration = A.Fake<IQualificationRunnerConfiguration>();
         _jsonSerializer = A.Fake<IJsonSerializer>();

         sut = new QualificationEngine(_logger, _startableProcessFactory, _applicationConfiguration, _jsonSerializer);

         _tempFolder = Path.Combine(Path.GetTempPath(), "QualificationEngineSpecs", Path.GetRandomFileName());
         Directory.CreateDirectory(_tempFolder);

         _mappingFile = Path.Combine(_tempFolder, "mapping.json");
         _projectLogFile = Path.Combine(_tempFolder, "log.txt");
         _configFile = Path.Combine(_tempFolder, "config.json");
         _runLogFile = Path.Combine(_tempFolder, "runner.log");
         _pksimInstallationFolder = Path.Combine(_tempFolder, "PKSim");
         Directory.CreateDirectory(_pksimInstallationFolder);
         _pksimCliPath = Path.Combine(_tempFolder, "PKSim.CLI.exe");
         File.WriteAllText(_pksimCliPath, "fake");

         _qualificationConfiguration = new QualifcationConfiguration
         {
            Project = "P1",
            TempFolder = _tempFolder,
            MappingFile = _mappingFile
         };

         _runOptions = new QualificationRunOptions
         {
            LogFile = _runLogFile,
            LogLevel = LogLevel.Warning,
            PKSimInstallationFolder = _pksimInstallationFolder,
            Run = false,
            ExportProjectFiles = false
         };

         A.CallTo(() => _applicationConfiguration.PKSimCLIPathFor(_runOptions.PKSimInstallationFolder)).Returns(_pksimCliPath);
         A.CallTo(() => _jsonSerializer.Serialize(_qualificationConfiguration, _configFile)).Returns(Task.CompletedTask);
      }

      public override void Cleanup()
      {
         if (Directory.Exists(_tempFolder))
            Directory.Delete(_tempFolder, true);

         base.Cleanup();
      }
   }

   public class When_running_a_qualification_and_the_pksim_cli_file_does_not_exist : concern_for_QualificationEngine
   {
      [Observation]
      public void should_throw_when_pksim_cli_does_not_exist()
      {
         var missingPath = Path.Combine(_tempFolder, "missing", "PKSim.CLI.exe");
         A.CallTo(() => _applicationConfiguration.PKSimCLIPathFor(_runOptions.PKSimInstallationFolder)).Returns(missingPath);

         The.Action(async () => await sut.Run(_qualificationConfiguration, _runOptions, CancellationToken.None))
            .ShouldThrowAn<QualificationRunException>();

         A.CallTo(() => _jsonSerializer.Serialize(_qualificationConfiguration, _configFile)).MustHaveHappened();
      }
   }

   public class When_validating_a_qualification_and_the_pksim_cli_file_does_not_exist : concern_for_QualificationEngine
   {
      [Observation]
      public void should_throw_when_pksim_cli_does_not_exist()
      {
         var missingPath = Path.Combine(_tempFolder, "missing", "PKSim.CLI.exe");
         A.CallTo(() => _applicationConfiguration.PKSimCLIPathFor(_runOptions.PKSimInstallationFolder)).Returns(missingPath);

         The.Action(async () => await sut.Validate(_qualificationConfiguration, _runOptions, CancellationToken.None))
            .ShouldThrowAn<QualificationRunException>();
      }

      protected override void Context()
      {
         base.Context();
         _runOptions.ExportProjectFiles = true;
      }
   }

   public class When_running_a_qualification_with_an_already_canceled_token : concern_for_QualificationEngine
   {
      [Observation]
      public void should_stop_when_the_cancellation_token_is_already_canceled()
      {
         var cancellationToken = new CancellationToken(canceled: true);

         The.Action(async () => await sut.Run(_qualificationConfiguration, _runOptions, cancellationToken))
            .ShouldThrowAn<TaskCanceledException>();
      }
   }
}
