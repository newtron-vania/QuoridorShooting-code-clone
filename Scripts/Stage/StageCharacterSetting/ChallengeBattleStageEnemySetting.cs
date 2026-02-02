using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
 * 챕터별 적 등장 수와 종류를 확률에 따라 생성해 전달합니다.
 * CharacterController.CharacterInstantiator.cs의 SpawnCharacters()
 -> SpawnAllEnemies() : RandomInRangeStrategy()로 enemy가 배치되는 범위 결정
 -> BaseCharacterSpawnManager.cs의 CharacterSpawnManager SpawnEnemyCharacters()
    : 적 정보 data setting, GetNextValidPosition()로 배치()
 * 그렇다면 이때 필요한 data setting은 어디서 가져오는가?
    TileCharacter Spawner.cs의 SpawnEnemyCharacters에서 가져온다.
    따라서 여기서 생성한 건 아래와 같은 SpawnEnemyCharacters에서()의 형식을 유지해야 한다.

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
    //여기서 PlayableCharacterCount을 더해주는 건 현재 사용가능한 character를 제외한 나머지를 enemy를 가정하는듯 
*/

/* 전체 호출순서는 아래와 같습니다.
1. 맵 진입
플레이어가 스테이지 선택-> MapPlayer.EnterStage() -> BattleScene 로드.

2. 배틀 씬 로딩
CharacterController가 초기화 -> CharacterController.SpawnCharacters() → SpawnAllEnemies()

3. 적 데이터 로드, 배치
SpawnAllEnemies() -> (spawner, strategy, placer 인자를 넘겨주며) -> CharacterSpawnManager.SpawnEnemyCharacters()
CharacterSpawnManager -> CharacterLoader.LoadEnemyCharacters() -> EnemyDataProvider.Load()
//EnemyDataProvider.Load() 이걸 수정해서 ChallengeBattleStageGeneration를 만들고 스테이지 정보에 따라 다르게 적 초기화.
가져온 적 데이터를 RandomInRangeStrategy를 이용해 배치
*/
public class ChallengeBattleStageEnemyInitializer
{
    //가중치를 더해서 확률적으로 적의 수와 종류를 뽑습니다.
    protected Dictionary<EnemyType, int> _enemyTypeProbabilityDict = new Dictionary<EnemyType, int>();
    protected Dictionary<int, int> _enemyCountProbabilityDict = new Dictionary<int, int>();
    //수정 필요...
    private CharacterData GetEnemyDataFromManager(EnemyType type, EnemyRank rank)
    {
        /*
        이런 느낌으로 캐릭터가 저장되어 있습니다
        "Index": 9, //~12 //업데이트됨에 따라 27~47번으로 바뀔 예정입니다.
        "Name": "와흐라트 병사",
        "Description": null,
        "Class": "Element2", //이게 rank에 해당한다면 전부 수정 필요
        "Type": "Tanker", //현재 사용하는 type과 일치하지 않으니 전부 수정 필요
        "Hp": 13,
        "Atk": 5.0,
        "Avd": 0.3,
        "ApRecovery": 50,
        "SkillId": 12,
        "MoveRangeId": 29, //확인해보니 json으로 맵핑되어 있음.
        "AttackRangeId": 14
        */
        int typeOffset = (int)type;
        int rankOffset = (int)rank;

        // 데이터상에 랭크별로 캐릭터가 나뉘어 있는 경우의 로직
        // 일단 type에 따라 9~12를 기본으로 쓰고... 여기 넘어가면 오류.(로드 못해서 바로 배틀이 끝나는 문제)
        // 현재 캐릭터 데이터에 rank는 빠져 있으니 능력치 보정으로 처리함(rpg색놀이처럼)
        int enemyCount = DataManager.Instance.EnemyCharacterCount;
        if (enemyCount <= 0) enemyCount = 4;
        int targetEnemyId = DataManager.Instance.PlayableCharacterCount + ((typeOffset + rankOffset) % enemyCount);
        // 만약 targetEnemyId가 유효한 범위라면 그걸 사용, 아니면 base 사용
        var enemyInfo = DataManager.Instance.GetEnemyCharacterInfo(targetEnemyId);
        if (enemyInfo == null)
        {
            Debug.LogError($"[ERROR] ChallengeBattleStageEnemyInitializer::GetEnemyDataFromManager: Failed to get enemy data, ID: {targetEnemyId}");
            return null;
        }

        // Rank에 따라 능력치는 보정하는데 복사해서 수정(부족하거나 원하는 데이터를 여기서 추가 가능합니당)
        var modifiedData = enemyInfo.CloneCharacterData();
        ApplyRankMultiplier(modifiedData, rank);

        return modifiedData;
    }

    private void ApplyRankMultiplier(CharacterData data, EnemyRank rank)
    {
        float multiplier = 1.0f;
        switch (rank)
        {
            case EnemyRank.Normal: multiplier = 1.0f; break;
            case EnemyRank.Named: multiplier = 1.2f; break;
            case EnemyRank.Champion: multiplier = 1.5f; break;
            case EnemyRank.Boss: multiplier = 2.0f; break;
        }
        data.Hp = (int)(data.Hp * multiplier);
        data.Atk = data.Atk * multiplier;
        // data.Name = $"[{rank}] {data.Name}"; //심각한 버그 원인, 이름을 바꾸면 sprite를 못 가져옴
    }

    //일반전투를 생성하는 메소드입니다. 다른 전투와의 생성 로직은 동일하며 확률이 다릅니다.
    public List<CharacterData> GenerateNormalBattle(int chapter)
    {
        //반환할 적 리스트(Todo : character 관련 코드에서 적 구별 방법 확인 필요)
        List<CharacterData> enemies = new List<CharacterData>();

        SetNormalBattleCountProbability(chapter);
        int spawnCount = PickRandomEnemyCount();
        Debug.Log($"[Info] ChallengeBattleStageEnemyInitializer::GenerateNormalBattle - Spawning enemies: {spawnCount}");
        SetEnemyTypeProbability(1, 1, 1, 1, 1); // 적 종류는 일단 전부 동일

        for (int i = 0; i < spawnCount; i++)
        {
            EnemyType type = PickRandomEnemyType();
            EnemyRank rank = EnemyRank.Normal;
            // 일부 적의 등급을 조정 - 챔피언
            if (chapter >= 5 && Random.value < 0.3f)
            {
                rank = EnemyRank.Champion;
            }
            enemies.Add(GetEnemyDataFromManager(type, rank));
        }

        return enemies;
    }

    public List<CharacterData> GenerateEliteBattle(int chapter)
    {
        List<CharacterData> enemies = new List<CharacterData>();

        //여기선 elite도 확률에 맞게 뽑아줍니다
        SetEliteEnemyCountProbability(chapter);
        int eliteCount = PickRandomEnemyCount();
        SetEliteNormalEnemyCountProbability(chapter);
        int normalCount = PickRandomEnemyCount();
        SetEnemyTypeProbability(1, 1, 1, 1, 1);

        for (int i = 0; i < eliteCount; i++)
        {
            EnemyType type = PickRandomEnemyType();
            EnemyRank rank = EnemyRank.Named;
            // 일부 엘리트 적의 등급을 챔피언으로 상향 조정
            if (chapter >= 5 && Random.value < 0.3f)
            {
                rank = EnemyRank.Champion;
            }
            enemies.Add(GetEnemyDataFromManager(type, rank));
        }
        for (int i = 0; i < normalCount; i++)
        {
            EnemyType type = PickRandomEnemyType();
            EnemyRank rank = EnemyRank.Normal;
            // 일반 적도 낮은 확률로 챔피언이 될 수 있음
            if (chapter >= 7 && Random.value < 0.1f)
            {
                rank = EnemyRank.Champion;
            }
            enemies.Add(GetEnemyDataFromManager(type, rank));
        }

        return enemies;
    }

    private void SetNormalBattleCountProbability(int chapter)
    {
        _enemyCountProbabilityDict.Clear();
        switch (chapter)
        {
            case 1:
                SetEnemyCountProbability(count1: 50); SetEnemyCountProbability(count2: 50);
                break;
            case 2:
            case 3:
                SetEnemyCountProbability(count2: 50); SetEnemyCountProbability(count3: 50);
                break;
            case 4:
            case 5:
                SetEnemyCountProbability(count3: 33); SetEnemyCountProbability(count4: 34); SetEnemyCountProbability(count5: 33);
                break;
            case 6:
            case 7:
                SetEnemyCountProbability(count6: 33); SetEnemyCountProbability(count7: 33); SetEnemyCountProbability(count8: 34);
                break;
            default:
                Debug.LogWarning("[WARN] ChallengeBattleStageEnemyInitializer::SetNormalBattleCountProbability - cahpter가 범위를 넘어갔습니다");
                break;
        }
    }

    private void SetEliteEnemyCountProbability(int chapter)
    {
        _enemyCountProbabilityDict.Clear();
        switch (chapter)
        {
            case 1:
                SetEnemyCountProbability(count1: 100);
                break;
            case 2:
            case 3:
                SetEnemyCountProbability(count1: 70, count2: 30);
                break;
            case 4:
            case 5:
                SetEnemyCountProbability(count1: 30, count2: 20, count3: 50);
                break;
            case 6:
            case 7:
                SetEnemyCountProbability(count3: 100);
                break;
            default:
                Debug.LogWarning("[WARN] ChallengeBattleStageEnemyInitializer::SetEliteEnemyCountProbability - chapter가 범위를 넘어갔습니다");
                break;
        }
    }

    //엘리트 전투에서의 normal 적 수
    private void SetEliteNormalEnemyCountProbability(int chapter)
    {
        switch (chapter)
        {
            case 1:
                SetEnemyCountProbability(count1: 50, count2: 50);
                break;
            case 2:
            case 3:
                SetEnemyCountProbability(count2: 50, count3: 50);
                break;
            case 4:
            case 5:
                SetEnemyCountProbability(count3: 20, count4: 30, count5: 50);
                break;
            case 6:
            case 7:
                SetEnemyCountProbability(count6: 50, count7: 50);
                break;
            default:
                Debug.LogWarning("[WARN] ChallengeBattleStageEnemyInitializer::SetEliteNormalEnemyCountProbability - chapter가 범위를 넘어갔습니다");
                break;
        }
    }

    public int GetEnemyCountTotalWeight() => _enemyCountProbabilityDict.Values.Sum();
    public int GetEnemyTypeTotalWeight() => _enemyTypeProbabilityDict.Values.Sum();
    //적의 수와 종류를 확률에 따라 뽑음
    private int PickRandomEnemyCount()
    {
        return PickByWeight(_enemyCountProbabilityDict, GetEnemyCountTotalWeight());
    }
    private EnemyType PickRandomEnemyType()
    {
        return PickByWeight(_enemyTypeProbabilityDict, GetEnemyTypeTotalWeight());
    }

    private T PickByWeight<T>(Dictionary<T, int> dict, int totalWeight)
    {
        int random = Random.Range(0, totalWeight);
        foreach (var element in dict)
        {
            if (random < element.Value)
                return element.Key;
            random -= element.Value;
        }
        //딕셔너리가 비어있지 않다면 여기까지 안 오고 반드시 return해야 됨.
        Debug.LogError($"[ERROR]: BattleStageGenerationManager::PickByWeight() - Failed to pick an element. [Random={random}, TotalWeight={totalWeight}]");
        return dict.Keys.First();
    }

    //가중치 설정 메소드
    protected void SetEnemyTypeWeight(EnemyType type, int weight)
    {
        _enemyTypeProbabilityDict[type] = weight;
    }
    protected void SetEnemyCountWeight(int count, int weight)
    {
        _enemyCountProbabilityDict[count] = weight;
    }

    private void SetEnemyCountProbability(int count1 = 0, int count2 = 0, int count3 = 0, int count4 = 0, int count5 = 0, int count6 = 0, int count7 = 0, int count8 = 0)
    {
        //기존 가중치는 제거합니다(따라서 위의 확률 설정 메소드에서 분기에 따라 한번만 호출되어야 합니다!)
        _enemyCountProbabilityDict.Clear();
        //1에 가중치를 count1만큼 줍니다
        if (count1 > 0) SetEnemyCountWeight(1, count1);
        if (count2 > 0) SetEnemyCountWeight(2, count2);
        if (count3 > 0) SetEnemyCountWeight(3, count3);
        if (count4 > 0) SetEnemyCountWeight(4, count4);
        if (count5 > 0) SetEnemyCountWeight(5, count5);
        if (count6 > 0) SetEnemyCountWeight(6, count6);
        if (count7 > 0) SetEnemyCountWeight(7, count7);
        if (count8 > 0) SetEnemyCountWeight(8, count8);
    }

    private void SetEnemyTypeProbability(int swordsman = 0, int spearman = 0, int gunner = 0, int cavalry = 0, int shielder = 0)
    {
        //기존 가중치는 제거합니다
        _enemyTypeProbabilityDict.Clear();
        //Swordman에 가중치를 swordsman만큼 줍니다
        if (swordsman > 0) SetEnemyTypeWeight(EnemyType.Swordsman, swordsman);
        if (spearman > 0) SetEnemyTypeWeight(EnemyType.Spearman, spearman);
        if (gunner > 0) SetEnemyTypeWeight(EnemyType.Gunner, gunner);
        if (cavalry > 0) SetEnemyTypeWeight(EnemyType.Cavalry, cavalry);
        if (shielder > 0) SetEnemyTypeWeight(EnemyType.Shielder, shielder);
    }
}

