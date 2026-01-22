using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using UnityEngine;


public class Vector2IntConverter : JsonConverter<Vector2Int>
{
    public override Vector2Int ReadJson(JsonReader reader, System.Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var arr = serializer.Deserialize<int[]>(reader);
        return new Vector2Int(arr[0], arr[1]);
    }

    public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, new int[] { value.x, value.y });
    }
}