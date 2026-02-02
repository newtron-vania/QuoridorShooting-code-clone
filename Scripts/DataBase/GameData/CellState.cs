using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// 셀 상태 Enum
/// SkillInstance의 EffectData Params에서 "CellState" 파라미터로 사용
/// 하나의 셀에 복수 상태가 동시 존재 가능
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum CellState
{
    None,           // 기본 상태 (효과 없음)
    Poison,         // 독 지대 (독성 폭탄 - AbilityId: 1008)
    SmokeBomb,      // 연막 지대 (연막탄 - AbilityId: 1003)
    Slime,          // 미끄럼 지대 (미끈젤리 - AbilityId: 1013)
    Fire            // 화염 지대 (확장용)
}
