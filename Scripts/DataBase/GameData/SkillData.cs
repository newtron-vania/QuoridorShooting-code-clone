using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum SkillTargetType
{
    Self,         // 캐릭터(본인)
    Ally,         // 캐릭터(아군)
    Enemy,        // 캐릭터(적군)
    All,          // 캐릭터(전부)
    Cell,          // 셀(영역)
    Tile
}
[JsonConverter(typeof(StringEnumConverter))]
public enum SkillType
{
    Attack,        // 공격형
    Defense,       // 방어형
    Support,       // 지원형
    Control,       // 제어형
    Deploy         // 설치형
}
[System.Serializable]
public class SkillData
{
    // ===================LEGACY================== //
    //[JsonConverter(typeof(StringEnumConverter))]
    //public enum SkillTarget
    //{
    //    Hate,      // 적대적 대상
    //    Friend,    // 우호적 대상
    //    All        // 모든 대상
    //}
    //public SkillTarget TargetTypeLegacy;      // 스킬 타겟
    //public int SkillDuration;           // 득수능력 효과 지속 라운드
    //public bool IsBlock;                // 벽으로 인해 차단되는지 여부
    //public List<int> EffectList;        // 효과 Id 목록
    // ===================LEGACY================== //
    public int Id;                        // ID
    public string Name;                   // 이름
    public string Description;            // 설명글
    public SkillType Type;                // 유형
    public int ApCost;                    // 행동력 소모량
    public List<string> Tags;             // 스킬 태그
    public SkillTargetType TargetType;    // 발동 대상
    public int TargetRangeId;             // 대상 지정 가능 범위
    public int EffectRangeId;             // 효과 적용 대상 범위
    public int Duration;                // 지속 시간
    public List<EffectData> EffectDataList;         // 효과 데이터
}