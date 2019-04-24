using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class SimulationParameterRef : IReferencingSimulation
   {
      public string Path { get; set; }
      public string[] TargetSimulations { get; set; }
      public string Project { get; set; }
      public string Simulation { get; set; }
   }
}