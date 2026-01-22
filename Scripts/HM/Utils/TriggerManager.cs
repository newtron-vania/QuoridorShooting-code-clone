using System.Collections.Generic;
using UnityEngine;

public enum GameTrigger
{
    GameStart,
    GameEnd,
    BattleOver,
    UIOpen,
    CharacterDamaged,
    CharacterAttack,
    PlayerFieldSetting,
    EnemyFieldSetting
}

public class TriggerManager : Singleton<TriggerManager>
{
    private Dictionary<GameTrigger, bool> activeTriggers = new Dictionary<GameTrigger, bool>();

    public override void Awake()
    {
        base.Awake();

        // 모든 트리거 초기화 (기본값: 비활성화)
        foreach (GameTrigger trigger in System.Enum.GetValues(typeof(GameTrigger)))
        {
            activeTriggers[trigger] = false;
        }
    }

    /// 특정 트리거를 활성화합니다.
    public void ActivateTrigger(GameTrigger trigger)
    {
        if (activeTriggers.ContainsKey(trigger))
        {
            activeTriggers[trigger] = true;
            Debug.Log($"[INFO] TriggerManager::ActivateTrigger({trigger}) - {trigger} 활성화됨");
        }
    }

    /// 특정 트리거를 비활성화합니다.
    public void DeactivateTrigger(GameTrigger trigger)
    {
        if (activeTriggers.ContainsKey(trigger))
        {
            activeTriggers[trigger] = false;
            Debug.Log($"[INFO] TriggerManager::DeactivateTrigger({trigger}) - {trigger} 비활성화됨");
        }
    }

    /// 특정 트리거가 활성화되어 있는지 확인합니다.
    public bool IsTriggerActive(GameTrigger trigger)
    {
        return activeTriggers.ContainsKey(trigger) && activeTriggers[trigger];
    }

    /// 활성화된 모든 트리거를 가져옵니다.
    public List<GameTrigger> GetActiveTriggers()
    {
        List<GameTrigger> activeList = new List<GameTrigger>();
        foreach (var trigger in activeTriggers)
        {
            if (trigger.Value)
            {
                activeList.Add(trigger.Key);
            }
        }
        return activeList;
    }
}