using System.Collections;
using System.Collections.Generic;
using HM.Containers;
using HM.Utils;
using Unity.Mathematics;
using UnityEngine;

public class TileController
{
    // 0 : 위    1 : 아래  2 : 좌 3 : 우
    private bool[,,] _tileGraph;
    private List<BaseCharacter>[,] _tileDamageGraph;
    
    public TileController()
    {
        Clear();
    }

    private void SetTileGraph(List<Vector2Int> characterPositions, List<WallData> walls)
    {
        _tileGraph = PathFindingUtils.GetMapGraph(characterPositions, walls, true);
    }

    public List<BaseCharacter>[,] SetDamageFieldGraph(List<BaseCharacter> characters)
    {
        Clear();

        List<Vector2Int> characterPositions = new List<Vector2Int>();
        foreach (var character in characters)
        {
            characterPositions.Add(character.Position);
        }

        SetTileGraph(characterPositions, GameManager.Instance.WallList);

        foreach (var character in characters)
        {
            List<Vector2Int> characterAttackablePositions = character.characterStat.AttackablePositions;
            Vector2Int characterPosition = character.Position;

            foreach (var attackablePosition in characterAttackablePositions)
            {
                Vector2Int targetPosition = characterPosition + attackablePosition;

                bool canReach = PathFindingUtils.CanReachWithoutStuckInWall(_tileGraph, characterPosition, targetPosition, true);
                if (!canReach) continue;

                _tileDamageGraph[targetPosition.x + 4, targetPosition.y + 4].Add(character);
            }
        }

        return _tileDamageGraph;
    }
    public List<BaseCharacter> GetDamageGraphTileCharacters(int posX, int posY)
    {
        posX += GameManager.Instance.MaxGridx;
        posY += GameManager.Instance.MaxGridy;

        return _tileDamageGraph[posX, posY];
    }
    
    public List<BaseCharacter> GetDamageGraphTileCharacters(Vector2Int position)
    {
        position.x += 4;
        position.y += 4;
        if (PathFindingUtils.IsPositionValid(position))
        {
            return _tileDamageGraph[position.x, position.y];
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
        _tileGraph = new bool[sizeX ,sizeY, 4];
        _tileDamageGraph = new List<BaseCharacter>[sizeX, sizeY];
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                _tileDamageGraph[x, y] = new List<BaseCharacter>();
            }
        }
    }
}
