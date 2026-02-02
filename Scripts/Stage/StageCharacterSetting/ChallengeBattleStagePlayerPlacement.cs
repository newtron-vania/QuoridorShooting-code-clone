using System;
using System.Collections.Generic;
using System.Linq;
using HM.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

//ChallengeBattleStageEnemyPlacement.cs 코드 재활용
public class ChallengeBattleStagePlayerPlacement : ISpawnPositionStrategy
{
    private readonly BattleSystem _controller;
    private readonly bool _isAmbush;
    private readonly int _xMin, _xMax, _yMin, _yMax;

    public ChallengeBattleStagePlayerPlacement(BattleSystem controller)
    {
        _controller = controller;
        
        // 일반/엘리트 전투: 기존과 동일하게 하단 랜덤 배치, 기습전투일 때만 가운데에 모이도록
        _xMin = -4; _xMax = 4; _yMin = -4; _yMax = -3;
        _isAmbush = false;
        if (StageManager.Instance != null && StageManager.Instance.StageDic.TryGetValue(StageManager.Instance.CurStageId, out var currentStage))
        {
            if (currentStage.Type == Stage.StageType.Ambush)
            {
                //Debug.Log("[INFO] ChallengeBattleStagePlayerPlacement - Ambush Stage, Center Placement");
                _isAmbush = true;
                _xMin = -4; _xMax = 4; _yMin = -4; _yMax = 4; 
            }
        }
    }

    public Vector2Int GetNextValidPosition(BaseCharacter character, List<Vector2Int> usedPositions)
    {
        if (_isAmbush)
            return GetAmbushPosition(character, usedPositions);
        else
            return GetRandomPosition(character, usedPositions);
    }

    private Vector2Int GetAmbushPosition(BaseCharacter character, List<Vector2Int> usedPositions)
    {
        Vector2 centerOfMap = Vector2.zero;
        var candidates = new List<Vector2Int>();
        for (int x = _xMin; x <= _xMax; x++)
        {
            for (int y = _yMin; y <= _yMax; y++)
            {
                candidates.Add(new Vector2Int(x, y));
            }
        }

        //중앙과 거리가 가까운 순서대로 정렬
        /*
         다닥다닥 붙어있는 모양새이므로 추후에 수정 필요성 있음
         */
        var sortedCandidates = candidates.OrderBy(pos => Vector2.Distance(pos, centerOfMap)).ToList();
        foreach (var pos in sortedCandidates)
        {
            if (IsValid(pos, usedPositions)) return pos;
        }

        return GetRandomPosition(character, usedPositions);
    }

    private Vector2Int GetRandomPosition(BaseCharacter character, List<Vector2Int> usedPositions)
    {
        const int maxAttempts = 100;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            var pos = new Vector2Int(
                Random.Range(_xMin, _xMax + 1),
                Random.Range(_yMin, _yMax + 1)
            );

            if (IsValid(pos, usedPositions)) return pos;
            attempt++;
        }

        for (int x = _xMin; x <= _xMax; x++)
        {
            for (int y = _yMin; y <= _yMax; y++)
            {
                var pos = new Vector2Int(x, y);
                if (IsValid(pos, usedPositions)) return pos;
            }
        }
        
        throw new Exception("[ERROR] ChallengeBattleStagePlayerPlacement::GetRandomPosition(), No valid position found.");
    }

    private bool IsValid(Vector2Int pos, List<Vector2Int> usedPositions)
    {
        return !usedPositions.Contains(pos) && PathFindingUtils.IsPositionValidWithOffset(pos);
    }
}