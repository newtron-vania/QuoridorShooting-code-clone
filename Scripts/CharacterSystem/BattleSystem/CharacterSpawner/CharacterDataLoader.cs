using System.Collections.Generic;

public class CharacterLoader
{
    public List<CharacterData> LoadPlayerCharacters(ICharacterDataProvider provider)
    {
        var list = provider.Load();
        UnityEngine.Debug.Log($"[Info] CharacterLoader::LoadPlayerCharacters: Loaded {list?.Count ?? 0} items.");
        if (list != null)
        {
            foreach (var data in list)
            {
                UnityEngine.Debug.Log($" CharacterLoader:: -> Player Character: {data.Name} (ID: {data.Index})");
            }
        }
        return list;
    }

    public List<CharacterData> LoadEnemyCharacters(ICharacterDataProvider provider)
    {
        //var list = provider.Load();
        UnityEngine.Debug.Log("[Info] CharacterLoader::LoadEnemyCharacters: Attempting to load enemy characters.");
        List<CharacterData> list = null;
        try
        {
            list = provider.Load();
            UnityEngine.Debug.Log($"[Info] CharacterLoader::LoadEnemyCharacters: Loaded {list?.Count ?? 0} items.");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[ERROR] CharacterLoader::LoadEnemyCharacters - Failed to load: {e.Message}\n{e.StackTrace}");
            return new List<CharacterData>();
        } 
        
        if (list != null)
        {
            foreach (var data in list)
            {
                UnityEngine.Debug.Log($" CharacterLoader:: -> Enemy Character: {data.Name} (ID: {data.Index})");
            }
        }
        else
        {
            UnityEngine.Debug.LogWarning("[Warning] CharacterLoader::LoadEnemyCharacters: No enemy characters loaded.");
        }
        return list;
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
        // 기존 코드에서는 TileCharacterSpawner 사용.
        //var spawner = new EnemyCharacterSpawnerOfTile(GameManager.Instance.RandSeed);
        //return spawner.SpawnEnemyCharacters();

        var retGenerator = new ChallengeBattleStageEnemyInitializer();
        int currentChapter = StageManager.Instance.CurrentChapterLevel;

        // 현재 스테이지 정보 가져와서 type에 맞게 적 생성
        int currentStageId = StageManager.Instance.CurStageId;
        Stage currentStage = null;
        if (StageManager.Instance.StageDic.ContainsKey(currentStageId))
        {
            currentStage = StageManager.Instance.StageDic[currentStageId];
        }
        if (currentStage == null)
        {
            UnityEngine.Debug.LogWarning($"[WARNING] EnemyDataProvider: Could not find stage info for ID {currentStageId}.");
            return retGenerator.GenerateNormalBattle(currentChapter);
        }

        UnityEngine.Debug.Log($"[INFO] EnemyDataProvider: Generating enemies, Stage: {currentStageId}, Type: {currentStage.Type}, Chapter: {currentChapter}");

        switch (currentStage.Type)
        {
            case Stage.StageType.Elite: return retGenerator.GenerateEliteBattle(currentChapter);
            case Stage.StageType.Boss: return retGenerator.GenerateEliteBattle(currentChapter); 
            case Stage.StageType.Normal:
            default:
                return retGenerator.GenerateNormalBattle(currentChapter);
        }
        var spawner = new EnemyCharacterSpawnerOfCell(GameManager.Instance.RandSeed);
        return spawner.SpawnEnemyCharacters();
    }
}