using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유물 시스템의 중앙 관리자
/// </summary>
public class RelicManager : Singleton<RelicManager>
{
    // 활성 유물 인스턴스 저장소
    private Dictionary<int, IRelic> _activeRelics = new();

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

        // [수정] RelicDatabase를 통해 데이터 호출
        RelicData data = RelicDatabase.Instance.GetRelic(relicId);
        
        if (data == null)
        {
            Debug.LogError($"[RelicManager] Failed to find relic data for ID: {relicId}");
            return;
        }

        // 팩토리를 통해 유물 인스턴스 생성
        IRelic relic = _factory.CreateRelic(data);
        
        // 데이터 등록 및 초기화 (Owner-less 구조이므로 캐릭터는 null 또는 시스템 주체 전달)
        relic.Register(data); 

        _activeRelics.Add(relicId, relic);

        Debug.Log($"[RelicManager] Acquired relic: {data.Name} (ID: {relicId})");
    }

    // 유물 제거
    public void RemoveRelic(int relicId)
    {
        if (_activeRelics.TryGetValue(relicId, out IRelic relic))
        {
            relic.Unregister();
            _activeRelics.Remove(relicId);

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
        Debug.Log("[RelicManager] Cleared all relics");
    }

    // 현재 보유 중인 유물 ID 목록 반환
    public List<int> GetActiveRelicIds() => new List<int>(_activeRelics.Keys);

    private void OnDestroy()
    {
        if (_activeRelics != null)
        {
            ClearAllRelics();
        }
    }
}
