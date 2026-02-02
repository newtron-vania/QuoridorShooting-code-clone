using System.Collections.Generic;
using CharacterDefinition;
using UnityEngine;

public class CharacterFactory
{
    private BattleSystem _controller;

    public CharacterFactory(BattleSystem controller)
    {
        _controller = controller;
    }

    public BaseCharacter CreateCharacter(CharacterData data, bool isPlayer)
    {
        BaseCharacter character = isPlayer ? new PlayerCharacter(_controller) : new EnemyCharacter(_controller);
        character.SetCharacterData(data);
        return character;
    }

    public void SpawnCharacter(BaseCharacter character, GameObject characterPrefab, Vector2Int position, List<GameObject> characterObjects, Dictionary<CharacterIdentification, List<BaseCharacter>> stageCharacter)
    {
        character.SetCharacterGameObject(characterPrefab.GetComponent<QuoridorCharacterObject>());
        character.Position = position;
        // 생성된 오브젝트 이미지 입히기, 이름_id 형태로 이름 수정
        characterPrefab.GetComponent<SpriteRenderer>().sprite = character.CharacterSprite;
        characterPrefab.name = character.CharacterName + "_" + character.Id;
        characterObjects.Add(characterPrefab);
    }
}