using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

// StatusEffect 관련 Enum이나 Class가 있다면 여기 아래로 적어주세요
[JsonConverter(typeof(StringEnumConverter))]
public enum StatuseffectTag
{
    Buff,               //버프
    DeBuff,             //디버프
    CrowdControl,       //군중제어
    Immutable,           //지속시간X
}

// Status Effect Data
[System.Serializable]
public class StatuseffectData
{
    public int Id;                          // 상태 이상 효과 ID
    public string Name;                     // 상태 이상 효과 이름
    public string NameKr;                   // 상태 이상 효과 이름 (한국어) 
    public string Description;              // 상태 이상 효과 설명
    public HashSet<StatuseffectTag> Tags;                // 상태 이상 효과 태그
    public List<EffectData> EffectDataList; // 효과 데이터
}
