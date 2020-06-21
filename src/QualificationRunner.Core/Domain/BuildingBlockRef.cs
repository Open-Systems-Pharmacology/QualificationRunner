using OSPSuite.Core.Domain;
using OSPSuite.Core.Qualification;

namespace QualificationRunner.Core.Domain
{
   public class BuildingBlockRef : IWithName, IReferencingProject
   {
      public PKSimBuildingBlockType Type { get; set; }
      public string Name { get; set; }
      public string Project { get; set; }
      public string Path { get; set; }
   }
}