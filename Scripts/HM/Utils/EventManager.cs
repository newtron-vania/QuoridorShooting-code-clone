using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HM
{

    public enum EventType
    {
        None,
        OnCharacterDead,
        OnGameStart,
        OnGameFinish,
        OnGameOver,
        OnRoundEnd,
        OnTurnEnd,
        OnTurnStart,
        OnCharacterSelected,     // 특정 캐릭터가 선택됐을 때
        OnCharacterDeselected,   // 어떤 캐릭터도 선택되지 않았을 때
        OnSupplyGet,             // 보급품을 획득했을 때
        OnBattleStart,
        OnBattleFinish,
        OnCellEnter,             // 캐릭터가 셀에 진입할 때
        OnCellStay,              // 캐릭터가 셀에 머물 때
        OnCellExit,              // 캐릭터가 셀을 떠날 때
        OnCharacterDamaged,
        OnPlayerHpChanged,
        OnCharacterTurnStart,
        etc,
    }

    public interface IEventListener
    {
        void OnEvent(EventType eventType, Component sender, object param = null);
    }
    public class EventManager : Singleton<EventManager>
    {
        private Dictionary<EventType, List<IEventListener>> _eventListners = new Dictionary<EventType, List<IEventListener>>();

        public void AddEvent(EventType eventType, IEventListener listner)
        {
            if (!_eventListners.ContainsKey(eventType))
            {
                _eventListners[eventType] = new List<IEventListener>();
            }
            _eventListners[eventType].Add(listner);
        }

        public void InvokeEvent(EventType eventType, Component sender, object param = null)
        {
            if (_eventListners.TryGetValue(eventType, out var listners))
            {
                foreach (var listner in listners)
                {
                    listner?.OnEvent(eventType, sender, param);
                }
            }
        }

        public void RemoveEvent(EventType eventType, IEventListener listner)
        {
            if (_eventListners.TryGetValue(eventType, out var listners))
            {
                listners.Remove(listner);

                if (listners.Count == 0)
                {
                    _eventListners.Remove(eventType);
                }
            }
        }

        public void EnsureIntegrity()
        {
            foreach (var eventType in _eventListners.Keys.ToList())
            {
                var listners = _eventListners[eventType].Where(listner => listner != null).ToList();

                if (listners.Count > 0)
                {
                    _eventListners[eventType] = listners;
                }
                else
                {
                    _eventListners.Remove(eventType);
                }
            }
        }

    }
}