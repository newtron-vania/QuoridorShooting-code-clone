using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

[System.Serializable]
public class SupplymentData
{
    // ============LEGACY============= //
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupplyTarget
    {
        None = -1,
        TargetChar, // 특정 단일 아군 캐릭터
        Allies,      // Player를 포함한 전체 아군 캐릭터
        TargetEnemy,// 특정 단일 적군 캐릭터
        AllEnemy    // 전체 적군 캐릭터
    }
    public SupplyTarget Target;           // 보급품 적용 대상
    public float BoxDropRate;             // 상자 출현 수치
    public float EnemyDropRate;           // 적군 드랍 수치
    public string EditName;               // 스크립트에서 사용할 영어 이름
    // ============LEGACY============= //
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupplyType
    {
        None = -1,
        Attack,
        Defense,
        Support,
        //==LEGACY==//
        Cure,
        Enhance
        //==LEGACY==//
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum SupplyGrade
    {
        None = -1,
        Normal,
        Rare,
        Unique
    }

    public int Id;              // 보급품 아이디
    public string Name;         // 보급품 이름
    public string Description;            // 보급품 설명
    public List<string> Tags;              // 보급품 태그
    public SupplyGrade Grade;              // 보급품 랭크
    public SupplyType Type;               // 보급품 종류
    public int IconId;                  // 보급품 아이콘
    public int TargetRangeId;           // 대상 지정 가능 범위
    public int EffectRangeId;           // 효과 적용 대상 범위
    public TriggerType Trigger;              // 트리거 (일단은 문자열로 저장, 추후 enum등으로 변경해야 할듯)
    public List<EffectData> EffectDataList;         // 효과 데이터

}
