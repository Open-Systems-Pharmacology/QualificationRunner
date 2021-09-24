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
      public static string PKSimInstallationFolderNotFound = $"{Tools.PKSIM_CLI} installation folder was not found. Ensure that PK-Sim was installed with the setup OR specify the --pksim option";

      public static string PKSimCLIFileNotFound(string pksimCLIPath) => $"'{pksimCLIPath}' does not exist on your computer.";

      public static string NodeNotDefinedInQualificationFile(string node) => $"{node} node not defined in qualification file";

      public static string ProjectsNotDefinedInQualificationFile = NodeNotDefinedInQualificationFile(Configuration.PROJECTS);

      public static string OutputFolderIsNotEmpty = "Output folder is not empty. Please use -f to force deletion. Beware: All files in the output folder will be deleted!";

      public static string ReferencedProjectNotDefinedInQualificationFile(string project) => $"Referenced project '{project}' is not defined in qualification file";

      public static string ProjectConfigurationNotValid(string project, string logFile) => $"Project configuration for '{project}' is invalid. Please check one of the log files at '{logFile}' for details.";

      public static string ObservedDataFileNotFound(string observedDataFilePath) => $"Observed data file '{observedDataFilePath}' does not exist.";

      public static string ContentFileNotFound(string contentFilePath) => $"content file '{contentFilePath}' does not exist.";

      public static string IntroductionFileNotFound(string introductionFilePath) => $"Introduction file '{introductionFilePath}' does not exist.";

      public static string SnapshotFileNotFound(string snapshotFilePath) => $"Snapshot file '{snapshotFilePath}' does not exist.";

      public static string ConfigurationFileNotFound(string configurationFilePath) => $"Configuration file '{configurationFilePath}' does not exist.";
   }
}