using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// JSON으로부터 로드되는 유물 정의 데이터
/// </summary>
[System.Serializable]
public class RelicData
{
    public int Id;
    public string Name;
    public string Description;

    // 수정: List 위에 붙어있던 JsonConverter를 제거합니다.
    // Enum 정의 자체에 어트리뷰트가 있으므로 자동으로 각 요소가 변환됩니다.
    public List<RelicTriggerType> TriggerTypes;

    public Dictionary<string, float> TriggerParams;

    // 단일 Enum 필드들도 Enum 정의에 이미 어트리뷰트가 있다면 제거해도 무방합니다. (코드가 깔끔해집니다)
    public RelicEffectType LogicType;

    public Dictionary<string, float> LogicParams;

    public TargetType TargetFilter;

    public RelicRarity Rarity;

    public string IconPath;
}

/// <summary>
/// 유물 실행 조건
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RelicTriggerType
{
    None = 0,

    // 캐릭터 이벤트 (1-99)
    Always = 1,            // 전투 시작 시 즉시 적용 (패시브)
    OnTurnStart = 2,       // 턴 시작 시
    OnTurnEnd = 3,         // 턴 종료 시
    OnDamaged = 4,         // 피격 시
    OnDealDamage = 5,      // 데미지 가할 시
    OnHpChanged = 6,       // HP 변동 시
    OnHpBelowThreshold = 7, // HP가 특정 % 이하일 때
    OnCharacterDeath = 8,  // 캐릭터 사망 시
    OnSkillUsed = 9,       // 스킬 사용 시
    OnMove = 10,           // 이동 시
    OnActionStart = 11,    // 캐릭터 행동 시작 시
    OnActionEnd = 12,      // 행동 완료 시
    OnApBelowThreshold = 13, // AP가 특정값 미만일 때

    // 전투/공격 세부 이벤트 (20-39)
    OnHitSuccess = 20,     // 일반 공격 성공 시
    OnHitFail = 21,        // 일반 공격 실패 시
    OnDefenseSuccess = 22, // 방어 성공 시
    OnDefenseFail = 23,    // 방어 실패 시

    // 시스템 이벤트 (100-199)
    OnBattleStart = 100,   // 전투 시작 시
    OnBattleWin = 101,     // 전투 승리 시
    OnBattleLose = 102,    // 전투 패배 시
    OnStageComplete = 103, // 스테이지 완료 시
    OnRewardCalculation = 104, // 보상 계산 시
    OnFirstTurn = 105,     // 첫 턴 시작 시

    // 보유/조건 이벤트 (150-179)
    InLineUp = 150,        // 특정 캐릭터 보유 시
    OwnArtifact = 151,     // 특정 등급 유물 보유 시

    // 필드/타일 이벤트 (200-299)
    OnTileEnter = 200,     // 타일 진입 시
    OnTileExit = 201,      // 타일 퇴출 시
    WhileInZone = 202,     // 특정 범위 내 위치 시
    OnSpecificTile = 203   // 특정 단일 타일 위치 시
}


/// <summary>
/// 유물 효과 타입
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RelicEffectType
{
    // 캐릭터 스탯 관련 (Target: BaseCharacter)
    StatMultiplier,       // 스탯 퍼센트 증가 (영구)
    StatFlat,             // 스탯 고정값 증가 (영구)
    TimedStatModifier,    // 일정 턴 동안 스탯 증가
    ConditionalStatBoost, // 조건부 스탯 증가

    // 보상 관련 (Target: RewardContext)
    RewardMultiplier,     // 보상 배율 증가
    RewardAdditional,     // 보상 추가 아이템

    // 전투 효과 (Target: BaseCharacter 또는 Tile)
    OnDeathExplosion,     // 사망 시 주변 폭발
    OnDeathHeal,          // 사망 시 주변 회복
    DamageReflect,        // 피격 데미지 반사

    // 자원/시스템 (Target: GameManager/TurnManager)
    ApBoost,              // AP 회복량 증가
    CooldownReduction,    // 스킬 쿨다운 감소

    // 타일/필드 (Target: Tile)
    CreatePoisonTile,     // 독 타일 생성
    CreateHealTile        // 회복 타일 생성
}

/// <summary>
/// 유물 희귀도
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RelicRarity
{
    Common,    // 일반
    Uncommon,  // 고급
    Rare,      // 희귀
    Epic,      // 영웅
    Legendary  // 전설
}