using System.Collections.Generic;

public class CharacterLoader
{
    public List<CharacterData> LoadPlayerCharacters(ICharacterDataProvider provider)
    {
        return provider.Load();
    }

    public List<CharacterData> LoadEnemyCharacters(ICharacterDataProvider provider)
    {
        return provider.Load();
    }
}

public interface ICharacterDataProvider
{
    List<CharacterData> Load();
}

public class PlayerDataProvider : ICharacterDataProvider
{
    public List<CharacterData> Load()
    {
        var playerList = GameManager.Instance.PlayerList;
        var characterDataList = new List<CharacterData>();

        foreach (var savedData in playerList)
        {
            characterDataList.Add(savedData.ConvertToCharacterData());
        }

        return characterDataList;
    }
}

public class EnemyDataProvider : ICharacterDataProvider
{
    public List<CharacterData> Load()
    {
        var spawner = new EnemyCharacterSpawnerOfTile(GameManager.Instance.RandSeed);
        return spawner.SpawnEnemyCharacters();
    }
}