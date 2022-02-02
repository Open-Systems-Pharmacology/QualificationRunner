using System.Collections.Generic;
using System.Linq;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class GOFGroup
   {
      public ReferencingSimulation[] OutputMappings { get; set; }
   }

   public class GOFMergedPlot : IReferencingSimulations
   {
      public GOFGroup[] Groups { get; set; }

      public IEnumerable<IReferencingSimulation> ReferencedSimulations => Groups?.SelectMany(x => x.OutputMappings);
   }
}