using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OSPSuite.Utility.Format;

namespace QualificationRunner.Core.Services
{
   public class NullabeDoubleJsonConverter : JsonConverter
   {
      private const int DOUBLE_PRECISION = 10;

      private readonly NumericFormatter<double> _doubleFormatter = new NumericFormatter<double>(new NumericFormatterOptions
      {
         AllowsScientificNotation = true,
         DecimalPlace = DOUBLE_PRECISION
      });

      public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
      {
         var d = (double?) value;
         if (!d.HasValue)
            return;

         var formatted = _doubleFormatter.Format(d.Value);

         double.TryParse(formatted, out double roundedFromString);

         writer.WriteValue(roundedFromString);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
      {
         //this will never be called for nullable as double reader will take precedence
         return 0;
      }

      public override bool CanRead => false;

      public override bool CanConvert(Type objectType)
      {
         return objectType == typeof(double?);
      }
   }

   public class WritablePropertiesOnlyResolver : DefaultContractResolver
   {
      protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
      {
         var props = base.CreateProperties(type, memberSerialization);
         return props.Where(p => p.Writable).ToList();
      }
   }

   public class QualificationRunnerJsonSerializerSetings : JsonSerializerSettings
   {
      public QualificationRunnerJsonSerializerSetings()
      {
         TypeNameHandling = TypeNameHandling.Auto;
         NullValueHandling = NullValueHandling.Ignore;
         ContractResolver = new WritablePropertiesOnlyResolver();
         Converters.Add(new StringEnumConverter());
         Converters.Add(new NullabeDoubleJsonConverter());
      }
   }

   public interface IJsonSerializer
   {
      Task Serialize(object objectToSerialize, string fileName);
      string SerializeAsString(object objectToSerialize);
      Task<object[]> DeserializeAsArray(string fileName, Type objectType);
      object[] DeserializeAsArrayFromString(string jsonString, Type objectType);
      Task<object> Deserialize(string fileName, Type objectType);
      Task<T> Deserialize<T>(string fileName) where T : class;
      object DeserializeFromString(string jsonString, Type objectType);
      T DeserializeFromString<T>(string jsonString) where T : class;
   }

   public class JsonSerializer : IJsonSerializer
   {
      private readonly JsonSerializerSettings _settings = new QualificationRunnerJsonSerializerSetings();

      public async Task Serialize(object objectToSerialize, string fileName)
      {
         var data = SerializeAsString(objectToSerialize);

         using (var sw = new StreamWriter(fileName))
         {
            await sw.WriteAsync(data);
         }
      }

      public string SerializeAsString(object objectToSerialize)
      {
         return JsonConvert.SerializeObject(objectToSerialize, Formatting.Indented, _settings);
      }

      public async Task<object[]> DeserializeAsArray(string fileName, Type objectType)
      {
         string json;
         using (var reader = new StreamReader(fileName))
         {
            json = await reader.ReadToEndAsync();
         }

         return deserializeAsArrayFromString(json, objectType);
      }

      public object[] DeserializeAsArrayFromString(string jsonString, Type objectType) => deserializeAsArrayFromString(jsonString, objectType);

      private object[] deserializeAsArrayFromString(string json, Type objectType)
      {
         var deserializedSnapshot = JsonConvert.DeserializeObject(json, _settings);

         switch (deserializedSnapshot)
         {
            case JObject jsonObject:
               return new[] {validatedObject(jsonObject, objectType)};

            case JArray array:
               return array.Select(x => validatedObject(x, objectType)).ToArray();
            default:
               return null;
         }
      }

      public async Task<object> Deserialize(string fileName, Type objectType)
      {
         var deserializedObjects = await DeserializeAsArray(fileName, objectType);
         return deserializedObjects.FirstOrDefault();
      }

      public async Task<T> Deserialize<T>(string fileName) where T : class
      {
         var deserializedObject = await Deserialize(fileName, typeof(T));
         return deserializedObject as T;
      }

      public object DeserializeFromString(string jsonString, Type objectType)
      {
         var deserializedObjects = DeserializeAsArrayFromString(jsonString, objectType);
         return deserializedObjects.FirstOrDefault();
      }

      public T DeserializeFromString<T>(string jsonString) where T : class
      {
         var deserializedObject = DeserializeFromString(jsonString, typeof(T));
         return deserializedObject as T;
      }

      private object validatedObject(JToken jToken, Type snapshotType)
      {
         return jToken.ToObject(snapshotType);
      }

//      private JSchema validateSnapshot(Type snapshotType)
//      {
//         return _schemas.GetOrAdd(snapshotType, createSchemaForType);
//      }
//
//      private JSchema createSchemaForType(Type snapshotType)
//      {
//         var generator = new JSchemaGenerator { DefaultRequired = Required.Default };
//         generator.GenerationProviders.Add(new StringEnumGenerationProvider());
//         return generator.Generate(snapshotType);
//      }
   }
}