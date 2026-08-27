using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using OSPSuite.Core.Qualification;
using OSPSuite.Core.Services;
using QualificationRunner.Core;
using QualificationRunner.Core.RunOptions;
using QualificationRunner.Core.Services;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IJsonSerializer = QualificationRunner.Core.Services.IJsonSerializer;

namespace QualificationRunner.Tests.Services
{
   public abstract class concern_for_QualificationEngine : ContextSpecification<QualificationEngine>
   {
      protected IOSPSuiteLogger _logger;
      protected IStartableProcessFactory _startableProcessFactory;
      protected IQualificationRunnerConfiguration _applicationConfiguration;
      protected IJsonSerializer _jsonSerializer;
      protected QualificationConfiguration _qualificationConfiguration;
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

         _qualificationConfiguration = new QualificationConfiguration
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
      protected override void Context()
      {
         base.Context();

         var missingPath = Path.Combine(_tempFolder, "missing", "PKSim.CLI.exe");
         A.CallTo(() => _applicationConfiguration.PKSimCLIPathFor(_runOptions.PKSimInstallationFolder)).Returns(missingPath);
      }

      [Observation]
      public void should_throw_when_pksim_cli_does_not_exist()
      {
         The.Action(async () => await sut.Run(_qualificationConfiguration, _runOptions, CancellationToken.None))
            .ShouldThrowAn<QualificationRunException>();

         A.CallTo(() => _jsonSerializer.Serialize(_qualificationConfiguration, _configFile)).MustHaveHappened();
      }
   }

   public class When_validating_a_qualification_and_the_pksim_cli_file_does_not_exist : concern_for_QualificationEngine
   {
      protected override void Context()
      {
         base.Context();

         _runOptions.ExportProjectFiles = true;
         var missingPath = Path.Combine(_tempFolder, "missing", "PKSim.CLI.exe");
         A.CallTo(() => _applicationConfiguration.PKSimCLIPathFor(_runOptions.PKSimInstallationFolder)).Returns(missingPath);
      }

      [Observation]
      public void should_throw_when_pksim_cli_does_not_exist()
      {
         The.Action(async () => await sut.Validate(_qualificationConfiguration, _runOptions, CancellationToken.None))
            .ShouldThrowAn<QualificationRunException>();
      }

   }

   public class When_running_a_qualification_and_the_cli_process_exits_successfully : concern_for_QualificationEngine
   {
      private TestStartableProcess _process;
      private QualificationRunResult _result;

      protected override void Context()
      {
         base.Context();
         _process = new TestStartableProcess();
         A.CallTo(() => _startableProcessFactory.CreateStartableProcess(A<string>._, A<string[]>._)).Returns(_process);
      }

      protected override void Because()
      {
         _result = sut.Run(_qualificationConfiguration, _runOptions, CancellationToken.None).Result;
      }

      [Observation]
      public void should_return_a_successful_result()
      {
         _result.Success.ShouldBeTrue();
      }

      [Observation]
      public void should_limit_the_processor_count_visible_to_the_cli_process()
      {
         _process.StartInfo.Environment[Constants.DOTNET_PROCESSOR_COUNT].ShouldBeEqualTo("1");
      }

      //Starts a real trivial process so that the engine can read an exit code once the process has exited
      private class TestStartableProcess : StartableProcess
      {
         public TestStartableProcess() : base("cmd.exe", "/c", "exit 0")
         {
         }

         //do not apply the priority: the trivial process may already have exited when the priority would be set
         public override void Start(ProcessPriorityClass? priority = null) => base.Start();
      }
   }

   public class When_running_a_qualification_with_an_already_canceled_token : concern_for_QualificationEngine
   {
      [Test]
      public void should_stop_when_the_cancellation_token_is_already_canceled()
      {
         var cancellationToken = new CancellationToken(canceled: true);

         The.Action(async () => await sut.Run(_qualificationConfiguration, _runOptions, cancellationToken))
            .ShouldThrowAn<TaskCanceledException>();
      }
   }
}
