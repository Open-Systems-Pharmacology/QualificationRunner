using System;
using OSPSuite.Assets;
using OSPSuite.Core;
using OSPSuite.Core.Domain;
using OSPSuite.Infrastructure.Configuration;

namespace QualificationRunner.Core
{
   public interface IQualificationRunnerConfiguration : IApplicationConfiguration
   {
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
   }
}