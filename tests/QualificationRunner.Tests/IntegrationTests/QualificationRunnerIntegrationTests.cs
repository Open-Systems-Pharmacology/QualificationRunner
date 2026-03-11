using System.IO;
using NUnit.Framework;
using OSPSuite.BDDHelper;
using QualificationRunner.Core.RunOptions;
using Microsoft.Extensions.Logging;
using Services = QualificationRunner.Core.Services;

namespace QualificationRunner.IntegrationTests
{
   [Category("IntegrationTests")]
   public abstract class concern_for_QualificationRunnerIntegration : ContextForIntegration<Services.QualificationRunner>
   {
      public override void GlobalContext()
      {
         base.GlobalContext();
         sut.RunBatchAsync(RunOptions).Wait();
      }

      protected abstract string TestProjectName();
      protected string TestProjectFolder => Path.Combine(TestDataFolder, TestProjectName());

      protected QualificationRunOptions RunOptions => new QualificationRunOptions
         {
            ConfigurationFile = Path.Combine(TestProjectFolder, "Input/qualification_plan.json"),
            OutputFolder = Path.Combine(TestProjectFolder, "re_input"),
            LogFile = Path.Combine(TestProjectFolder, "logfile.txt"),
            Run = false,
            ExportProjectFiles = false,
            ForceDelete = true,
            LogLevel = LogLevel.Debug,
            ConfigurationFolder = Path.Combine(TestProjectFolder, "Input"),
            ReportConfigurationFileName = "report-configuration-plan",
            PKSimInstallationFolder = System.Environment.GetEnvironmentVariable("PKSIM_INSTALLATION_FOLDER")
         };
   }

   public class When_processing_a_qualification_plan_single_project : concern_for_QualificationRunnerIntegration
   {
      protected override string TestProjectName()
      {
         return "SingleProject";
      }

      [Observation]
      public void should_create_all_expected_files()
      {
         var x = 1;
      }

      [Observation]
      public void log_files_should_not_contain_errors_or_warnings()
      {
         var x = 1;
      }

   }
}
