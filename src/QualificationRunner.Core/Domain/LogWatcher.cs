using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using OSPSuite.Utility;
using OSPSuite.Utility.Extensions;
using QualificationRunner.Core.Services;
using ILogger = OSPSuite.Core.Services.ILogger;

namespace QualificationRunner.Core.Domain
{
   namespace InstallationValidator.Core.Domain
   {
      public interface ILogWatcher : IDisposable
      {
         void Watch();
      }

      public class LogWatcher : ILogWatcher
      {
         private readonly ILogger _logger;
         private readonly LogWatcherOptions _logWatcherOptions;
         private bool _disposed;
         private readonly List<FileSystemWatcher> _fileSystemWatchers;
         private StreamReader _sr;
         private FileSystemWatcher _logFileWatcher;


         public LogWatcher(ILogger logger, LogWatcherOptions logWatcherOptions)
         {
            _logger = logger;
            _logWatcherOptions = logWatcherOptions;
            _fileSystemWatchers = new List<FileSystemWatcher>();
            configureFileSystemWatcher(_logWatcherOptions.LogFile);

            subscribe();
         }

         private void configureFileSystemWatcher(string logFile)
         {
            var folderFromFileFullPath = FileHelper.FolderFromFileFullPath(logFile);
            var fileNameEndExtension = Path.GetFileName(_logWatcherOptions.LogFile);

            FileHelper.DeleteFile(logFile);

            _logFileWatcher = new FileSystemWatcher(string.IsNullOrEmpty(folderFromFileFullPath) ? "./" : folderFromFileFullPath, fileNameEndExtension)
            {
               NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.Size
            };

            _fileSystemWatchers.Add(_logFileWatcher);

            _logWatcherOptions.AdditionalFoldersToWatch.Each(folder => { _fileSystemWatchers.Add(new FileSystemWatcher(folder, _logWatcherOptions.AdditionalFilesExtension)); });
         }

         private void forAllWatchers(Action<FileSystemWatcher> actionForFileSystemWatcher)
         {
            _fileSystemWatchers.Each(actionForFileSystemWatcher);
         }

         private void subscribe()
         {
            _logFileWatcher.Created += onCreated;
            forAllWatchers(watcher => { watcher.Changed += onChanged; });
         }

         private void unsubscribe()
         {
            _logFileWatcher.Created -= onCreated;
            forAllWatchers(watcher => { watcher.Changed -= onChanged; });
         }

         private void onCreated(object sender, FileSystemEventArgs e)
         {
            var logFileStream = new FileStream(_logWatcherOptions.LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            _sr = new StreamReader(logFileStream);

            appendTextToLog();
         }

         private string readFileText()
         {
            return _sr.ReadToEnd();
         }

         private void onChanged(object sender, FileSystemEventArgs e)
         {
            if (logFileCreated())
               appendTextToLog();
         }

         private bool logFileCreated()
         {
            return _sr != null;
         }

         private void appendTextToLog()
         {
            var entries = readFileText()?.Split('\n');
            entries.Each(s =>
            {
               var (entry, level) = logLevelFor(s);
               _logger.AddToLog(entry, level, _logWatcherOptions.Category);
            });
         }

         private (string, LogLevel) logLevelFor(string entry)
         {
            if (string.IsNullOrEmpty(entry))
               return (entry, LogLevel.Debug);

            if (entry.StartsWith("Debug:"))
               return (entry.Replace("Debug: ", ""), LogLevel.Debug);

            if (entry.StartsWith("Information:"))
               return (entry.Replace("Information: ", ""), LogLevel.Information);

            if (entry.StartsWith("Error"))
               return (entry.Replace("Error: ", ""), LogLevel.Error);

            if (entry.StartsWith("Warning"))
               return (entry.Replace("Warning: ", ""), LogLevel.Warning);

            return (entry, LogLevel.Information);
         }

         public virtual void Watch()
         {
            forAllWatchers(watcher => watcher.EnableRaisingEvents = true);
         }

         public void Dispose()
         {
            if (_disposed) return;

            Cleanup();
            GC.SuppressFinalize(this);
            _disposed = true;
         }

         ~LogWatcher()
         {
            Cleanup();
         }

         protected virtual void Cleanup()
         {
            unsubscribe();
            forAllWatchers(watcher => watcher.Dispose());
            _sr?.Close();
            _sr?.Dispose();
         }
      }
   }
}