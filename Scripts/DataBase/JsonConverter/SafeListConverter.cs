using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
public class SafeListConverter<T> : JsonConverter<List<T>>
{
    public override List<T> ReadJson(JsonReader reader, Type objectType, List<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var result = new List<T>();
            var arr = JArray.Load(reader);
            foreach (var token in arr)
            {
                try
                {
                    var item = token.ToObject<T>(serializer);
                    result.Add(item);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[WARN] SafeListConverter::ReadJson - Failed to parse item: {ex.Message}");
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[WARN] SafeListConverter::ReadJson - Failed to parse list: {ex.Message}");
            return new List<T>();
        }
    }
    public override void WriteJson(JsonWriter writer, List<T> value, JsonSerializer serializer) { serializer.Serialize(writer, value ?? new List<T>()); }
}