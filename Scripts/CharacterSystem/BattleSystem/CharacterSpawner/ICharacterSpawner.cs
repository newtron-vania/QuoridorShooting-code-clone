
using System.Collections.Generic;
using System.Linq;
using HM;
using UnityEngine;
using EventType = HM.EventType;

public interface ICharacterSpawner
{
    BaseCharacter SpawnCharacter(CharacterData characterData, BattleSystem controller);
}

public class PlayerCharacterSpawner : ICharacterSpawner
{
    private int count = 0;
    public BaseCharacter SpawnCharacter(CharacterData characterData, BattleSystem controller)
    {
        var character = new PlayerCharacter(controller);
        character.SetCharacterData(characterData);
        character.Id = count++;
        character.AttachSkillSystem(controller.SkillSystem);

        return character;
    }
    
}

public class EnemyCharacterSpawner : ICharacterSpawner
{
    private int count = 0;
    public BaseCharacter SpawnCharacter(CharacterData characterData, BattleSystem controller)
    {
        var character = new EnemyCharacter(controller);
        character.SetCharacterData(characterData);        
        character.Id = count++;
        character.AttachSkillSystem(controller.SkillSystem);
        
        return character;
    }
}