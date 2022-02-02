using System.Collections.Generic;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class ComparisonTimeProfilePlot : IReferencingSimulations
   {
      public ReferencingSimulation[] OutputMappings { get; set; }

      public IEnumerable<IReferencingSimulation> ReferencedSimulations => OutputMappings;
   }
}