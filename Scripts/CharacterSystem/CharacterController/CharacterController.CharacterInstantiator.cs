using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using UnityEngine;

public partial class CharacterController
{
    public List<CharacterData> _readyToSpawnCharacterData = new(); // 전투 시작 전 스폰 예정 캐릭터 데이터 목록
    private CharacterSpawnManager _spawnManager;
    public List<GameObject> CharacterObjects = new(); // 씬에 존재하는 캐릭터 오브젝트 리스트

    
    private void SpawnCharacters()
    {
        _spawnManager = new CharacterSpawnManager(this);

        SetupSpawnCharacter();
        SpawnAllPlayers();
        SpawnAllEnemies();
    }
    
    private void SpawnAllPlayers()
    {
        var spawner = new PlayerCharacterSpawner();
        var placer = new GenericCharacterPlacer(this, 
            PlayerCharacterPrefab, 
            CharacterIdentification.Player,
            new List<Vector2Int>()
            );

        var strategy = new RandomInRangeStrategy(-4, 4, -4, -3);
        
        _spawnManager.SpawnPlayerCharacters(spawner, strategy, placer);

        GameManager.Instance.playerCount = StageCharacter[CharacterIdentification.Player].Count;
    }

    private void SpawnAllEnemies()
    {
        var spawner = new EnemyCharacterSpawner();
        var placer = new GenericCharacterPlacer(this, 
            EnemyCharacterPrefab, 
            CharacterIdentification.Enemy,
            new List<Vector2Int>());
        
        var strategy = new RandomInRangeStrategy(-4, 4, 2, 4);
        
        _spawnManager.SpawnEnemyCharacters(spawner, strategy, placer);

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