using System.Threading;
using OSPSuite.Utility.Container;
using OSPSuite.Utility.Format;
using QualificationRunner.Core;

namespace QualificationRunner.Bootstrap
{
   public class ApplicationStartup
   {
      public static void Initialize()
      {
         new ApplicationStartup().InitializeForStartup();
      }

      public void InitializeForStartup()
      {
         var container = QualificationRunnerRegister.Initialize();
         registerAllInContainer(container);
         NumericFormatterOptions.Instance.DecimalPlace = 2;
      }

      private void registerAllInContainer(IContainer container)
      {
         using (container.OptimizeDependencyResolution())
         {
            container.RegisterImplementationOf(new SynchronizationContext());
            container.AddRegister(x => x.FromType<QualificationRunnerRegister>());
         }
      }
   }
}