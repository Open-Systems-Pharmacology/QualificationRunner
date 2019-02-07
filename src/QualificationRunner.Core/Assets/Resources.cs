using static QualificationRunner.Core.Constants;

namespace QualificationRunner.Core.Assets
{
   public static class Logs
   {
      public static string StartingQualificationRunForProject(string projectId) => $"Starting qualification run for project '{projectId}'";
      public static string QualificationConfigurationForProjectExportedTo(string projectId, string file) => $"Qualification configuration for project '{projectId}' exported to '{file}'";
   }

   public static class Errors
   {
      public static string NodeNotDefinedInQualificationFile(string node) => $"{node} node not defined in qualification file";
      public static string ProjectsNotDefinedInQualificationFile = NodeNotDefinedInQualificationFile(Configuration.PROJECTS);

      public static string ReferencedProjectNotDefinedInQualificationFile(string project) => $"Referenced project '{project}' is not defined in qualification file";


      public static string ProjectConfigurationNotValid(string project, string logFile) => $"Project configuration for '{project}' is invalid. Please check log file at '{logFile}' for details.";

      public static string ObservedDataFileNotFound(string observedDataFilePath) => $"Observed data file '{observedDataFilePath}' does not exist.";
   }
}