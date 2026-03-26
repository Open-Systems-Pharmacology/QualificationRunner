using System.IO;
using System.Threading.Tasks;
using OSPSuite.BDDHelper;
using OSPSuite.BDDHelper.Extensions;
using QualificationRunner.Core.Services;

namespace QualificationRunner.Tests.Services
{
   public abstract class concern_for_JsonSerializer : ContextSpecification<JsonSerializer>
   {
      protected string _tempFolder;
      protected string _filePath;

      protected override void Context()
      {
         sut = new JsonSerializer();
         _tempFolder = Path.Combine(Path.GetTempPath(), "JsonSerializerSpecs", Path.GetRandomFileName());
         Directory.CreateDirectory(_tempFolder);
         _filePath = Path.Combine(_tempFolder, "test.json");
      }

      public override void Cleanup()
      {
         if (Directory.Exists(_tempFolder))
            Directory.Delete(_tempFolder, true);

         base.Cleanup();
      }
   }

   public class When_serializing_an_object_with_writable_and_readonly_properties : concern_for_JsonSerializer
   {
      private string _json;

      [Observation]
      public void should_only_serialize_writable_properties()
      {
         var dto = new TestDto { Name = "test", Value = 5 };

         _json = sut.SerializeAsString(dto);

         _json.Contains("\"Name\"").ShouldBeTrue();
         _json.Contains("\"Value\"").ShouldBeTrue();
         _json.Contains("\"ReadOnlyValue\"").ShouldBeFalse();
      }
   }

   public class When_serializing_nullable_double_values : concern_for_JsonSerializer
   {
      [Observation]
      public void should_ignore_null_nullable_double_value()
      {
         var dto = new NullableDoubleDto { Number = null };

         var json = sut.SerializeAsString(dto);

         json.Contains("\"Number\"").ShouldBeFalse();
      }
   }

   public class When_deserializing_from_a_json_object_string : concern_for_JsonSerializer
   {
      [Observation]
      public void should_return_the_deserialized_instance()
      {
         const string json = "{\"Name\":\"abc\",\"Value\":10}";

         var result = sut.DeserializeFromString<TestDto>(json);

         result.ShouldNotBeNull();
         result.Name.ShouldBeEqualTo("abc");
         result.Value.ShouldBeEqualTo(10);
      }
   }

   public class When_deserializing_from_a_json_array_string : concern_for_JsonSerializer
   {
      [Observation]
      public void should_return_all_items_for_deserialize_as_array_from_string()
      {
         const string json = "[{\"Name\":\"a\",\"Value\":1},{\"Name\":\"b\",\"Value\":2}]";

         var result = sut.DeserializeAsArrayFromString(json, typeof(TestDto));

         result.ShouldNotBeNull();
         result.Length.ShouldBeEqualTo(2);
         ((TestDto)result[0]).Name.ShouldBeEqualTo("a");
         ((TestDto)result[1]).Name.ShouldBeEqualTo("b");
      }

      [Observation]
      public void should_return_the_first_item_for_generic_deserialize_from_string()
      {
         const string json = "[{\"Name\":\"first\",\"Value\":100},{\"Name\":\"second\",\"Value\":200}]";

         var result = sut.DeserializeFromString<TestDto>(json);

         result.ShouldNotBeNull();
         result.Name.ShouldBeEqualTo("first");
         result.Value.ShouldBeEqualTo(100);
      }
   }

   public class When_serializing_and_deserializing_using_files : concern_for_JsonSerializer
   {
      [Observation]
      public async Task should_serialize_to_file_and_deserialize_from_file()
      {
         var dto = new TestDto { Name = "file", Value = 42 };

         await sut.Serialize(dto, _filePath);
         var fromFile = await sut.Deserialize<TestDto>(_filePath);

         fromFile.ShouldNotBeNull();
         fromFile.Name.ShouldBeEqualTo("file");
         fromFile.Value.ShouldBeEqualTo(42);
      }

      [Observation]
      public async Task should_deserialize_array_from_file()
      {
         var json = "[{\"Name\":\"x\",\"Value\":1},{\"Name\":\"y\",\"Value\":2}]";
         File.WriteAllText(_filePath, json);

         var objects = await sut.DeserializeAsArray(_filePath, typeof(TestDto));

         objects.ShouldNotBeNull();
         objects.Length.ShouldBeEqualTo(2);
         ((TestDto)objects[0]).Name.ShouldBeEqualTo("x");
      }
   }

   public class When_deserializing_invalid_json_content : concern_for_JsonSerializer
   {
      [Observation]
      public void should_return_null_for_non_object_and_non_array_payload()
      {
         var result = sut.DeserializeAsArrayFromString("123", typeof(TestDto));

         result.ShouldBeNull();
      }
   }

   internal class TestDto
   {
      public string Name { get; set; }
      public int Value { get; set; }
      public string ReadOnlyValue => "readonly";
   }

   internal class NullableDoubleDto
   {
      public double? Number { get; set; }
   }
}
