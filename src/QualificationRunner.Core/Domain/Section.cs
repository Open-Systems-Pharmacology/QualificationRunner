namespace QualificationRunner.Core.Domain
{
   public class Section
   {
      public int Id { get; set; }
      public string Title { get; set; }
      public string Content { get; set; }
      public Section[] Sections { get; set; }
   }
}