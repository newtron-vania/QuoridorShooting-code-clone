using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// TargetFilter 문자열과 TargetFilterParams를 해석하여
/// 실제 대상 캐릭터 리스트를 반환
/// </summary>
public static class RelicTargetResolver
{
    public static List<BaseCharacter> Resolve(
        string targetFilter,
        Dictionary<string, JToken> filterParams,
        RelicSignal signal)
    {
        // 1단계: 기본 풀 선택
        var candidates = GetBaseCandidates(targetFilter, signal);

        // 2단계: filterParams로 추가 필터링
        if (filterParams != null)
        {
            if (filterParams.TryGetValue("ExcludeType", out var excludeType))
                candidates.RemoveAll(c => c.Type.ToString() == excludeType.ToString());

            if (filterParams.TryGetValue("RequireType", out var requireType))
                candidates.RemoveAll(c => c.Type.ToString() != requireType.ToString());

            if (filterParams.TryGetValue("RequireClass", out var requireClass))
                candidates.RemoveAll(c => c.Class.ToString() != requireClass.ToString());

            if (filterParams.TryGetValue("ExcludeClass", out var excludeClass))
                candidates.RemoveAll(c => c.Class.ToString() == excludeClass.ToString());

            // 최소 아군 타입 수 조건
            if (filterParams.TryGetValue("RequireAllyTypeMinCount", out var typeToken)
                && filterParams.TryGetValue("MinCount", out var minCountToken))
            {
                string requiredType = typeToken.ToString();
                int minCount = minCountToken.Value<int>();
                int actualCount = CountAlliesByTypeName(requiredType);
                if (actualCount < minCount)
                    candidates.Clear();
            }
        }

        return candidates;
    }

    private static List<BaseCharacter> GetBaseCandidates(string targetFilter, RelicSignal signal)
    {
        if (string.IsNullOrEmpty(targetFilter))
            return new List<BaseCharacter>();

        var battle = GameManager.Instance.BattleSystem;
        if (battle?.StageCharacter == null)
            return new List<BaseCharacter>();

        return targetFilter switch
        {
            "Self" => GetSelfList(signal),
            "AllAllies" => GetAllies(),
            "AllEnemies" => GetEnemies(),
            "AllUnits" => GetAllUnits(),

            // 클래스별 아군
            "AlliesTanker" => GetAlliesByClass(CharacterClass.Tanker),
            "AlliesDealer" => GetAlliesByClass(CharacterClass.Dealer),
            "AlliesSupporter" => GetAlliesByClass(CharacterClass.Supporter),

            // 타입별 아군
            "AlliesSuccessor" => GetAlliesByType(CharacterType.Successor),
            "AlliesAwakener" => GetAlliesByType(CharacterType.Awakener),
            "AlliesEngineer" => GetAlliesByType(CharacterType.Engineer),
            "AlliesFreeman" => GetAlliesByType(CharacterType.Freeman),
            "AlliesPathfinder" => GetAlliesByType(CharacterType.Pathfinder),

            // 특수 선택
            "LowHpAlly" => GetLowestHp(GetAllies()),
            "LowHpEnemy" => GetLowestHp(GetEnemies()),
            "RandomEnemy" => GetRandomOne(GetEnemies()),
            "Attacker" => GetAttackerFromSignal(signal),

            // 시스템 레벨 (보상/자원 효과용 — 빈 리스트 반환, 별도 처리)
            "Player" => new List<BaseCharacter>(),

            _ => HandleUnknownFilter(targetFilter)
        };
    }

    private static List<BaseCharacter> GetSelfList(RelicSignal signal)
    {
        if (signal?.Subject is BaseCharacter character)
            return new List<BaseCharacter> { character };
        return new List<BaseCharacter>();
    }

    private static List<BaseCharacter> GetAllies()
    {
        var battle = GameManager.Instance.BattleSystem;
        if (battle.StageCharacter.TryGetValue(CharacterIdentification.Player, out var players))
            return new List<BaseCharacter>(players);
        return new List<BaseCharacter>();
    }

    private static List<BaseCharacter> GetEnemies()
    {
        var battle = GameManager.Instance.BattleSystem;
        if (battle.StageCharacter.TryGetValue(CharacterIdentification.Enemy, out var enemies))
            return new List<BaseCharacter>(enemies);
        return new List<BaseCharacter>();
    }

    private static List<BaseCharacter> GetAllUnits()
    {
        var result = GetAllies();
        result.AddRange(GetEnemies());
        return result;
    }

    private static List<BaseCharacter> GetAlliesByClass(CharacterClass cls)
    {
        return GetAllies().Where(c => c.Class == cls).ToList();
    }

    private static List<BaseCharacter> GetAlliesByType(CharacterType type)
    {
        return GetAllies().Where(c => c.Type == type).ToList();
    }

    private static List<BaseCharacter> GetLowestHp(List<BaseCharacter> pool)
    {
        if (pool.Count == 0) return new List<BaseCharacter>();
        var lowest = pool.OrderBy(c => (float)c.Hp / c.MaxHp).First();
        return new List<BaseCharacter> { lowest };
    }

    private static List<BaseCharacter> GetRandomOne(List<BaseCharacter> pool)
    {
        if (pool.Count == 0) return new List<BaseCharacter>();
        int index = Random.Range(0, pool.Count);
        return new List<BaseCharacter> { pool[index] };
    }

    private static List<BaseCharacter> GetAttackerFromSignal(RelicSignal signal)
    {
        // DamageEventData에서 공격자 추출
        if (signal?.ContextData is DamageEventData damageData && damageData.Attacker != null)
            return new List<BaseCharacter> { damageData.Attacker };
        return new List<BaseCharacter>();
    }

    private static int CountAlliesByTypeName(string typeName)
    {
        if (!System.Enum.TryParse<CharacterType>(typeName, out var type))
            return 0;
        return GetAlliesByType(type).Count;
    }

    private static List<BaseCharacter> HandleUnknownFilter(string targetFilter)
    {
        Debug.LogWarning($"[RelicTargetResolver] Unknown TargetFilter: '{targetFilter}'");
        return new List<BaseCharacter>();
    }
}
