using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Services
{
   public class QualificationRunResult : IReferencingProject
   {
      /// <summary>
      ///    Path of the log file associated only with the current run
      /// </summary>
      public string LogFilePath { get; set; }

      /// <summary>
      ///    Path of the config file associated with the rin
      /// </summary>
      public string ConfigFile { get; set; }

      /// <summary>
      ///    Name of the mapping file created as a result of the qualification run
      /// </summary>
      public string MappingFile { get; set; }

      /// <summary>
      ///    Was the run successful
      /// </summary>
      public bool Success { get; set; }

      public string Project { get; set; }
   }
}