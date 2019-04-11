using System.Text;
using CommandLine;
using OSPSuite.Utility;
using QualificationRunner.Core;
using QualificationRunner.Core.RunOptions;

namespace QualificationRunner.Commands
{
   [Verb("qualification", HelpText = "Start qualification run workflow")]
   public class QualificationRunCommand : CLICommand<QualificationRunOptions>
   {
      public override string Name { get; } = "Qualification";

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
         };
      }

      public override string ToString()
      {
         var sb = new StringBuilder();
         LogDefaultOptions(sb);

         sb.AppendLine($"Configuration file: {ConfigurationFile}");
         sb.AppendLine($"Output folder: {OutputFolder}");

         return sb.ToString();
      }
   }
}