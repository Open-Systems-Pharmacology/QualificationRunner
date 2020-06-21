using System.Collections.Generic;
using System.Linq;
using QualificationRunner.Core.Domain;

namespace QualificationRunner.Core.Services
{
   public class LogWatcherOptions
   {
      public string LogFile { get; set; }
      public string Category { get; set; }
      public IEnumerable<string> AdditionalFoldersToWatch { get; set; } = Enumerable.Empty<string>();
      public string AdditionalFilesExtension { get; set; } = "*.*";
   }

   public interface ILogWatcherFactory
   {
      ILogWatcher CreateLogWatcher(LogWatcherOptions logWatcherOptions);
   }
}