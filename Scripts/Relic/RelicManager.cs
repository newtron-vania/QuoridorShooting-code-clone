using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 유물 시스템의 중앙 관리자
/// - Priority 기반 등록 순서 보장
/// - Save/Load 지원
/// </summary>
public class RelicManager : Singleton<RelicManager>
{
    // 활성 유물 인스턴스 저장소
    private Dictionary<int, IRelic> _activeRelics = new();

    // Priority 순서 유지 (낮은 우선순위 먼저)
    private readonly List<(int relicId, int priority)> _priorityOrder = new();

    // 유물 Factory
    private RelicFactory _factory;

    public override void Awake()
    {
        base.Awake();
        _factory = new RelicFactory();
    }

    // 유물 획득
    public void AcquireRelic(int relicId)
    {
        if (_activeRelics.ContainsKey(relicId))
        {
            Debug.LogWarning($"[RelicManager] Relic already active: {relicId}");
            return;
        }

        RelicData data = RelicDatabase.Instance.GetRelic(relicId);

        if (data == null)
        {
            Debug.LogError($"[RelicManager] Failed to find relic data for ID: {relicId}");
            return;
        }

        // 팩토리를 통해 유물 인스턴스 생성
        IRelic relic = _factory.CreateRelic(data);

        // 데이터 등록 및 초기화
        relic.Register(data);

        _activeRelics.Add(relicId, relic);

        // Priority 순서 유지: 삽입 위치 탐색
        int insertIdx = _priorityOrder.FindIndex(p => p.priority > data.Priority);
        if (insertIdx < 0) insertIdx = _priorityOrder.Count;
        _priorityOrder.Insert(insertIdx, (relicId, data.Priority));

        Debug.Log($"[RelicManager] Acquired relic: {data.Name} (ID: {relicId}, Priority: {data.Priority})");
    }

    // 유물 제거
    public void RemoveRelic(int relicId)
    {
        if (_activeRelics.TryGetValue(relicId, out IRelic relic))
        {
            relic.Unregister();
            _activeRelics.Remove(relicId);
            _priorityOrder.RemoveAll(p => p.relicId == relicId);

            Debug.Log($"[RelicManager] Removed relic: {relicId}");
        }
    }

    // 모든 유물 정리
    public void ClearAllRelics()
    {
        foreach (var relic in _activeRelics.Values)
        {
            relic.Unregister();
        }

        _activeRelics.Clear();
        _priorityOrder.Clear();
        Debug.Log("[RelicManager] Cleared all relics");
    }

    // 현재 보유 중인 유물 ID 목록 반환
    public List<int> GetActiveRelicIds() => new List<int>(_activeRelics.Keys);

    // Priority 순서로 정렬된 유물 ID 목록
    public List<int> GetRelicIdsInPriorityOrder() =>
        _priorityOrder.Select(p => p.relicId).ToList();

    // 활성 유물 인스턴스 조회
    public IRelic GetRelic(int relicId) =>
        _activeRelics.TryGetValue(relicId, out var relic) ? relic : null;

    // === Save/Load ===

    /// <summary>
    /// 현재 유물 시스템 상태를 직렬화
    /// </summary>
    public RelicSystemSaveData ToSaveData()
    {
        var saveData = new RelicSystemSaveData();
        foreach (var (relicId, _) in _priorityOrder)
        {
            if (_activeRelics.TryGetValue(relicId, out var relic) && relic is DataDrivenRelicInstance ddri)
            {
                saveData.ActiveRelics.Add(ddri.ToSaveData());
            }
            else
            {
                // Phase 1 유물은 ID만 저장
                saveData.ActiveRelics.Add(new RelicSaveData
                {
                    RelicId = relicId,
                    RuntimeState = new RelicRuntimeState()
                });
            }
        }
        return saveData;
    }

    /// <summary>
    /// 저장된 데이터로부터 유물 시스템 복원
    /// </summary>
    public void LoadFromSaveData(RelicSystemSaveData saveData)
    {
        ClearAllRelics();

        if (saveData?.ActiveRelics == null) return;

        foreach (var relicSave in saveData.ActiveRelics)
        {
            RelicData data = RelicDatabase.Instance.GetRelic(relicSave.RelicId);
            if (data == null) continue;

            IRelic relic = _factory.CreateRelic(data);
            relic.Register(data);
            _activeRelics.Add(relicSave.RelicId, relic);

            int insertIdx = _priorityOrder.FindIndex(p => p.priority > data.Priority);
            if (insertIdx < 0) insertIdx = _priorityOrder.Count;
            _priorityOrder.Insert(insertIdx, (relicSave.RelicId, data.Priority));

            // DataDrivenRelicInstance는 상태 복원
            if (relic is DataDrivenRelicInstance ddri && relicSave.RuntimeState != null)
                ddri.LoadState(relicSave.RuntimeState);
        }

        Debug.Log($"[RelicManager] Loaded {saveData.ActiveRelics.Count} relics from save data");
    }

    private void OnDestroy()
    {
        if (_activeRelics != null)
        {
            ClearAllRelics();
        }
    }
}
