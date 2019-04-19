using OSPSuite.Core.Domain;

namespace QualificationRunner.Core.Domain
{
   public class Project : IWithId
   {
      public string Id { get; set; }
      //Path of project. Either local path relative to qualification plan or a url
      public string Path { get; set; }
      public BuildingBlockRef[] BuildingBlocks { get; set; }
      public SimulationParameterRef[] SimulationParameters { get; set; }
      public string SnapshotFilePath { get; set; }
   }
}