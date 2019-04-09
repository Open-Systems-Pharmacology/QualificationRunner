using OSPSuite.Core.Domain;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class Project : IWithId
   {
      public string Id { get; set; }
      public string Path { get; set; }
      public BuildingBlockRef[] BuildingBlocks { get; set; }
      public SimulationParameterRef[] SimulationParameters { get; set; }
   }
}