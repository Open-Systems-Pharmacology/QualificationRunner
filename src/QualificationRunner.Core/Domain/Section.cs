using OSPSuite.Core.Domain;

namespace QualificationRunner.Core.Domain
{
   public class Section : IWithId
   {
      public string Id { get; set; }
      public string Title { get; set; }
      public string Content { get; set; }
      public Section[] Sections { get; set; }
   }
}