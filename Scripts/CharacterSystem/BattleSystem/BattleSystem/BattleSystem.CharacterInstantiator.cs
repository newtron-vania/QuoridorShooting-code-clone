using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public partial class BattleSystem
{
    public List<CharacterData> _readyToSpawnCharacterData = new(); // 전투 시작 전 스폰 예정 캐릭터 데이터 목록
    public CharacterSpawnManager  SpawnManager { get; private set; }
    public List<GameObject> CharacterObjects = new(); // 씬에 존재하는 캐릭터 오브젝트 리스트

    
    private void SpawnCharacters()
    {
        SpawnManager = new CharacterSpawnManager(this);
        SetupSpawnCharacter();
        SpawnAllPlayers();
        SpawnAllEnemies();
        
        
        // GameObject.FindObjectOfType<SkillIntegrationTest>().Init(); 테스트 스크립트
    }
    
    private void SpawnAllPlayers()
    {
        var spawner = new PlayerCharacterSpawner();
        var placer = new GenericCharacterPlacer(this, 
            PlayerCharacterPrefab, 
            CharacterIdentification.Player,
            new List<Vector2Int>()
            );

        // var strategy = new RandomInRangeStrategy(-4, 4, -4, -3);
        //플레이어 배치도 단순 랜덤이 아니라 따로 관리합니다.
        var strategy = new ChallengeBattleStagePlayerPlacement(this);
        
        SpawnManager.SpawnPlayerCharacters(spawner, strategy, placer);

        GameManager.Instance.playerCount = StageCharacter[CharacterIdentification.Player].Count;
    }

    private void SpawnAllEnemies()
    {
        var spawner = new EnemyCharacterSpawner();
        var placer = new GenericCharacterPlacer(this, 
            EnemyCharacterPrefab, 
            CharacterIdentification.Enemy,
            new List<Vector2Int>());

        // var strategy = new RandomInRangeStrategy(-4, 4, 2, 4);
        // var placementType = ChallengeBattleStageEnemyPlacement.SetupPlacementTypeStrategy();
        //배치 범위가 placementType에 따라 결정합니다.
        var strategy = new ChallengeBattleStageEnemyPlacement(this);
        
        SpawnManager.SpawnEnemyCharacters(spawner, strategy, placer);

        GameManager.Instance.enemyCount = StageCharacter[CharacterIdentification.Enemy].Count;
    }

    private void SetupSpawnCharacter()
    {
        // StageCharacter 초기화
        StageCharacter.Clear();
        StageCharacter = new Dictionary<CharacterIdentification, List<BaseCharacter>>
        {
            { CharacterIdentification.Player, new List<BaseCharacter>() },
            { CharacterIdentification.Enemy, new List<BaseCharacter>() }
        };

        StageDeadCharacter = new Dictionary<CharacterIdentification, List<BaseCharacter>>
        {
            { CharacterIdentification.Player, new List<BaseCharacter>() },
            { CharacterIdentification.Enemy, new List<BaseCharacter>() }
        };

        //테스트용 생성
        for (var i = 0; i < 3; i++)
        {
            var selectPlayerNum = Random.Range(0, DataManager.Instance.PlayableCharacterCount);
            var data = DataManager.Instance.GetPlayableCharacterInfo(selectPlayerNum);
            GameManager.Instance.PlayerList.Add(new SavedPlayerCharacterData(data));
        }
    }
}