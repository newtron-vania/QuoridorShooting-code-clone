using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

/// <summary>
/// JSON으로부터 로드되는 유물 정의 데이터
/// Phase 2 스키마: 복수 효과, 시너지, 전역 바인딩, 조건부 실행 지원
/// </summary>
[System.Serializable]
public class RelicData
{
    // === 기본 정보 ===
    public int Id;
    public string Name;
    public string Description;
    public RelicRarity Rarity;
    public string IconPath;

    // === 시너지 ===
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Synergy;

    // === 트리거 ===
    public List<RelicTriggerType> TriggerTypes;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, JToken> TriggerParams;

    // === 타겟 필터 (string 기반) ===
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string TargetFilter;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, JToken> TargetFilterParams;

    // === 효과 (복수 효과 지원) ===
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<RelicEffectEntry> Effects;

    // === 실행 제어 ===
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int Priority;

    // === 하위호환: Phase 1 JSON 지원 (Phase 2D에서 제거 예정) ===
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public RelicEffectType? LogicType;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, float> LogicParams;
}

/// <summary>
/// 개별 효과 항목 — 조건부 실행과 전역 데이터 참조를 지원
/// </summary>
[System.Serializable]
public class RelicEffectEntry
{
    public RelicEffectType EffectType;

    // 정적 파라미터 (JSON 데이터에서 로드)
    public Dictionary<string, JToken> Params;

    // 조건부 실행 (선택): 이 조건이 참일 때만 이 효과를 실행
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public RelicEffectCondition Condition;

    // 전역 데이터 참조 키 (선택): 런타임에 GlobalDataProvider에서 값을 가져와 Params에 주입
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<GlobalParamBinding> GlobalBindings;

    // 가역성 플래그 (선택): true이면 조건 미충족 시 효과 제거
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public bool IsReversible;
}

/// <summary>
/// 효과별 조건 — 조건이 충족될 때만 해당 효과를 실행
/// </summary>
[System.Serializable]
public class RelicEffectCondition
{
    public string ConditionType;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, JToken> ConditionParams;
}

/// <summary>
/// 전역 데이터 바인딩 — 런타임 전역 상태를 파라미터로 주입
/// </summary>
[System.Serializable]
public class GlobalParamBinding
{
    public string ParamKey;         // Effects.Params에서 이 키의 값을 대체
    public string GlobalSource;     // 전역 데이터 소스 식별자

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Expression;       // 계산식 (선택)
}

/// <summary>
/// 유물 실행 조건
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RelicTriggerType
{
    None = 0,

    // 캐릭터 이벤트 (1-99)
    Always = 1,
    OnTurnStart = 2,
    OnTurnEnd = 3,
    OnDamaged = 4,
    OnDealDamage = 5,
    OnHpChanged = 6,

    OnHpBelowThreshold = 7,
    HPLessThan = 7,                 // 별칭: JSON "HPLessThan"

    OnCharacterDeath = 8,
    OnKill = 8,                     // 별칭: JSON "OnKill"

    OnSkillUsed = 9,
    OnSkillUse = 9,                 // 별칭: JSON "OnSkillUse"

    OnMove = 10,

    OnActionStart = 11,
    ActionStart = 11,               // 별칭: JSON "ActionStart"

    OnActionEnd = 12,

    OnApBelowThreshold = 13,
    APLessThan = 13,                // 별칭: JSON "APLessThan"

    APFull = 130,                   // 신규: JSON "APFull"

    // 전투/공격 세부 이벤트 (20-39)
    OnHitSuccess = 20,
    HitSuccess = 20,                // 별칭: JSON "HitSuccess"

    OnHitFail = 21,
    OnDefenseSuccess = 22,
    OnDefenseFail = 23,

    // 시스템 이벤트 (100-199)
    OnBattleStart = 100,

    OnBattleWin = 101,
    BattleWin = 101,                // 별칭: JSON "BattleWin"

    OnBattleLose = 102,
    BattleDefeat = 102,             // 별칭: JSON "BattleDefeat"

    OnStageComplete = 103,
    OnRewardCalculation = 104,
    OnFirstTurn = 105,

    // 보유/조건 이벤트 (150-179)
    InLineUp = 150,
    OwnArtifact = 151,

    // 필드/타일 이벤트 (200-299)
    OnTileEnter = 200,
    OnTileExit = 201,
    WhileInZone = 202,
    OnSpecificTile = 203,

    // 상태이상 이벤트 (300-399)
    OnStatusApplied = 300,
    OnStatusRemoved = 301,
}

/// <summary>
/// 유물 효과 타입
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RelicEffectType
{
    // 캐릭터 스탯 관련
    StatMultiplier,
    StatFlat,
    TimedStatModifier,
    ConditionalStatBoost,

    // 보상 관련
    RewardMultiplier,
    RewardAdditional,

    // 전투 효과
    OnDeathExplosion,
    OnDeathHeal,
    DamageReflect,

    // 자원/시스템
    ApBoost,
    CooldownReduction,

    // 타일/필드
    CreatePoisonTile,
    CreateHealTile,

    // === 신규: Phase 2 효과 ===
    ApplyStatusEffect,
    ApplyStatusToAttacker,
    ResourceGain,
    ShopDiscount,
    HealOnEvent,
    DamageReduction,
    BuffSpread,
    CleanseOnBuff,
    ApplyStatusOnAllyStatus,
    TransferDamage,
    EnemyStatFlat,
    ExtraAttack,
    SynergyOnAcquire,
}

/// <summary>
/// 유물 희귀도 — JSON 값("Normal", "Rare", "Unique", "Legend")에 맞춤
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum RelicRarity
{
    Normal = 0,
    Common = 0,       // 하위호환 별칭
    Rare = 1,
    Unique = 2,
    Epic = 2,         // 하위호환 별칭
    Legend = 3,
    Legendary = 3,    // 하위호환 별칭
}

/// <summary>
/// 유물 인스턴스의 런타임 상태 — 쿨다운, 카운터, 스택 등
/// </summary>
[System.Serializable]
public class RelicRuntimeState
{
    public float LastTriggerTurn = -999f;
    public Dictionary<string, int> Counters = new();
    public Dictionary<string, Dictionary<string, object>> ConditionStates = new();

    public void ResetForScope(RelicResetScope scope)
    {
        switch (scope)
        {
            case RelicResetScope.Battle:
                Counters.Clear();
                foreach (var cs in ConditionStates.Values)
                    cs.Clear();
                break;
            case RelicResetScope.Stage:
                if (Counters.ContainsKey("stageCount"))
                    Counters["stageCount"] = 0;
                break;
        }
    }
}

/// <summary>
/// Stateful 조건 리셋 범위
/// </summary>
public enum RelicResetScope
{
    Battle,
    Stage,
    Turn
}

/// <summary>
/// 유물 시스템 저장 데이터
/// </summary>
[System.Serializable]
public class RelicSystemSaveData
{
    public List<RelicSaveData> ActiveRelics = new();
}

[System.Serializable]
public class RelicSaveData
{
    public int RelicId;
    public RelicRuntimeState RuntimeState;
}
