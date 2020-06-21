using System.IO;
using Microsoft.Extensions.Logging;

namespace QualificationRunner.Core.RunOptions
{
   public class QualificationRunOptions
   {
      public string ConfigurationFile { get; set; }
      public string ConfigurationFolder { get; set; }
      public string OutputFolder { get; set; }
      public string ReportConfigurationFileName { get; set; }
      public string PKSimInstallationFolder { get; set; }
      public bool ForceDelete { get; set; }
      public LogLevel LogLevel { get; set; }
      public string LogFile { get; set; }

      /// <summary>
      /// This is the ObservedData folder that will be created in the output folder
      /// </summary>
      public string ObservedDataFolder => Path.Combine(OutputFolder, Constants.OBSERVED_DATA_FOLDER);

      /// <summary>
      /// This is the Input folder that will be created in the output folder
      /// </summary>
      public string InputsFolder => Path.Combine(OutputFolder, Constants.INPUTS_FOLDER);

      /// <summary>
      /// This is the Content folder that will be created in the output folder
      /// </summary>
      public string ContentFolder => Path.Combine(OutputFolder, Constants.CONTENT_FOLDER);

      /// <summary>
      /// This is the Intro folder that will be created in the output folder
      /// </summary>
      public string IntroFolder => Path.Combine(OutputFolder, Constants.INTRO_FOLDER);

      public string ReportConfigurationFile => Path.Combine(OutputFolder, $"{ReportConfigurationFileName}.json");

      public string TempFolder => Path.Combine(OutputFolder, Constants.TEMP_FOLDER);
   }
}