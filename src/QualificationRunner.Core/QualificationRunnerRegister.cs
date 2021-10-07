using Castle.Facilities.TypedFactory;
using Microsoft.Extensions.Logging;
using OSPSuite.Core;
using OSPSuite.Core.Services;
using OSPSuite.Infrastructure;
using OSPSuite.Infrastructure.Container.Castle;
using OSPSuite.Utility.Container;
using OSPSuite.Utility.Extensions;
using QualificationRunner.Core.RunOptions;
using QualificationRunner.Core.Services;

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
            x.WithConvention(new OSPSuiteRegistrationConvention(registerConcreteType: true));
            x.RegisterAs(LifeStyle.Transient);
         });

         container.Register<IBatchRunner<QualificationRunOptions>, Services.QualificationRunner>();

         container.RegisterFactory<IQualificationEngineFactory>();
         container.RegisterFactory<IStartableProcessFactory>();
      }


      private void registerCoreDependencies(IContainer container)
      {
       container.Register<StartableProcess, StartableProcess>();
      }

      public static IContainer Initialize()
      {
         var container = new CastleWindsorContainer();
         IoC.InitializeWith(container);
         container.WindsorContainer.AddFacility<TypedFactoryFacility>();
         container.WindsorContainer.AddFacility<EventRegisterFacility>();

         container.RegisterImplementationOf(container.DowncastTo<IContainer>());
         container.Register<IApplicationConfiguration, IQualificationRunnerConfiguration, QualificationRunnerConfiguration>(LifeStyle.Singleton);

         container.AddRegister(x => x.FromType<InfrastructureRegister>());

         return container;
      }
   }
}