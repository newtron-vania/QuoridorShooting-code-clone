using System;
using System.Collections.Generic;
using System.Linq;
using HM.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

//characterController의 코드와 호환되도록 구현했습니다.
/*
 현재 각 캐릭터는 턴마다 오프셋으로 지정된 이동 가능한 영역 내에서 한 번 이동하므로
따로 이동력이라는 개념이 존재하지 않으며, 이는 같은 데이터로 관리되는 각 캐릭터의 사거리도
마찬가지입니다. 따라서 직관적인 구분이 불가능하므로 id별로 따로 맵핑해 구분하였습니다.

사거리에 배치에 영향을 주는 Tactical placement는 elite전투에서 사용되는데, 다음과 같이 구분합니다.
 - Melee (Front lines)
 - Mid-range (Middle lines)
 - Long-range (Back lines)
 */
public class ChallengeBattleStageEnemyPlacement : ISpawnPositionStrategy
{
    public enum PlacementType
    {
        Tactical, //전열 근접, 후열 원거리 (-4, 4, 1, 4)
        Surround, //player characters가 적에게 둘러쌓이도록(ambush 느낌) (this, -4, 4, -4, 4)
        Random //원래 그랬던 것처럼 그냥 랜덤 (-4, 4, 2, 4)
    }

    private readonly BattleSystem _controller;
    private readonly PlacementType _type;
    private readonly int _xMin, _xMax, _yMin, _yMax;

    public ChallengeBattleStageEnemyPlacement(BattleSystem controller) {
        _controller = controller;
        _type = SetupPlacementTypeStrategy();
        if (StageManager.Instance.StageDic.TryGetValue(StageManager.Instance.CurStageId, out var currentStage)) {
            if (currentStage.Type == Stage.StageType.Elite) {
                //캐릭터의 사거리에 따라 그냥 랜덤보다 더 가까이에 배치될 수 있음.
                _xMin = -4; _xMax = 4; _yMin = 1; _yMax = 4;
            }
            else if (currentStage.Type == Stage.StageType.Ambush) {
                _xMin = -4; _xMax = 4; _yMin = -4; _yMax = 4;
            }
            else {
                _xMin = -4; _xMax = 4; _yMin = 2; _yMax = 4;
            }
        }
    }

    //여기서 스테이지 타입을 보고 placement type을 결정합니다.
    public PlacementType SetupPlacementTypeStrategy() {
        var placementType = PlacementType.Random; //normal battle
        if (StageManager.Instance != null) {
            if (StageManager.Instance.StageDic.TryGetValue(StageManager.Instance.CurStageId, out var currentStage)) {
                if (currentStage.Type == Stage.StageType.Elite) {
                    placementType = PlacementType.Tactical;
                    Debug.Log("[INFO] CharacterController.CharacterInstantiator::SpawnAllEnemies - Elite Stage, Tactical");
                }
                else if (currentStage.Type == Stage.StageType.Ambush) {
                    placementType = PlacementType.Surround;
                    Debug.Log("[INFO] CharacterController.CharacterInstantiator::SpawnAllEnemies - Ambush Stage, Surround");
                }
            }
        }
        return placementType;
    }

    //현재의 battleStage가 어떻게 만들어졌는지(즉 stage type에 딷른 placement type)에 따라 다르게 위치를 뽑습니다.
    //BaseCharacterSpawnManager.cs에서 캐릭터마다 한번씩 호출되어 위치를 정해줍니다.
    public Vector2Int GetNextValidPosition(BaseCharacter character, List<Vector2Int> usedPositions) {
        switch (_type) {
            case PlacementType.Tactical:
                return GetTacticalPosition(character, usedPositions);
            case PlacementType.Surround:
                return GetSurroundPosition(character, usedPositions);
            case PlacementType.Random:
            default:
                return GetRandomPosition(character, usedPositions);
        }
    }

    //id별로 사거리 구분, 7은 불가능("7": [])하므로 제외.
    private readonly HashSet<int> _meleeIds = new() { 0, 1, 4, 8, 13, 23, 24, 26 };
    private readonly HashSet<int> _midRangeIds = new() { 2, 3, 9, 10, 14, 15, 16, 17, 20, 21, 22, 27, 29, 31 };
    private readonly HashSet<int> _longRangeIds = new() { 5, 6, 11, 12, 18, 19, 25, 28, 30 };

    private enum RangeCategory { Melee, Mid, Long, Unknown }

    private RangeCategory GetRangeCategory(int id) {
        if (_meleeIds.Contains(id)) return RangeCategory.Melee;
        if (_midRangeIds.Contains(id)) return RangeCategory.Mid;
        if (_longRangeIds.Contains(id)) return RangeCategory.Long;
        return RangeCategory.Unknown;
    }

    private Vector2Int GetTacticalPosition(BaseCharacter character, List<Vector2Int> usedPositions) {
        var category = GetRangeCategory(character.AttackRangeId);
        List<int> preferredRows;

        switch (category) {
            case RangeCategory.Melee:
                //앞에서부터 뒤로
                preferredRows = Enumerable.Range(_yMin, _yMax - _yMin + 1).ToList();
                break;
            case RangeCategory.Long:
                //뒤에서부터 앞으로
                preferredRows = Enumerable.Range(_yMin, _yMax - _yMin + 1).Reverse().ToList();
                break;
            case RangeCategory.Mid:
            default:
                //중앙에서 바깥으로
                int midY = (_yMin + _yMax) / 2;
                preferredRows = Enumerable.Range(_yMin, _yMax - _yMin + 1).OrderBy(y => Mathf.Abs(y - midY)).ToList();
                break;
        }

        foreach (var y in preferredRows) {
            if (y < _yMin || y > _yMax) continue;

            var xPositions = Enumerable.Range(_xMin, _xMax - _xMin + 1).OrderBy(x => Random.value).ToList();

            foreach (var x in xPositions) {
                var pos = new Vector2Int(x, y);
                if (IsValid(pos, usedPositions)) return pos;
            }
        }

        return GetRandomPosition(character, usedPositions);
    }

    private Vector2Int GetSurroundPosition(BaseCharacter character, List<Vector2Int> usedPositions) {
        var playerCharacters = _controller.StageCharacter[CharacterDefinition.CharacterIdentification.Player];
        Vector2 center = Vector2.zero;
        if (playerCharacters.Count > 0) {
            foreach (var p in playerCharacters) center += p.Position;
            center /= playerCharacters.Count;
        }
        else {
            center = new Vector2(0, -3.5f);
        }

        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = _xMin; x <= _xMax; x++) {
            for (int y = _yMin; y <= _yMax; y++) {
                candidates.Add(new Vector2Int(x, y));
            }
        }

        //360도 랜덤 방향에서부터 시계방향으로 정렬, 같은 각도면 멀리 있는 순서
        float targetAngle = Random.Range(0f, 360f);
        candidates = candidates.OrderBy(p => {
            Vector2 dir = new Vector2(p.x, p.y) - center;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Mathf.Abs(Mathf.DeltaAngle(angle, targetAngle));
        })
        .ThenByDescending(p => Vector2.Distance(p, center))
        .ToList();

        foreach (var pos in candidates) {
            if (IsValid(pos, usedPositions)) return pos;
        }

        return GetRandomPosition(character, usedPositions);
    }

    //원본인 ISpawnPositionStrategy.cs와 거의 유사함
    private Vector2Int GetRandomPosition(BaseCharacter character, List<Vector2Int> usedPositions) {
        const int maxAttempts = 100;
        int attempt = 0;

        while (attempt < maxAttempts) {
            var pos = new Vector2Int(
                Random.Range(_xMin, _xMax + 1),
                Random.Range(_yMin, _yMax + 1)
            );

            if (IsValid(pos, usedPositions)) return pos;
            attempt++;
        }

        for (int x = _xMin; x <= _xMax; x++) {
            for (int y = _yMin; y <= _yMax; y++) {
                var pos = new Vector2Int(x, y);
                if (IsValid(pos, usedPositions)) return pos;
            }
        }
        throw new Exception("[ERROR]ChallengeBattleStageEnemyPlacement::GetRandomPosition(), No valid position found.");
    }

    private bool IsValid(Vector2Int pos, List<Vector2Int> usedPositions) {
        return !usedPositions.Contains(pos) && PathFindingUtils.IsPositionValidWithOffset(pos);
    }
}
