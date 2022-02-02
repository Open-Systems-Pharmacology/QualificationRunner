using System;
using System.Collections.Generic;
using System.Linq;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class Plots
   {
      public SimulationPlot[] AllPlots { get; set; }
      public GOFMergedPlot[] GOFMergedPlots { get; set; }
      public ComparisonTimeProfilePlot[] ComparisonTimeProfilePlots { get; set; }
      public DDIRatioPlot[] DDIRatioPlots { get; set; }
      public PKRatioPlot[] PKRatioPlots { get; set; }


      public string[] ReferencedSimulations(string project)
      {

         var simulations = new List<string>();
         simulations.AddRange(namesFrom(AllPlots, project));
         simulations.AddRange(namesFrom(GOFMergedPlots, project)); 
         simulations.AddRange(namesFrom(ComparisonTimeProfilePlots, project));
         simulations.AddRange(namesFrom(DDIRatioPlots, project));
         simulations.AddRange(namesFrom(PKRatioPlots, project));
         return simulations.Distinct().ToArray();
      }

      private IEnumerable<string> namesFrom(IEnumerable<IReferencingSimulations> referencingSimulationsList, string project) =>
         namesFrom(referencingSimulationsList?.SelectMany(x => x.ReferencedSimulations), project);

      private IEnumerable<string> namesFrom(IEnumerable<IReferencingSimulation> referencingSimulations, string project) =>
         referencingSimulations
            ?.Where(x => string.Equals(x.Project, project))
            ?.Select(x => x.Simulation) ?? Array.Empty<string>();
   }
}