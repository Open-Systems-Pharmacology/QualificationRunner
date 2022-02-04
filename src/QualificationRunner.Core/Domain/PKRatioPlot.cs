using System.Collections.Generic;
using System.Linq;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class PKRatioGroup
   {
      public ReferencingSimulation[] PKRatios { get; set; }
   }

   public class PKRatioPlot : IReferencingSimulations
   {
      public PKRatioGroup[] Groups { get; set; }

      public IEnumerable<IReferencingSimulation> ReferencedSimulations => Groups?.SelectMany(x => x.PKRatios);
   }
}