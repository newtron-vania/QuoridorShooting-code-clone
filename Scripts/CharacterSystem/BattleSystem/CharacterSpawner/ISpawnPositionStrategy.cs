using System;
using System.Collections.Generic;
using HM.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public interface ISpawnPositionStrategy
{
    /// <summary> 유효한 위치를 반환하며, 사용된 위치 목록을 고려함 </summary>
    Vector2Int GetNextValidPosition(BaseCharacter character, List<Vector2Int> usedPositions);
}

public class FixedPositionStrategy : ISpawnPositionStrategy
{
    private readonly Queue<Vector2Int> _positions;

    public FixedPositionStrategy(IEnumerable<Vector2Int> positions)
    {
        _positions = new Queue<Vector2Int>(positions);
    }

    public Vector2Int GetNextValidPosition(BaseCharacter character, List<Vector2Int> usedPositions)
    {
        while (_positions.Count > 0)
        {
            var pos = _positions.Dequeue();

            if (!usedPositions.Contains(pos) && PathFindingUtils.IsPositionValidWithOffset(pos))
                return pos;

            Debug.LogWarning($"[Warning] FixedPositionStrategy::GetNextValidPosition():위치 {pos} 는 이미 사용 중이거나 유효하지 않습니다.");
        }

        throw new Exception("[NullException] FixedPositionStrategy::GetNextValidPosition():사용할 수 있는 고정 위치가 더 이상 없습니다.");
    }
}


public class RandomInRangeStrategy : ISpawnPositionStrategy
{
    private readonly int _xMin, _xMax, _yMin, _yMax;

    public RandomInRangeStrategy(int xMin, int xMax, int yMin, int yMax)
    {
        _xMin = xMin;
        _xMax = xMax;
        _yMin = yMin;
        _yMax = yMax;
    }

    public Vector2Int GetNextValidPosition(BaseCharacter character, List<Vector2Int> usedPositions)
    {
        const int maxAttempts = 100;
        int attempt = 0;

        while (attempt < maxAttempts)
        {
            var pos = new Vector2Int(
                Random.Range(_xMin, _xMax + 1),
                Random.Range(_yMin, _yMax + 1)
            );

            if (!usedPositions.Contains(pos) && PathFindingUtils.IsPositionValidWithOffset(pos))
                return pos;

            attempt++;
        }

        throw new Exception("[NullException] RandomInRangeStrategy::GetNextValidPosition():유효한 위치를 찾을 수 없습니다.");
    }
}