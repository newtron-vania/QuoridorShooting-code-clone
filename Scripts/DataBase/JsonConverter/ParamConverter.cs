using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System;

public class ParamJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(Param);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // 기대 형태: ["int", 3] 또는 ["SpecialEffectType", "Burnt"]
        var token = JToken.Load(reader);

        if (token.Type == JTokenType.Array)
        {
            var arr = (JArray)token;
            if (arr.Count != 2)
                throw new JsonSerializationException("Param must be an array of [typeName, value].");

            var typeName = arr[0].Type == JTokenType.String ? arr[0].Value<string>() : arr[0].ToString();
            var value = arr[1]; // 그대로 Raw로 보관 (JToken)

            return new Param
            {
                TypeName = typeName,
                Raw = value
            };
        }

        // 방어: {"TypeName":"int","Raw":3} 같은 백업 형태도 허용
        if (token.Type == JTokenType.Object)
        {
            var obj = (JObject)token;
            var p = new Param();
            if (obj.TryGetValue("TypeName", out var tn)) p.TypeName = tn.Value<string>();
            if (obj.TryGetValue("Raw", out var rv)) p.Raw = rv;
            else p.Raw = obj["Value"] ?? obj; // 혹시 Value 필드만 있거나 통째로 들어온 경우
            return p;
        }

        // 방어: 그냥 값만 온 경우
        return new Param
        {
            TypeName = InferTypeName(token),
            Raw = token
        };
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var p = (Param)value;
        var arr = new JArray { p.TypeName ?? InferTypeName(p.Raw), p.Raw };
        arr.WriteTo(writer);
    }

    private static string InferTypeName(JToken tok)
    {
        return tok.Type switch
        {
            JTokenType.Integer => "int",
            JTokenType.Float => "float",
            JTokenType.Boolean => "bool",
            JTokenType.String => "string",
            _ => "unknown"
        };
    }
}