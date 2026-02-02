using System.Collections.Generic;
using UnityEngine;

namespace Battle
{
    /// <summary>
    /// 셀 클래스 - 위치와 복수 상태를 관리
    /// 하나의 셀에 여러 CellState가 동시 존재 가능 (예: Poison + Fire)
    /// 각 상태는 개별 Duration을 가짐
    /// </summary>
    public class Cell
    {
        public Vector2Int Position { get; private set; }
        private HashSet<CellState> _states = new HashSet<CellState>();
        private Dictionary<CellState, int> _stateDurations = new Dictionary<CellState, int>();

        public Cell(Vector2Int position)
        {
            Position = position;
        }

        /// <summary>
        /// 셀에 상태 추가 (Duration 없음, 영구)
        /// </summary>
        public void AddState(CellState state)
        {
            if (state == CellState.None)
                return;

            _states.Add(state);
            _stateDurations[state] = -1; // -1 = 영구
        }

        /// <summary>
        /// 셀에 상태 추가 (Duration 포함)
        /// </summary>
        public void SetState(CellState state, int duration)
        {
            if (state == CellState.None)
                return;

            _states.Add(state);
            _stateDurations[state] = duration;
        }

        /// <summary>
        /// 셀에서 상태 제거
        /// </summary>
        public void RemoveState(CellState state)
        {
            _states.Remove(state);
            _stateDurations.Remove(state);
        }

        /// <summary>
        /// 특정 상태의 Duration 반환
        /// </summary>
        public int GetStateDuration(CellState state)
        {
            if (_stateDurations.TryGetValue(state, out int duration))
            {
                return duration;
            }
            return -1; // 상태 없음
        }

        /// <summary>
        /// 모든 상태의 Duration 감소 및 만료된 상태 제거
        /// </summary>
        /// <returns>제거된 상태 목록</returns>
        public List<CellState> DecrementDurations()
        {
            List<CellState> expiredStates = new List<CellState>();

            foreach (var state in new List<CellState>(_states))
            {
                if (_stateDurations.TryGetValue(state, out int duration))
                {
                    if (duration > 0)
                    {
                        _stateDurations[state] = duration - 1;
                    }
                    else if (duration == 0)
                    {
                        // Duration이 0이 되면 제거
                        _states.Remove(state);
                        _stateDurations.Remove(state);
                        expiredStates.Add(state);
                    }
                    // duration == -1 (영구)는 감소하지 않음
                }
            }

            return expiredStates;
        }

        /// <summary>
        /// 특정 상태를 가지고 있는지 확인
        /// </summary>
        public bool HasState(CellState state)
        {
            return _states.Contains(state);
        }

        /// <summary>
        /// 모든 상태 반환
        /// </summary>
        public IReadOnlyCollection<CellState> GetStates()
        {
            return _states;
        }

        /// <summary>
        /// 셀에 상태가 있는지 확인
        /// </summary>
        public bool HasAnyState()
        {
            return _states.Count > 0;
        }

        /// <summary>
        /// 모든 상태 제거
        /// </summary>
        public void ClearAllStates()
        {
            _states.Clear();
            _stateDurations.Clear();
        }
    }
}
