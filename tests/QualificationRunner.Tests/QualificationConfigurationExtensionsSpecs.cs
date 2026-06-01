using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using OSPSuite.Core.Qualification;
using QualificationRunner.Core;

namespace QualificationRunner.Tests
{
   public abstract class concern_for_QualificationConfigurationExtensions : ContextSpecification<QualifcationConfiguration>
   {
      protected override void Context()
      {
         sut = new QualifcationConfiguration();
      }

   }

   public class When_checking_if_a_configuration_must_be_exported_for_further_processing_and_no_references_exist : concern_for_QualificationConfigurationExtensions
   {
      protected override void Context()
      {
         base.Context();
         sut = new QualifcationConfiguration();
      }

      [Observation]
      public void should_not_be_exported()
      {
         sut.MustBeExportedForFurtherProcessing().ShouldBeFalse();
      }
   }

   public class When_checking_if_a_configuration_must_be_exported_for_further_processing_and_simulations_exist : concern_for_QualificationConfigurationExtensions
   {
      protected override void Context()
      {
         base.Context();
         sut = new QualifcationConfiguration
         {
            Simulations = new[] { "Simulation1" }
         };
      }

      [Observation]
      public void should_be_exported()
      {
         sut.MustBeExportedForFurtherProcessing().ShouldBeTrue();
      }
   }

   public class When_checking_if_a_configuration_must_be_exported_for_further_processing_and_inputs_exist : concern_for_QualificationConfigurationExtensions
   {
      protected override void Context()
      {
         base.Context();
         sut = new QualifcationConfiguration
         {
            Inputs = new[] { new Input() }
         };
      }

      [Observation]
      public void should_be_exported()
      {
         sut.MustBeExportedForFurtherProcessing().ShouldBeTrue();
      }
   }

   public class When_checking_if_a_configuration_must_be_exported_for_further_processing_and_simulation_plots_exist : concern_for_QualificationConfigurationExtensions
   {
      protected override void Context()
      {
         base.Context();
         sut = new QualifcationConfiguration
         {
            SimulationPlots = new[] { new SimulationPlot() }
         };
      }

      [Observation]
      public void should_be_exported()
      {
         sut.MustBeExportedForFurtherProcessing().ShouldBeTrue();
      }
   }

}
