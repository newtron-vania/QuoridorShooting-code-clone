using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

// 벽에 대한 여러 정보를 저장하는 struct
public struct WallData
{
    private Vector3 _worldPosition;
    public Vector2Int Position => new Vector2Int(Mathf.FloorToInt(_worldPosition.x / GameManager.GridSize), Mathf.FloorToInt(_worldPosition.y / GameManager.GridSize));
    public bool IsHorizontal;

    public WallData(Vector3 worldPosition, bool isHorizontal)
    {
        _worldPosition = worldPosition;
        IsHorizontal = isHorizontal;
    }
    
    public static bool operator ==(WallData left, WallData right)
    {
        return left.Position == right.Position && left.IsHorizontal == right.IsHorizontal;
    }

    public static bool operator !=(WallData left, WallData right)
    {
        return !(left == right);
    }
    
    public override bool Equals(object obj)
    {
        if (obj is WallData other)
        {
            return this == other;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Position.GetHashCode();
            hash = hash * 23 + IsHorizontal.GetHashCode();
            return hash;
        }
    }
    
}