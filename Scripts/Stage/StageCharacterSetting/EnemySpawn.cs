using System.Collections.Generic;

// 적 타입 어차피 임시라 편의를 위해 stage.cs로 옮김
//public enum EnemyRank { Normal, Champion, Boss }
//public enum EnemyType { Swordsman, Spearman, Gunner, Cavalry, Shielder }
public enum EnemyRank { Normal, Named, Champion, Boss }
public enum EnemyType { Swordsman, Spearman, Gunner, Cavalry, Shielder }

// 개별 적 정보 - 임시(이미 존재하는 캐릭터 관련 코드로 대체할 것)
public class EnemySpawnData
{
    public EnemyRank Rank;
    public EnemyType Type;
    public bool IsElite;
    public List<EnemySpawnData> EnemyList = new List<EnemySpawnData>();

    public EnemySpawnData(EnemyRank rank, EnemyType type)
    {
        Rank = rank;
        Type = type;
        IsElite = false;
    }
}