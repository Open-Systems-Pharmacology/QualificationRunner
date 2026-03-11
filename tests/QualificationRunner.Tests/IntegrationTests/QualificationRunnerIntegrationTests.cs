using Microsoft.Extensions.Logging;
using NUnit.Framework;
using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using QualificationRunner.Core.RunOptions;
using System;
using System.IO;
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

      protected string OutputFolder => Path.Combine(TestProjectFolder, "re_input");

      protected string LogFile => Path.Combine(OutputFolder, "logfile.txt");

      protected string ReportConfigurationFileName => "report-configuration-plan";

      protected string InputFolder => Path.Combine(TestProjectFolder, "Input");

      protected string QualificationPlanFile => Path.Combine(InputFolder, "qualification_plan.json");

      protected QualificationRunOptions RunOptions => new QualificationRunOptions
         {
            ConfigurationFile = QualificationPlanFile,
            OutputFolder = OutputFolder,
            LogFile = LogFile,
            Run = false,
            ExportProjectFiles = false,
            ForceDelete = true,
            LogLevel = LogLevel.Debug,
            ConfigurationFolder = InputFolder,
            ReportConfigurationFileName = ReportConfigurationFileName,
            PKSimInstallationFolder = Environment.GetEnvironmentVariable("PKSIM_INSTALLATION_FOLDER")
         };

      protected void CheckFilesExist(string[] filesInOutputFolder)
      {
         foreach (var file in filesInOutputFolder)
         {
            var filePath = file.StartsWith(OutputFolder) ? file : Path.Combine(OutputFolder, file);
            File.Exists(filePath).ShouldBeTrue($"Expected file '{filePath}' does not exist.");
         }
      }

      protected void CheckLogFilesDoNotContainErrorsOrWarnings()
      {
         var logFiles = Directory.GetFiles(OutputFolder, "log*.txt", SearchOption.AllDirectories);
         (logFiles.Length>=2).ShouldBeTrue("Less than 2 log files found in the output folder or its subdirectories.");

         foreach (var logFile in logFiles)
         {
            File.Exists(logFile).ShouldBeTrue($"Log file does not exist at {logFile}");

            var logContent = File.ReadAllText(logFile);
            logContent.IndexOf("Error", StringComparison.OrdinalIgnoreCase).ShouldBeEqualTo(-1, $"Log file {logFile} contains errors.");
            logContent.IndexOf("Warn", StringComparison.OrdinalIgnoreCase).ShouldBeEqualTo(-1, $"Log file {logFile} contains warnings.");
            logContent.IndexOf("Failed", StringComparison.OrdinalIgnoreCase).ShouldBeEqualTo(-1, $"Log file {logFile} contains failed entries.");
            logContent.IndexOf("Invalid", StringComparison.OrdinalIgnoreCase).ShouldBeEqualTo(-1, $"Log file {logFile} contains invalid entries.");
         }
      }
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
         CheckFilesExist(new[]
         {
            LogFile,
            $"{ReportConfigurationFileName}.json",
            @"Compound\Sim 1 - intravenous\Sim 1 - intravenous.pkml",
            @"Compound\Sim 2 - peroral\Sim 2 - peroral.pkml",
            @"Content\References.md",
            @"Inputs\Compound\Compound\COMPOUND.md",
            @"Inputs\Compound\Formulation\Tablet.md",
            @"Intro\titlepage.md",
            @"ObservedData\Observed data iv.csv",
            @"ObservedData\Observed data po.csv",
            @"temp\Compound\config.json",
            @"temp\Compound\log.txt",
            @"temp\Compound\mapping.json"
         });
      }

      [Observation]
      public void log_files_should_not_contain_errors_or_warnings()
      {
         CheckLogFilesDoNotContainErrorsOrWarnings();
      }
   }

   public class When_processing_a_qualification_plan_multiple_projects_with_inheritance : concern_for_QualificationRunnerIntegration
   {
      protected override string TestProjectName()
      {
         return "ProjectWithBBInheritance";
      }

      [Observation]
      public void should_create_all_expected_files()
      {
         CheckFilesExist(new[]
         {
            LogFile,
            $"{ReportConfigurationFileName}.json",
            @"Mefenamic_acid-Dapagliflozin-DDI\DDI_Control\DDI_Control.pkml",
            @"Mefenamic_acid-Dapagliflozin-DDI\DDI_Treatment\DDI_Treatment.pkml",
            @"Content\References.md",
            @"Content\images\GFME_equation.PNG",
            @"Intro\titlepage.md",
            @"ObservedData\DDI.csv",
            @"ObservedData\Obs_Control.csv",
            @"ObservedData\Obs_Treatment.csv",
            @"temp\Mefenamic_acid-Dapagliflozin-DDI\config.json",
            @"temp\Mefenamic_acid-Dapagliflozin-DDI\log.txt",
            @"temp\Mefenamic_acid-Dapagliflozin-DDI\mapping.json"
         });
      }

      [Observation]
      public void log_files_should_not_contain_errors_or_warnings()
      {
         CheckLogFilesDoNotContainErrorsOrWarnings();
      }

   }
}
