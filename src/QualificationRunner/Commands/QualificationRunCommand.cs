using System.Text;
using CommandLine;
using QualificationRunner.Core.RunOptions;

namespace QualificationRunner.Commands
{
   [Verb("qualification", HelpText = "Start qualification run workflow")]
   public class QualificationRunCommand : CLICommand<QualificationRunOptions>
   {
      public override string Name { get; } = "Qualification";
      
      [Option('f', "file", Required = true, HelpText = "Json configuration file used to start the qualification workflow.")]
      public string ConfigurationFile { get; set; }

      [Option('o', "output", Required = true, HelpText = "Output folder where the qualifaction workflow files will be created.")]
      public string OutputFolder { get; set; }

      public override QualificationRunOptions ToRunOptions()
      {
         return new QualificationRunOptions
         {
            ConfigurationFile = ConfigurationFile,
            OutputFolder = OutputFolder
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