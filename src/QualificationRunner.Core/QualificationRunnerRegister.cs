using Castle.Facilities.TypedFactory;
using Microsoft.Extensions.Logging;
using OSPSuite.Core;
using OSPSuite.Core.Services;
using OSPSuite.Infrastructure.Container.Castle;
using OSPSuite.Utility.Container;
using OSPSuite.Utility.Extensions;
using QualificationRunner.Core.RunOptions;
using QualificationRunner.Core.Services;
using ILogger = OSPSuite.Core.Services.ILogger;

namespace QualificationRunner.Core
{
   public class QualificationRunnerRegister : IRegister
   {
      public void RegisterInContainer(IContainer container)
      {
         registerCoreDependencies(container);

         container.AddScanner(x =>
         {
            x.AssemblyContainingType<QualificationRunnerRegister>();
            x.ExcludeType<QualificationRunnerConfiguration>();
            x.ExcludeType<QualificationRunnerLogger>();
            x.WithConvention(new OSPSuiteRegistrationConvention(registerConcreteType: true));
            x.RegisterAs(LifeStyle.Transient);
         });

         container.Register<IBatchRunner<QualificationRunOptions>, Services.QualificationRunner>();

         container.RegisterFactory<IQualificationEngineFactory>();
         container.RegisterFactory<IStartableProcessFactory>();
         container.RegisterFactory<ILogWatcherFactory>();
      }


      private void registerCoreDependencies(IContainer container)
      {
//         container.Register<ICompression, SharpLibCompression>();
//         container.Register<IStringCompression, StringCompression>();
//
//         container.Register<IUnitSystemXmlSerializerRepository, UnitSystemXmlSerializerRepository>(LifeStyle.Singleton);
//         container.Resolve<IUnitSystemXmlSerializerRepository>().PerformMapping();
//         container.Register<IDimensionFactoryPersistor, DimensionFactoryPersistor>();
//
//         container.Register<IExceptionManager, ExceptionManager>(LifeStyle.Singleton);
//         container.Register<IEventPublisher, EventPublisher>(LifeStyle.Singleton);
//         container.Register<DirectoryMapSettings, DirectoryMapSettings>(LifeStyle.Singleton);
         container.Register<StartableProcess, StartableProcess>();
      }

      private static void registerLogging(IContainer container)
      {
         var loggerFactory = new LoggerFactory();
         container.RegisterImplementationOf((ILoggerFactory) loggerFactory);
         container.Register<ILogger, QualificationRunnerLogger>(LifeStyle.Singleton);
      }

      public static IContainer Initialize()
      {
         var container = new CastleWindsorContainer();
         IoC.InitializeWith(container);
         container.WindsorContainer.AddFacility<TypedFactoryFacility>();
         container.WindsorContainer.AddFacility<EventRegisterFacility>();

         container.RegisterImplementationOf(container.DowncastTo<IContainer>());
         container.Register<IApplicationConfiguration, IQualificationRunnerConfiguration, QualificationRunnerConfiguration>(LifeStyle.Singleton);

         registerLogging(container);

         return container;
      }
   }
}