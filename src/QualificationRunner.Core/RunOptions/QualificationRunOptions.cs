using System.IO;
using Microsoft.Extensions.Logging;

namespace QualificationRunner.Core.RunOptions
{
   public class QualificationRunOptions
   {
      public string ConfigurationFile { get; set; }
      public string OutputFolder { get; set; }
      public string ReportConfigurationFileName { get; set; }
      public bool ForceDelete { get; set; }
      public LogLevel LogLevel { get; set; }
      public string LogFile { get; set; }


      public string ObservedDataFolder => Path.Combine(OutputFolder, Constants.OBSERVED_DATA_FOLDER);

      public string ReportConfigurationFile => Path.Combine(OutputFolder, $"{ReportConfigurationFileName}.json");
   }
}