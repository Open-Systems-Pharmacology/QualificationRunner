using System;
using System.IO;
using Microsoft.Win32;
using OSPSuite.Assets;
using OSPSuite.Core;
using OSPSuite.Core.Domain;
using OSPSuite.Core.Qualification;
using OSPSuite.Infrastructure.Configuration;
using QualificationRunner.Core.Assets;

namespace QualificationRunner.Core
{
   public interface IQualificationRunnerConfiguration : IApplicationConfiguration
   {
      string PKSimCLIPathFor(string pksimInstallationFolder);
   }

   public class QualificationRunnerConfiguration : OSPSuiteConfiguration, IQualificationRunnerConfiguration
   {
      public override string ProductName => Constants.PRODUCT_NAME_WITH_TRADEMARK;

      //not used in this context
      public override int InternalVersion { get; } = 1;
      public override Origin Product { get; } = Origins.Other;
      public override string ProductNameWithTrademark => Constants.PRODUCT_NAME_WITH_TRADEMARK;
      public override ApplicationIcon Icon { get; } = ApplicationIcons.Comparison;
      public override string UserSettingsFileName { get; } = "UserSettings.xml";
      public override string ApplicationSettingsFileName { get; } = "ApplicationSettings.xml";
      public override string IssueTrackerUrl { get; } = Constants.ISSUE_TRACKER_URL;
      protected override string[] LatestVersionWithOtherMajor { get; } = new String[0];
      public override string WatermarkOptionLocation { get; } = "Options -> Settings -> Application";
      public override string ApplicationFolderPathName { get; } = Constants.APPLICATION_FOLDER_PATH;

      public string PKSimCLIPathFor(string pksimInstallationFolder)
      {
         return getPKSimCLIPathFor(string.IsNullOrEmpty(pksimInstallationFolder) ? retrievePKSimInstallFolderPathFromRegistry() : pksimInstallationFolder);
      }

      private string retrievePKSimInstallFolderPathFromRegistry() => getRegistryValueForRegistryPathAndKey(OSPSuite.Core.Domain.Constants.RegistryPaths.PKSIM_REG_PATH, OSPSuite.Core.Domain.Constants.RegistryPaths.INSTALL_DIR);

      private string getPKSimCLIPathFor(string pksimInstallationFolder)
      {
         if (string.IsNullOrEmpty(pksimInstallationFolder))
            throw new QualificationRunException(Errors.PKSimInstallationFolderNotFound);

         return Path.Combine(pksimInstallationFolder, Constants.Tools.PKSIM_CLI);
      }

      private string getRegistryValueForRegistryPathAndKey(string openSystemsPharmacology, string installDir)
      {
         try
         {
            return (string) Registry.GetValue($@"HKEY_LOCAL_MACHINE\SOFTWARE\{openSystemsPharmacology}{Version}", installDir, null);
         }
         catch (Exception)
         {
            return string.Empty;
         }
      }
   }
}