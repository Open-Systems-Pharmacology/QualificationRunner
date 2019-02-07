using System.Text;
using CommandLine;
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

      [Option('o', "output", Required = true, HelpText = "Output folder where the qualifaction workflow files will be created.")]
      public string OutputFolder { get; set; }

      [Option('f', "force", Required = false, HelpText = "Set to true, the output folder will be deleted, even if it not empty. Default is false")]
      public bool ForceDelete { get; set; }

      [Option('n', "name", Required = false, HelpText = "Name of the report qualification plan to be generated")]
      public string ReportConfigurationFileName { get; set; } = Constants.DEFAULT_REPORT_CONFIGURATION_PLAN_NAME;

      public override QualificationRunOptions ToRunOptions()
      {
         return new QualificationRunOptions
         {
            ConfigurationFile = ConfigurationFile,
            OutputFolder = OutputFolder,
            ForceDelete = ForceDelete,
            ReportConfigurationFileName = ReportConfigurationFileName,
            LogLevel = LogLevel,
            LogFileFullPath = LogFileFullPath
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