using System.Collections.Generic;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public interface IReferencingSimulations
   {
      IEnumerable<IReferencingSimulation> ReferencedSimulations { get; }
   }
}