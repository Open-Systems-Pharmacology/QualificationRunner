﻿using System.Text;
using CommandLine;
using OSPSuite.Utility;
using QualificationRunner.Core;
using QualificationRunner.Core.RunOptions;

namespace QualificationRunner.Commands
{
   [Verb("qualification", HelpText = "Start qualification workflow")]
   public class QualificationRunCommand : CLICommand<QualificationRunOptions>
   {
      public override string Name { get; } = "Qualification Workflow";

      [Option('i', "input", Required = true, HelpText = "Json configuration input file used to start the qualification workflow.")]
      public string ConfigurationFile { get; set; }

      [Option('o', "output", Required = true, HelpText = "Output folder where the qualification workflow files will be created.")]
      public string OutputFolder { get; set; }

      [Option('f', "force", Required = false, HelpText = "Optional. Set to true, the output folder will be deleted, even if it not empty. Default is false")]
      public bool ForceDelete { get; set; }

      [Option('n', "name", Required = false, HelpText = "Optional. Name of the report qualification plan to be generated")]
      public string ReportConfigurationFileName { get; set; } = Constants.DEFAULT_REPORT_CONFIGURATION_PLAN_NAME;

      [Option('p', "pksim", Required = false, HelpText = "Optional. Path of PK-Sim installation folder. If not specified, installation path will be read from registry (e.g required full install of PK-Sim via setup)")]
      public string PKSimInstallationFolder { get; set; }

      [Option("norun", Required = false, HelpText = "Should the qualification runner by pass running the simulation. Default is false (e.g. it will run the simulations)")]
      public bool NoRun { get; set; } = false;

      [Option('e', "exp", Required = false, HelpText = "Should the qualification runner also export the project files (snapshot and PK-Sim project file). Default is false")]
      public bool ExportProjectFiles { get; set; } = false;

      public override QualificationRunOptions ToRunOptions()
      {
         return new QualificationRunOptions
         {
            ConfigurationFile = ConfigurationFile,
            ConfigurationFolder = FileHelper.FolderFromFileFullPath(ConfigurationFile),
            OutputFolder = OutputFolder,
            ForceDelete = ForceDelete,
            ReportConfigurationFileName = ReportConfigurationFileName,
            PKSimInstallationFolder = PKSimInstallationFolder,
            LogLevel = LogLevel,
            LogFile = LogFileFullPath,
            //TODO switch to RUN when we move to R
            Run = !NoRun,
            ExportProjectFiles = ExportProjectFiles
         };
      }

      public override string ToString()
      {
         var sb = new StringBuilder();
         LogDefaultOptions(sb);

         sb.AppendLine($"Configuration file: {ConfigurationFile}");
         sb.AppendLine($"Output folder: {OutputFolder}");
         sb.AppendLine($"Run simulations: {!NoRun}");
         sb.AppendLine($"Export project files: {ExportProjectFiles}");

         return sb.ToString();
      }
   }
}