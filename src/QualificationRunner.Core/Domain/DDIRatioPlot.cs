using System.Collections.Generic;
using System.Linq;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class DDIRatioGroup
   {
      public DDIRatio[] DDIRatios { get; set; }
   }

   public class DDIRatio : IReferencingSimulations
   {
      public ReferencingSimulation SimulationControl { get; set; }
      public ReferencingSimulation SimulationDDI { get; set; }
      public IEnumerable<IReferencingSimulation> ReferencedSimulations => new[] {SimulationControl, SimulationDDI};
   }

   public class DDIRatioPlot : IReferencingSimulations
   {
      public DDIRatioGroup[] Groups { get; set; }

      public IEnumerable<IReferencingSimulation> ReferencedSimulations =>
         Groups?.SelectMany(x => x.DDIRatios)?.SelectMany(x => x.ReferencedSimulations);
   }
}