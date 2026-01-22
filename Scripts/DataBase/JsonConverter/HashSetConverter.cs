using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

public class HashSetConverter<T> : JsonConverter<HashSet<T>>
{
    public override HashSet<T> ReadJson(JsonReader reader, Type objectType, HashSet<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var result = new HashSet<T>();
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
                    UnityEngine.Debug.LogWarning($"[WARN] HashSetConverter::ReadJson - Failed to parse item: {ex.Message}");
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[WARN] HashSetConverter::ReadJson - Failed to parse list: {ex.Message}");
            return new HashSet<T>();
        }
    }

    public override void WriteJson(JsonWriter writer, HashSet<T> value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value ?? new HashSet<T>());
    }
}