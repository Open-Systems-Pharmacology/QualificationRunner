using System.IO;
using NUnit.Framework;
using OSPSuite.BDDHelper;
using QualificationRunner.Core.RunOptions;
using Microsoft.Extensions.Logging;
using Services = QualificationRunner.Core.Services;

namespace QualificationRunner.IntegrationTests
{
   [Category("IntegrationTests")]
   public abstract class concern_for_QualificationEngineIntegration : ContextForIntegration<Services.QualificationRunner>
   {
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

   public class When_processing_a_qualification_plan_single_project : concern_for_QualificationEngineIntegration
   {
      protected override string TestProjectName()
      {
         return "SingleProject";
      }

      [Observation]
      public void should_create_all_expected_files()
      {
         //var runOptions = new QualificationRunOptions
         //{
         //   ConfigurationFile = Path.Combine(TestProjectFolder, "Input/qualification_plan.json"),
         //   OutputFolder = Path.Combine(TestProjectFolder, "re_input"),
         //   LogFile = Path.Combine(TestProjectFolder, "logfile.txt"),
         //   Run = false,
         //   ExportProjectFiles = false,
         //   ForceDelete = true,
         //   LogLevel = LogLevel.Debug,
         //   ConfigurationFolder = Path.Combine(TestProjectFolder, "Input"),
         //   ReportConfigurationFileName = "report-configuration-plan",
         //   PKSimInstallationFolder = System.Environment.GetEnvironmentVariable("PKSIM_INSTALLATION_FOLDER")
         //};

         sut.RunBatchAsync(RunOptions).Wait();

      }


   }
}
