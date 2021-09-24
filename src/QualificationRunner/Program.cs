using System;
using System.Diagnostics;
using CommandLine;
using Microsoft.Extensions.Logging;
using OSPSuite.Core.Services;
using OSPSuite.Infrastructure.Services;
using OSPSuite.Utility.Container;
using QualificationRunner.Bootstrap;
using QualificationRunner.Commands;
using QualificationRunner.Core.Domain;
using QualificationRunner.Core.Services;

namespace QualificationRunner
{
   class Program
   {
      static bool _valid = true;

      static int Main(string[] args)
      {
         ApplicationStartup.Initialize();

         Parser.Default.ParseArguments<QualificationRunCommand>(args)
            .WithParsed(startCommand)
            .WithNotParsed(err => _valid = false);

         if (Debugger.IsAttached)
            Console.ReadLine();

         if (!_valid)
            return (int) ExitCodes.Error;

         return (int) ExitCodes.Success;
      }

      private static void startCommand<TRunOptions>(CLICommand<TRunOptions> command)
      {
         var logger = initializeLogger(command);

         logger.AddInfo($"Starting {command.Name.ToLower()}");
         logger.AddDebug($"Arguments:\n{command}");

         var runner = IoC.Resolve<IBatchRunner<TRunOptions>>();

         try
         {
            runner.RunBatchAsync(command.ToRunOptions()).Wait();
            logger.AddInfo($"{command.Name} finished");
         }
         catch (Exception e)
         {
            logger.AddException(e);
            logger.AddError($"{command.Name} failed");
            _valid = false;
         } 
      }

      private static IOSPSuiteLogger initializeLogger(CLICommand runCommand)
      {
         var loggerCreator = IoC.Resolve<ILoggerCreator>();


         loggerCreator.AddLoggingBuilderConfiguration(x => x.SetMinimumLevel(runCommand.LogLevel).AddConsole());

         if (!string.IsNullOrEmpty(runCommand.LogFileFullPath))
            loggerCreator.AddLoggingBuilderConfiguration(builder => builder.AddFile(runCommand.LogFileFullPath, runCommand.LogLevel, shared: true));

         var logger = IoC.Resolve<IOSPSuiteLogger>();
         logger.DefaultCategoryName = "QualificationRunner";

         return logger;
      }
   }
}