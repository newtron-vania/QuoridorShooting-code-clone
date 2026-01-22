using System;
using System.Collections.Generic;
using UnityEngine;
public class CharacterSpawnManager
{
    private readonly CharacterLoader _loader;
    private readonly CharacterController _controller;

    public CharacterSpawnManager(CharacterController controller)
    {
        _controller = controller;
        _loader = new CharacterLoader();
    }

    /// <summary>
    /// 플레이어 캐릭터 생성 및 배치
    /// </summary>
    public void SpawnPlayerCharacters(ICharacterSpawner spawner, ISpawnPositionStrategy positionStrategy, ICharacterPlacer placer)
    {
        List<CharacterData> dataList = _loader.LoadPlayerCharacters(new PlayerDataProvider());
        var usedPositions = _controller.GetAllCharactersPosition();

        foreach (var data in dataList)
        {
            var character = spawner.SpawnCharacter(data, _controller);
            var position = positionStrategy.GetNextValidPosition(character, usedPositions);
            usedPositions.Add(position);
            placer.PlaceCharacter(character, position);
        }

        TriggerManager.Instance.ActivateTrigger(GameTrigger.PlayerFieldSetting);
    }

    /// <summary>
    /// 적 캐릭터 생성 및 배치
    /// </summary>
    public void SpawnEnemyCharacters(ICharacterSpawner spawner, ISpawnPositionStrategy positionStrategy, ICharacterPlacer placer)
    {
        List<CharacterData> dataList = _loader.LoadEnemyCharacters(new EnemyDataProvider());
        List<Vector2Int> usedPositions = _controller.GetAllCharactersPosition();

        foreach (var data in dataList)
        {
            var character = spawner.SpawnCharacter(data, _controller);
            var position = positionStrategy.GetNextValidPosition(character, usedPositions);
            usedPositions.Add(position);
            placer.PlaceCharacter(character, position);
        }
        
        TriggerManager.Instance.ActivateTrigger(GameTrigger.EnemyFieldSetting);
    }

    /// <summary>
    /// 단일 캐릭터 생성 및 배치
    /// </summary>
    public void SpawnCharacter(CharacterData data, ICharacterSpawner spawner, ISpawnPositionStrategy positionStrategy, ICharacterPlacer placer, List<Vector2Int> usedPositions, int id = 0)
    {
        var character = spawner.SpawnCharacter(data, _controller);
        character.Id = id;
        var position = positionStrategy.GetNextValidPosition(character, usedPositions);
        usedPositions.Add(position);
        placer.PlaceCharacter(character, position);
    }
}
