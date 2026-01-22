using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public static class DataLoader
{
    // blob(파일 바이트) -> T (예: List<EffectData>)
    public static T Load<T>(byte[] encryptedBlob, string keyString)
    {
        byte[] plain = null;
        try
        {
            // 1) 복호화 (AES-GCM + raw DEFLATE 해제는 DataDecoder가 수행)
            plain = DataDecoder.DecryptQED(encryptedBlob, keyString);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] DataLoader::Load - Error decrypting data: {ex.Message}");
            return default;
        }

        // 2) UTF-8 -> string
        string json = Encoding.UTF8.GetString(plain);

        // 3) Newtonsoft.Json 역직렬화
        var settings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
        };
        settings.Converters.Add(new StringEnumConverter());
        settings.Converters.Add(new ParamJsonConverter());
        settings.Converters.Add(new Vector2IntConverter());
        settings.Converters.Add(new SafeListConverter<EffectData>());
        settings.Converters.Add(new HashSetConverter<StatuseffectTag>());

        try
        {
            T result = JsonConvert.DeserializeObject<T>(json, settings);

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ERROR] DataLoader::Load - Error loading data: {ex.Message}");
            return default;
        }
    }

    // 파일 경로에서 바로 로드
    public static T LoadFromFile<T>(string fullPath, string keyString)
    {
        byte[] blob = File.ReadAllBytes(fullPath);
        return Load<T>(blob, keyString);
    }
}
