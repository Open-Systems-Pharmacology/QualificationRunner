using OSPSuite.BDDHelper;
using OSPSuite.Utility.Container;
using QualificationRunner.Core;
using System;
using System.IO;
using System.Threading;

namespace QualificationRunner.IntegrationTests
{
   [IntegrationTests]
   public abstract class ContextForIntegration<T> : ContextSpecification<T>
   {
      public override void GlobalContext()
      {
         if (IoC.Container == null)
         {
            var container = QualificationRunnerRegister.Initialize();

            using (container.OptimizeDependencyResolution())
            {
               container.RegisterImplementationOf(new SynchronizationContext());
               container.AddRegister(x => x.FromType<QualificationRunnerRegister>());
            }
         }

         sut = IoC.Resolve<T>();
      }

      protected static string TestDataFolder => 
         Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\TestData");
   }
}
