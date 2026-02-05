using System.Collections.Generic;
using CharacterDefinition;
using UnityEngine;

/// <summary>
/// 유물이 런타임에 전역 데이터를 조회하는 중앙 인터페이스
/// RelicEffectEntry.GlobalBindings에서 참조하는 값을 해석
/// 프레임 캐싱으로 반복 조회 최적화
/// </summary>
public static class GlobalDataProvider
{
    // === 캐싱 시스템 ===
    private static readonly Dictionary<string, float> _cache = new();
    private static int _cacheFrame = -1;

    /// <summary>
    /// GlobalSource 문자열로 전역 값을 조회
    /// </summary>
    /// <param name="globalSource">전역 데이터 소스 식별자</param>
    /// <param name="context">캐릭터 컨텍스트 (TargetHpRatio 등에 필요)</param>
    public static float Resolve(string globalSource, BaseCharacter context = null)
    {
        // 프레임 캐싱: context-free 소스만 캐시 대상
        if (context == null && TryGetCached(globalSource, out float cached))
            return cached;

        float result = ResolveInternal(globalSource, context);

        if (context == null)
            SetCache(globalSource, result);

        return result;
    }

    private static float ResolveInternal(string globalSource, BaseCharacter context)
    {
        return globalSource switch
        {
            // 턴/스테이지
            "CurrentTurn"       => GameManager.Instance.Turn,
            "CurrentStage"      => GameManager.Stage,

            // 시너지 카운트 (태그별)
            var s when s.StartsWith("SynergyCount_")
                => CountSynergyTag(s.Substring("SynergyCount_".Length)),

            // 전장 클래스별 아군 수
            "AllyCount_Tanker"    => CountAlliesByClass(CharacterClass.Tanker),
            "AllyCount_Dealer"    => CountAlliesByClass(CharacterClass.Dealer),
            "AllyCount_Supporter" => CountAlliesByClass(CharacterClass.Supporter),

            // 전장 타입별 아군 수
            "AllyCount_Successor" => CountAlliesByType(CharacterType.Successor),
            "AllyCount_Awakener"  => CountAlliesByType(CharacterType.Awakener),
            "AllyCount_Engineer"  => CountAlliesByType(CharacterType.Engineer),
            "AllyCount_Freeman"   => CountAlliesByType(CharacterType.Freeman),
            "AllyCount_Pathfinder"=> CountAlliesByType(CharacterType.Pathfinder),

            // 보유 유물 수
            "OwnedRelicCount"   => RelicManager.Instance.GetActiveRelicIds().Count,

            // 캐릭터 개별 스탯 (context 필요)
            "TargetHpRatio"     => context != null
                                   ? (float)context.Hp / context.MaxHp : 0f,
            "TargetMaxHp"       => context?.MaxHp ?? 0f,
            "TargetAtk"         => context?.CharacterStat?.Atk ?? 0f,

            // 미등록 소스
            _ => HandleUnknownSource(globalSource)
        };
    }

    private static float HandleUnknownSource(string globalSource)
    {
        Debug.LogWarning($"[GlobalDataProvider] Unknown GlobalSource: '{globalSource}' -> defaulting to 0f");
        return 0f;
    }

    private static bool TryGetCached(string key, out float value)
    {
        if (Time.frameCount != _cacheFrame)
        {
            _cache.Clear();
            _cacheFrame = Time.frameCount;
            value = 0f;
            return false;
        }
        return _cache.TryGetValue(key, out value);
    }

    private static void SetCache(string key, float value)
    {
        _cache[key] = value;
    }

    private static int CountSynergyTag(string tagName)
    {
        int count = 0;
        var activeIds = RelicManager.Instance.GetActiveRelicIds();
        foreach (var id in activeIds)
        {
            var data = RelicDatabase.Instance.GetRelic(id);
            if (data?.Synergy != null && data.Synergy.Contains(tagName))
                count++;
        }
        return count;
    }

    private static int CountAlliesByClass(CharacterClass cls)
    {
        var battle = GameManager.Instance.BattleSystem;
        if (battle?.StageCharacter == null) return 0;
        if (!battle.StageCharacter.ContainsKey(CharacterIdentification.Player)) return 0;
        int count = 0;
        foreach (var c in battle.StageCharacter[CharacterIdentification.Player])
            if (c.Class == cls) count++;
        return count;
    }

    private static int CountAlliesByType(CharacterType type)
    {
        var battle = GameManager.Instance.BattleSystem;
        if (battle?.StageCharacter == null) return 0;
        if (!battle.StageCharacter.ContainsKey(CharacterIdentification.Player)) return 0;
        int count = 0;
        foreach (var c in battle.StageCharacter[CharacterIdentification.Player])
            if (c.Type == type) count++;
        return count;
    }
}
