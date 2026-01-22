using System;
using System.Collections;
using System.Collections.Generic;
using CharacterDefinition;
using HM.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyCharacterSpawnerOfTile
{
    private int _randSeed;

    private int _maxEnemyCount = 4;

    public EnemyCharacterSpawnerOfTile(int seed)
    {
        _randSeed = seed;
        SetRandomSeed();
    }

    public EnemyCharacterSpawnerOfTile()
    {
        _randSeed = (int)DateTime.Now.Ticks;
        SetRandomSeed();
    }

    public List<CharacterData> SpawnEnemyCharacters()
    {
        int enemyCount = Random.Range(1, _maxEnemyCount);
        var enemyDatas = new List<CharacterData>();
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyId = Random.Range(0, DataManager.Instance.EnemyCharacterCount);
            var enemyInfo = DataManager.Instance.GetEnemyCharacterInfo(enemyId + DataManager.Instance.PlayableCharacterCount);
            enemyDatas.Add(enemyInfo);
            // 추가 능력치 수정 여부 체크
            
        }

        return enemyDatas;
    }


    public void SetRandomSeed()
    {
        Random.InitState((int)_randSeed);
    }
}