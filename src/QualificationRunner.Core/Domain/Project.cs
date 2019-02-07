using OSPSuite.Core.Domain;

namespace QualificationRunner.Core.Domain
{
   public class Project : IWithId
   {
      public string Id { get; set; }
      public string Path { get; set; }
      public BuildingBlock[] BuildingBlocks { get; set; }
   }
}