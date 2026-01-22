using System.Collections.Generic;
using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;


[JsonConverter(typeof(StringEnumConverter))]
public enum EffectType
{

    //공용
    //대미지
    CommonSimpleDamage,
    CommonSimpleDamage_OnTurnStart,
    CommonStatBaseDamage_Atk,

    //생성
    CommonStatuseffectCreation,
    CommonStatuseffectCreation_OnEnd,

    //영역 진입 시 상태 부여
    CommonStatuseffectCreationArea,
    CommonStatuseffectCreationArea_AreaOnly,

    //액션 카운트 증감
    CommonActionCountAdd_Attack,
    CommonActionCountAdd_Move,

    //스텟 증감
    CommonStatAdd_ApRecovery,
    CommonStatAdd_Avd,

    //상태이상 제거 
    CommonStatuseffectClear_ByTag,

    //힐
    CommonSimpleHeal,

    //Skill Logic, SkillInstance를 참조하여 효과 진행
    //대미지
    //힐
    //이동기
    //능력 인스턴스 상태변경
    SkillStateChange,



    //Statuseffect Logic
    //생성
    StatuseffectDamageStatuseffectCreation_OnAttack,
    //스텟 증감
    StatuseffectStatAdd_Avd,
    StatuseffectStatAdd_Atk,
    //데미지
    StatuseffectDamage_OnTurnStart,


}
[JsonConverter(typeof(StringEnumConverter))]
public enum TargetType
{
    Self,
    Allies,
    TargetEnemy,
    All,

    Ally,
    Enemy,

}
[JsonConverter(typeof(StringEnumConverter))]
public enum TriggerType
{
    Always,
    OnHitSuccess,
    OnPlayerTurnStart,
}

public class EffectData
{
    [JsonProperty("EffectType")]
    public EffectType Type;
    [JsonProperty("TargetType")]
    public TargetType Target;
    [JsonProperty("Params")]
    public Dictionary<string, Param> Params;

    public bool TryGet<T>(string key, out T value)
    {
        value = default;
        if (Params != null && Params.TryGetValue(key, out var p))
        {
            try { value = p.As<T>(); return true; } catch { return false; }
        }
        return false;
    }

    public T Get<T>(string key, T fallback = default)
    {
        if (Params == null || !Params.TryGetValue(key, out var p))
            return fallback;

        try
        {
            // 1) enum 처리
            if (typeof(T).IsEnum)
            {
                // 숫자로 들어온 경우
                if (p.Raw.Type == JTokenType.Integer)
                {
                    var iv = p.Raw.Value<int>();
                    return (T)Enum.ToObject(typeof(T), iv);
                }

                // 문자열로 들어온 경우
                var name = p.Raw.Value<string>();
                return (T)Enum.Parse(typeof(T), name, ignoreCase: true);
            }
            // 2) 일반 타입 처리
            return p.As<T>();
        }
        catch
        {
            return fallback;
        }
    }

    public bool TryGetEnum<TEnum>(string key, out TEnum value) where TEnum : struct, Enum
    {
        value = default;
        if (Params != null && Params.TryGetValue(key, out var p))
        {
            try { value = p.AsEnum<TEnum>(); return true; } catch { return false; }
        }
        return false;
    }
}

[JsonConverter(typeof(ParamJsonConverter))]
public class Param
{
    public string TypeName { get; set; }   // 예: "int", "float", "string", "SpecialEffectType"
    public JToken Raw { get; set; }        // 실제 값 (원시/문자열/숫자 등)

    // 강타입 변환 (int/float/string/bool 등)
    public T As<T>()
    {
        var t = typeof(T);

        // 타입명이 들어왔으면 힌트로 활용 (필수는 아님)
        if (Raw == null || Raw.Type == JTokenType.Null)
            return default;

        // 숫자/문자/불리언 등 기본형 처리
        try
        {
            // JToken -> T 직접 변환 시도
            return Raw.ToObject<T>();
        }
        catch
        {
            // 실패 시 형변환 재시도 (예: "3" -> int)
            var s = Raw.Type == JTokenType.String ? Raw.Value<string>() : Raw.ToString();
            return (T)Convert.ChangeType(s, typeof(T));
        }
    }

    public TEnum AsEnum<TEnum>() where TEnum : struct, Enum
    {
        if (Raw == null || Raw.Type == JTokenType.Null)
            throw new InvalidOperationException("Param value is null.");

        // 이름/숫자 모두 허용
        if (Raw.Type == JTokenType.Integer)
        {
            var iv = Raw.Value<int>();
            return (TEnum)Enum.ToObject(typeof(TEnum), iv);
        }
        else
        {
            var name = Raw.Value<string>();
            if (Enum.TryParse<TEnum>(name, ignoreCase: true, out var result))
                return result;
            throw new ArgumentException($"'{name}' is not a valid {typeof(TEnum).Name}.");
        }
    }
}