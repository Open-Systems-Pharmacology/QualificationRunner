using System.IO;

namespace QualificationRunner.Core
{
   public static class Constants
   {
      public static readonly string ISSUE_TRACKER_URL = "https://github.com/Open-Systems-Pharmacology/QualificationRunner/issues";
      public static readonly string APPLICATION_FOLDER_PATH = @"Open Systems Pharmacology\QualificationRunner";
      public const string PRODUCT_NAME = "Qualification Runner";
      public static readonly string PRODUCT_NAME_WITH_TRADEMARK = $"{PRODUCT_NAME}®";

      public static class Tools
      {
         public static readonly string PKSIM_CLI = "PKSim.CLI.exe";
         public static readonly string BATCH_INPUTS = Path.Combine("Inputs", "BatchFiles");
         public static readonly string BATCH_OUTPUTS = Path.Combine("Outputs", "BatchFiles");
         public static readonly string CALCULATION_OUTPUTS = "Outputs";
         public static readonly string BATCH_LOG = "batch.log";
      }
   }
}