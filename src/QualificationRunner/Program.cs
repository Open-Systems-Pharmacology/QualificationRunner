using System;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.Logging;
using OSPSuite.Core.Services;
using OSPSuite.Infrastructure.Logging;
using OSPSuite.Utility.Container;
using QualificationRunner.Bootstrap;
using QualificationRunner.Commands;
using QualificationRunner.Core.Services;
using ILogger = OSPSuite.Core.Services.ILogger;

namespace QualificationRunner
{
   [Flags]
   enum ExitCodes
   {
      Success = 0,
      Error = 1 << 0,
   }

   class Program
   {
      static int Main(string[] args)
      {
         //starting batch tool with arguments
         var valid = true;

         ApplicationStartup.Initialize();

         Parser.Default.ParseArguments<QualificationRunCommand>(args)
            .WithParsed(startCommand)
            .WithNotParsed(err => valid = false);

         Thread.Sleep(1000);
         if (!valid)
            return (int) ExitCodes.Error;

         return (int) ExitCodes.Success;

      }

      private static void startCommand<TRunOptions>(CLICommand<TRunOptions> command)
      {
         var logger = initializeLogger(command);

         logger.AddInfo($"Starting {command.Name.ToLower()} run with arguments:\n{command}");
         var runner = IoC.Resolve<IBatchRunner<TRunOptions>>();
         try
         {
            runner.RunBatchAsync(command.ToRunOptions()).Wait();
         }
         catch (Exception e)
         {
            logger.AddException(e);
         }

         logger.AddInfo($"{command.Name} run finished");
      }

      private static ILogger initializeLogger(CLICommand runCommand)
      {
         var loggerFactory = IoC.Resolve<ILoggerFactory>();

         loggerFactory
            .AddConsole(runCommand.LogLevel);

         if (!string.IsNullOrEmpty(runCommand.LogFileFullPath))
            loggerFactory.AddFile(runCommand.LogFileFullPath, runCommand.LogLevel, runCommand.AppendToLog);

         return IoC.Resolve<ILogger>();
      }
   }
}