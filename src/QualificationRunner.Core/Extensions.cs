using OSPSuite.Core.Qualification;
using System.Collections.Generic;
using System.Linq;

namespace QualificationRunner.Core
{
   public static class StringExtensions
   {
      public static string SurroundWith(this string stringToSurround, string surroundString)
      {
         return $"{surroundString}{stringToSurround}{surroundString}";
      }

      public static string InQuotes(this string stringToSurround)
      {
         return stringToSurround.SurroundWith("\"");
      }
   }

   public static class QualificationConfigurationExtensions
   {
      /// <summary>
      /// Evaluates whether the specified qualification configuration contains elements 
      /// (such as simulations, inputs, or simulation plots) that require further processing.
      /// </summary>
      /// <param name="config">The qualification configuration to evaluate.</param>
      /// <returns>
      /// <c>true</c> if the configuration contains any simulations, inputs, or simulation plots; 
      /// otherwise, <c>false</c>.
      /// </returns>
      /// <remarks>
      /// This method is critical for determining which configurations should be included in 
      /// subsequent processing steps. For more details, refer to the GitHub issue: 
      /// <see href="https://github.com/Open-Systems-Pharmacology/QualificationRunner/issues/173" />.
      /// </remarks>
      public static bool MustBeExportedForFurtherProcessing(this QualifcationConfiguration config)
      {
         return config != null && 
            (hasAnyElements(config.Simulations) || hasAnyElements(config.Inputs) || hasAnyElements(config.SimulationPlots));
      }

      private static bool hasAnyElements<T>(IEnumerable<T> collection)
      {
         return collection != null && collection.Any();
      }
   }
}