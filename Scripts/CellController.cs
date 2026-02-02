using System.Collections;
using System.Collections.Generic;
using HM.Containers;
using HM.Utils;
using Unity.Mathematics;
using UnityEngine;

public class CellController
{
    // 0 : 위    1 : 아래  2 : 좌 3 : 우
    private bool[,,] _cellGraph;
    private List<BaseCharacter>[,] _cellDamageGraph;
    
    public CellController()
    {
        Clear();
    }

    private void SetCellGraph(List<Vector2Int> characterPositions, List<WallData> walls)
    {
        _cellGraph = PathFindingUtils.GetMapGraph(characterPositions, walls, true);
    }

    public List<BaseCharacter>[,] SetDamageFieldGraph(List<BaseCharacter> characters)
    {
        Clear();

        List<Vector2Int> characterPositions = new List<Vector2Int>();
        foreach (var character in characters)
        {
            characterPositions.Add(character.Position);
        }

        SetCellGraph(characterPositions, GameManager.Instance.WallList);

        foreach (var character in characters)
        {
            List<Vector2Int> characterAttackablePositions = character.characterStat.AttackablePositions;
            Vector2Int characterPosition = character.Position;

            foreach (var attackablePosition in characterAttackablePositions)
            {
                Vector2Int targetPosition = characterPosition + attackablePosition;

                bool canReach = PathFindingUtils.CanReachWithoutStuckInWall(_cellGraph, characterPosition, targetPosition, true);
                if (!canReach) continue;

                _cellDamageGraph[targetPosition.x + 4, targetPosition.y + 4].Add(character);
            }
        }

        return _cellDamageGraph;
    }
    public List<BaseCharacter> GetDamageGraphCellCharacters(int posX, int posY)
    {
        posX += GameManager.Instance.MaxGridx;
        posY += GameManager.Instance.MaxGridy;

        return _cellDamageGraph[posX, posY];
    }
    
    public List<BaseCharacter> GetDamageGraphCellCharacters(Vector2Int position)
    {
        position.x += 4;
        position.y += 4;
        if (PathFindingUtils.IsPositionValid(position))
        {
            return _cellDamageGraph[position.x, position.y];
        }
        else
        {
            return new List<BaseCharacter>();
        }
    }
    

    public void Clear()
    {
        int sizeX = Mathf.Abs(GameManager.Instance.MinGridx - GameManager.Instance.MaxGridx) + 1;
        int sizeY = Mathf.Abs(GameManager.Instance.MinGridy - GameManager.Instance.MaxGridy) + 1;
        _cellGraph = new bool[sizeX ,sizeY, 4];
        _cellDamageGraph = new List<BaseCharacter>[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                _cellDamageGraph[x, y] = new List<BaseCharacter>();
            }
        }
    }
}
