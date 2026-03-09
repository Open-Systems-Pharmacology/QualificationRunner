using System.Collections.Generic;
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

   public class When_casting_an_object_to_a_given_type : concern_for_QualificationRunner
   {
      [Observation]
      public void should_use_the_json_serializer_to_convert_the_object()
      {
         var source = new { Name = "source" };
         var expected = new RunnerTestDto { Name = "converted" };

         A.CallTo(() => _jsonSerializer.SerializeAsString(A<object>.Ignored)).Returns("json");
         A.CallTo(() => _jsonSerializer.DeserializeFromString(A<string>.Ignored, typeof(RunnerTestDto))).Returns(expected);

         var result = sut.Cast<RunnerTestDto>(source);

         result.ShouldBeEqualTo(expected);
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

      [Observation]
      public void should_convert_each_item_using_cast()
      {
         var item1 = new { Name = "first" };
         var item2 = new { Name = "second" };
         var enumerable = new List<object> { item1, item2 };

         A.CallTo(() => _jsonSerializer.SerializeAsString(A<object>.Ignored)).ReturnsNextFromSequence("json1", "json2");
         A.CallTo(() => _jsonSerializer.DeserializeFromString(A<string>.Ignored, typeof(RunnerTestDto)))
            .ReturnsNextFromSequence(new RunnerTestDto { Name = "one" }, new RunnerTestDto { Name = "two" });

         var result = sut.GetListFrom<RunnerTestDto>(enumerable);

         result.Count.ShouldBeEqualTo(2);
         result[0].Name.ShouldBeEqualTo("one");
         result[1].Name.ShouldBeEqualTo("two");
      }
   }

   public class RunnerTestDto
   {
      public string Name { get; set; }
   }
}
