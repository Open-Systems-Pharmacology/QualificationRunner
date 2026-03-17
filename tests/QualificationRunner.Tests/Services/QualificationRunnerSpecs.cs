using System.IO;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using OSPSuite.Core.Qualification;
using OSPSuite.Core.Services;
using QualificationRunner.Core.RunOptions;
using QualificationRunner.Core.Services;

namespace QualificationRunner.Tests.Services
{
   public abstract class concern_for_QualificationRunner : ContextSpecification<QualificationRunner.Core.Services.QualificationRunner>
   {
      protected IJsonSerializer _jsonSerializer;
      protected IOSPSuiteLogger _logger;
      protected IQualificationEngineFactory _qualificationEngineFactory;

      protected override void Context()
      {
         _jsonSerializer = A.Fake<IJsonSerializer>();
         _logger = A.Fake<IOSPSuiteLogger>();
         _qualificationEngineFactory = A.Fake<IQualificationEngineFactory>();

         sut = new QualificationRunner.Core.Services.QualificationRunner(_jsonSerializer, _logger, _qualificationEngineFactory);
      }

   }

   public class When_running_a_batch_and_the_configuration_file_does_not_exist : concern_for_QualificationRunner
   {
      private QualificationRunOptions _runOptions;

      protected override void Context()
      {
         base.Context();
         _runOptions = new QualificationRunOptions
         {
            ConfigurationFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName(), "missing.json"),
            OutputFolder = Path.Combine(Path.GetTempPath(), "output")
         };
      }

      [Observation]
      public void should_throw_a_qualification_run_exception()
      {
         The.Action(async () => await sut.RunBatchAsync(_runOptions))
            .ShouldThrowAn<QualificationRunException>();
      }
   }

   public class When_removing_a_property_by_name_from_a_json_object : concern_for_QualificationRunner
   {
      [Observation]
      public void should_remove_the_property_when_it_exists()
      {
         var obj = JObject.Parse("{\"A\":1,\"B\":2}");

         sut.RemoveByName(obj, "A");

         (obj["A"] == null).ShouldBeTrue();
         (obj["B"] != null).ShouldBeTrue();
      }

      [Observation]
      public void should_do_nothing_when_property_does_not_exist()
      {
         var obj = JObject.Parse("{\"A\":1}");

         sut.RemoveByName(obj, "Unknown");

         (obj["A"] != null).ShouldBeTrue();
      }
   }

   public class When_getting_a_list_from_a_dynamic_enumerable : concern_for_QualificationRunner
   {
      [Observation]
      public void should_return_an_empty_list_when_enumerable_is_null()
      {
         var result = sut.GetListFrom<RunnerTestDto>(null);

         result.ShouldNotBeNull();
         result.Count.ShouldBeEqualTo(0);
      }
   }

   public class RunnerTestDto
   {
      public string Name { get; set; }
   }
}
