using System.Collections.Generic;
using QualificationRunner.Core.Domain.InstallationValidator.Core.Domain;

namespace QualificationRunner.Core.Services
{
   public interface ILogWatcherFactory
   {
      ILogWatcher CreateLogWatcher(string logFile, IEnumerable<string> additionalFoldersToWatch);
   }
}