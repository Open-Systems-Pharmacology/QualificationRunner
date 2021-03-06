﻿using System.IO;

namespace QualificationRunner.Core
{
   public static class Constants
   {
      public static readonly string ISSUE_TRACKER_URL = "https://github.com/Open-Systems-Pharmacology/QualificationRunner/issues";
      public static readonly string APPLICATION_FOLDER_PATH = @"Open Systems Pharmacology\QualificationRunner";
      public const string PRODUCT_NAME = "Qualification Runner";
      public static readonly string PRODUCT_NAME_WITH_TRADEMARK = $"{PRODUCT_NAME}®";


      public const string DEFAULT_REPORT_CONFIGURATION_PLAN_NAME = "report-configuration-plan";
      public const string CONTENT_FOLDER = "Content";
      public const string OBSERVED_DATA_FOLDER = "ObservedData";
      public const string INPUTS_FOLDER = "Inputs";
      public const string INTRO_FOLDER = "Intro";
      public const string TEMP_FOLDER = "temp";
      public const string PROJECT_DOWNLOAD_FOLDER = "_project";
      public const string OBSERVED_DATA_DOWNLOAD_FOLDER = "_observedData";
      public const string CONTENT_DOWNLOAD_FOLDER = "_content";
      public const string INTRODUCTION_DOWNLOAD_FOLDER = "_intro";

      public static class Tools
      {
         public static readonly string PKSIM_CLI = "PKSim.CLI.exe";
         public static readonly string BATCH_LOG = "batch.log";
      }

      public static class Configuration
      {
         public const string PROJECTS = "Projects";
         public const string SECTIONS = "Sections";
         public const string OBSERVED_DATA_SETS = "ObservedDataSets";
         public const string PLOTS = "Plots";
         public const string ALL_PLOTS = "AllPlots";
         public const string GOF_MERGED = "GOFMerged";
      }
   }
}