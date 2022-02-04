using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class ReferencingSimulation : IReferencingSimulation
   {
      public string Project { get; set; }
      public string Simulation { get; set; }
   }
}