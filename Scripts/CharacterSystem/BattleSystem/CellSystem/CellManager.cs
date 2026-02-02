using System.Collections.Generic;
using UnityEngine;
using HM;
using HM.Containers;
using HM.Utils;

namespace Battle
{
    /// <summary>
    /// 통합 셀 관리자
    /// - 셀 상태 관리 (Poison, Slime, Fire, SmokeBomb 등)
    /// - 경로 찾기 및 데미지 필드 그래프 관리
    /// - 캐릭터 셀 이벤트 처리 (Enter/Stay/Exit)
    /// - Duration 관리는 BattleTurnController를 통해 턴 종료 시 자동 처리
    /// </summary>
    public class CellManager : ITurnEventHandler
    {
        /// <summary>
        /// ITurnEventHandler 구현 - 핸들러 이름
        /// </summary>
        public string HandlerName => "CellManager";

        #region Fields - Cell State Management

        private BattleSystem _battleSystem;
        private Dictionary<Vector2Int, Cell> _cells = new Dictionary<Vector2Int, Cell>();
        private CellEffectListener _cellEffectListener;

        #endregion

        #region Fields - Damage Calculation & Pathfinding

        // 경로 찾기 그래프 (0: 위, 1: 아래, 2: 좌, 3: 우)
        private bool[,,] _pathfindingGraph;

        // 데미지 필드 그래프 - 각 셀에 공격 가능한 캐릭터 목록
        private List<BaseCharacter>[,] _damageFieldGraph;

        // 데미지 계산 범주 내부 변수
        private int _gridSizeX;
        private int _gridSizeY;
        private int _gridOffsetX;
        private int _gridOffsetY;

        #endregion

        public CellManager(BattleSystem battleSystem)
        {
            _battleSystem = battleSystem;
            _cellEffectListener = new CellEffectListener(battleSystem);

            // 데미지 그래프 초기화
            InitializeDamageCalculationFields();
            ClearDamageGraph();
        }

        /// <summary>
        /// 데미지 계산 관련 필드 초기화
        /// </summary>
        private void InitializeDamageCalculationFields()
        {
            _gridSizeX = Mathf.Abs(GameManager.Instance.MinGridx - GameManager.Instance.MaxGridx) + 1;
            _gridSizeY = Mathf.Abs(GameManager.Instance.MinGridy - GameManager.Instance.MaxGridy) + 1;
            _gridOffsetX = GameManager.Instance.MaxGridx;
            _gridOffsetY = GameManager.Instance.MaxGridy;
        }

        /// <summary>
        /// 셀에 상태 추가 (영구)
        /// 셀이 존재하지 않으면 자동 생성
        /// </summary>
        public void AddCellState(Vector2Int position, CellState state)
        {
            if (!_cells.TryGetValue(position, out Cell cell))
            {
                cell = new Cell(position);
                _cells.Add(position, cell);
            }

            cell.AddState(state);
            Debug.Log($"[INFO] CellManager::AddCellState() - Position: {position}, State: {state}");
        }

        /// <summary>
        /// 셀에 상태 추가 (Duration 포함)
        /// 셀이 존재하지 않으면 자동 생성
        /// </summary>
        public void SetCellState(Vector2Int position, CellState state, int duration)
        {
            if (!_cells.TryGetValue(position, out Cell cell))
            {
                cell = new Cell(position);
                _cells.Add(position, cell);
            }

            cell.SetState(state, duration);
            Debug.Log($"[INFO] CellManager::SetCellState() - Position: {position}, State: {state}, Duration: {duration}");
        }

        /// <summary>
        /// 셀에서 상태 제거
        /// </summary>
        public void RemoveCellState(Vector2Int position, CellState state)
        {
            if (_cells.TryGetValue(position, out Cell cell))
            {
                cell.RemoveState(state);

                // 모든 상태가 제거되면 셀도 제거
                if (!cell.HasAnyState())
                {
                    _cells.Remove(position);
                }

                Debug.Log($"[INFO] CellManager::RemoveCellState() - Position: {position}, State: {state}");
            }
        }

        /// <summary>
        /// 특정 위치의 셀 반환
        /// </summary>
        public Cell GetCell(Vector2Int position)
        {
            _cells.TryGetValue(position, out Cell cell);
            return cell;
        }

        /// <summary>
        /// 특정 위치에 특정 상태가 있는지 확인
        /// </summary>
        public bool HasCellState(Vector2Int position, CellState state)
        {
            if (_cells.TryGetValue(position, out Cell cell))
            {
                return cell.HasState(state);
            }
            return false;
        }

        /// <summary>
        /// 캐릭터가 셀에 진입할 때 호출
        /// EventManager를 통해 OnCellEnter 이벤트 발생
        /// </summary>
        public void OnCharacterEnterCell(BaseCharacter character, Vector2Int position)
        {
            if (_cells.TryGetValue(position, out Cell cell))
            {
                if (cell.HasAnyState())
                {
                    Debug.Log($"[INFO] CellManager::OnCharacterEnterCell() - Character: {character.CharacterName}, Position: {position}, States: {string.Join(", ", cell.GetStates())}");

                    // OnCellEnter 이벤트 발생
                    EventManager.Instance.InvokeEvent(
                        HM.EventType.OnCellEnter,
                        _battleSystem,
                        new CellEventData { Character = character, Position = position, Cell = cell }
                    );
                }
            }
        }

        /// <summary>
        /// 캐릭터가 셀에 머물 때 호출 (턴 시작 등)
        /// EventManager를 통해 OnCellStay 이벤트 발생
        /// </summary>
        public void OnCharacterStayOnCell(BaseCharacter character, Vector2Int position)
        {
            if (_cells.TryGetValue(position, out Cell cell))
            {
                if (cell.HasAnyState())
                {
                    Debug.Log($"[INFO] CellManager::OnCharacterStayOnCell() - Character: {character.CharacterName}, Position: {position}, States: {string.Join(", ", cell.GetStates())}");

                    // OnCellStay 이벤트 발생
                    EventManager.Instance.InvokeEvent(
                        HM.EventType.OnCellStay,
                        _battleSystem,
                        new CellEventData { Character = character, Position = position, Cell = cell }
                    );
                }
            }
        }

        /// <summary>
        /// 캐릭터가 셀을 떠날 때 호출
        /// EventManager를 통해 OnCellExit 이벤트 발생
        /// </summary>
        public void OnCharacterExitCell(BaseCharacter character, Vector2Int position)
        {
            if (_cells.TryGetValue(position, out Cell cell))
            {
                if (cell.HasAnyState())
                {
                    Debug.Log($"[INFO] CellManager::OnCharacterExitCell() - Character: {character.CharacterName}, Position: {position}, States: {string.Join(", ", cell.GetStates())}");

                    // OnCellExit 이벤트 발생
                    EventManager.Instance.InvokeEvent(
                        HM.EventType.OnCellExit,
                        _battleSystem,
                        new CellEventData { Character = character, Position = position, Cell = cell }
                    );
                }
            }
        }

        /// <summary>
        /// 모든 셀 상태 초기화
        /// </summary>
        public void ClearAllCells()
        {
            _cells.Clear();
            Debug.Log("[INFO] CellManager::ClearAllCells() - All cells cleared");
        }

        /// <summary>
        /// 특정 위치의 셀 제거
        /// </summary>
        public void RemoveCell(Vector2Int position)
        {
            if (_cells.Remove(position))
            {
                Debug.Log($"[INFO] CellManager::RemoveCell() - Position: {position}");
            }
        }

        #region ITurnEventHandler Implementation

        /// <summary>
        /// ITurnEventHandler 구현 - 턴 시작 시 호출
        /// 현재는 사용하지 않음
        /// </summary>
        public void OnTurnStart()
        {
            // 턴 시작 시 셀 관련 처리가 필요하면 여기에 구현
            // 예: 캐릭터들이 현재 위치의 셀에 머물러 있을 때의 효과 처리
            // OnCharacterStayOnCell 호출 등
        }

        /// <summary>
        /// ITurnEventHandler 구현 - 턴 종료 시 호출
        /// BattleTurnController에서 자동으로 호출됨
        /// </summary>
        public void OnTurnEnd()
        {
            ProcessTurnEnd();
        }

        /// <summary>
        /// 턴 종료 시 모든 셀의 Duration 감소 및 만료된 상태 제거
        /// </summary>
        private void ProcessTurnEnd()
        {
            List<Vector2Int> cellsToRemove = new List<Vector2Int>();

            Debug.Log($"[INFO] CellManager::ProcessTurnEnd() - Processing {_cells.Count} cells");

            foreach (var kvp in _cells)
            {
                Vector2Int position = kvp.Key;
                Cell cell = kvp.Value;

                // 셀의 모든 상태 Duration 감소
                var expiredStates = cell.DecrementDurations();

                if (expiredStates.Count > 0)
                {
                    Debug.Log($"[INFO] CellManager::ProcessTurnEnd() - Position: {position}, Expired States: {string.Join(", ", expiredStates)}");
                }

                // 모든 상태가 제거되면 셀도 제거 대상
                if (!cell.HasAnyState())
                {
                    cellsToRemove.Add(position);
                }
            }

            // 빈 셀 제거
            foreach (var position in cellsToRemove)
            {
                _cells.Remove(position);
                Debug.Log($"[INFO] CellManager::ProcessTurnEnd() - Removed empty cell at Position: {position}");
            }
        }

        #endregion

        #region Pathfinding & Damage Graph

        /// <summary>
        /// 경로 찾기 그래프 설정
        /// </summary>
        private void SetPathfindingGraph(List<Vector2Int> characterPositions, List<WallData> walls)
        {
            _pathfindingGraph = PathFindingUtils.GetMapGraph(characterPositions, walls, true);
        }

        /// <summary>
        /// 데미지 필드 그래프 생성
        /// 모든 캐릭터의 공격 가능 위치 계산
        /// </summary>
        public List<BaseCharacter>[,] SetDamageFieldGraph(List<BaseCharacter> characters)
        {
            ClearDamageGraph();

            // 캐릭터 위치 목록 생성
            List<Vector2Int> characterPositions = new List<Vector2Int>();
            foreach (var character in characters)
            {
                characterPositions.Add(character.Position);
            }

            // 경로 찾기 그래프 설정
            SetPathfindingGraph(characterPositions, GameManager.Instance.WallList);

            // 각 캐릭터의 공격 가능 위치 계산
            foreach (var character in characters)
            {
                CalculateCharacterDamageField(character);
            }

            return _damageFieldGraph;
        }

        /// <summary>
        /// 개별 캐릭터의 데미지 필드 계산 (내부 전용)
        /// </summary>
        private void CalculateCharacterDamageField(BaseCharacter character)
        {
            List<Vector2Int> attackablePositions = character.characterStat.AttackablePositions;
            Vector2Int characterPosition = character.Position;

            foreach (var attackableOffset in attackablePositions)
            {
                Vector2Int targetPosition = characterPosition + attackableOffset;

                // 경로 찾기로 도달 가능 여부 확인
                if (!CanReachPosition(characterPosition, targetPosition))
                {
                    continue;
                }

                // 그리드 좌표를 배열 인덱스로 변환
                int arrayX = ConvertToArrayIndexX(targetPosition.x);
                int arrayY = ConvertToArrayIndexY(targetPosition.y);

                // 배열 범위 검증
                if (IsValidArrayIndex(arrayX, arrayY))
                {
                    _damageFieldGraph[arrayX, arrayY].Add(character);
                }
            }
        }

        /// <summary>
        /// 경로 찾기로 위치 도달 가능 여부 확인 (내부 전용)
        /// </summary>
        private bool CanReachPosition(Vector2Int from, Vector2Int to)
        {
            return PathFindingUtils.CanReachWithoutStuckInWall(_pathfindingGraph, from, to, true);
        }

        /// <summary>
        /// 그리드 X 좌표를 배열 인덱스로 변환 (내부 전용)
        /// </summary>
        private int ConvertToArrayIndexX(int gridX)
        {
            return gridX + _gridOffsetX;
        }

        /// <summary>
        /// 그리드 Y 좌표를 배열 인덱스로 변환 (내부 전용)
        /// </summary>
        private int ConvertToArrayIndexY(int gridY)
        {
            return gridY + _gridOffsetY;
        }

        /// <summary>
        /// 배열 인덱스 유효성 검증 (내부 전용)
        /// </summary>
        private bool IsValidArrayIndex(int arrayX, int arrayY)
        {
            return arrayX >= 0 && arrayX < _gridSizeX && arrayY >= 0 && arrayY < _gridSizeY;
        }

        /// <summary>
        /// 특정 위치의 데미지 그래프 캐릭터 목록 반환 (int 좌표)
        /// </summary>
        public List<BaseCharacter> GetDamageGraphCellCharacters(int posX, int posY)
        {
            int arrayX = ConvertToArrayIndexX(posX);
            int arrayY = ConvertToArrayIndexY(posY);

            if (IsValidArrayIndex(arrayX, arrayY))
            {
                return _damageFieldGraph[arrayX, arrayY];
            }

            return new List<BaseCharacter>();
        }

        /// <summary>
        /// 특정 위치의 데미지 그래프 캐릭터 목록 반환 (Vector2Int)
        /// </summary>
        public List<BaseCharacter> GetDamageGraphCellCharacters(Vector2Int position)
        {
            return GetDamageGraphCellCharacters(position.x, position.y);
        }

        /// <summary>
        /// 데미지 그래프 초기화
        /// </summary>
        public void ClearDamageGraph()
        {
            _pathfindingGraph = new bool[_gridSizeX, _gridSizeY, 4];
            _damageFieldGraph = new List<BaseCharacter>[_gridSizeX, _gridSizeY];

            for (int x = 0; x < _gridSizeX; x++)
            {
                for (int y = 0; y < _gridSizeY; y++)
                {
                    _damageFieldGraph[x, y] = new List<BaseCharacter>();
                }
            }
        }
        #endregion

        /// <summary>
        /// 셀 이벤트 데이터 구조
        /// </summary>
        public class CellEventData
        {
            public BaseCharacter Character;
            public Vector2Int Position;
            public Cell Cell;
        }
    }
}
