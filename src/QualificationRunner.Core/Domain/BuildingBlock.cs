using OSPSuite.Core.Domain;

namespace QualificationRunner.Core.Domain
{
   public class BuildingBlock : IWithName
   {
      public PKSimBuildingBlockType BuildingBlockType { get; set; }
      public string Name { get; set; }
      public string RefProject { get; set; }
   }

   public class Plot
   {
      public string RefProject { get; set; }
      public string RefSimulation { get; set; }
      public int SectionId { get; set; }
   }
}