using Unity.VisualScripting;
using UnityEngine;

public class SavedPlayerCharacterData
{
    public int Index;
    public int MaxHp;
    public int CurrentHp;
    public int MaxAp;
    public int CurrentAp;


    public SavedPlayerCharacterData(CharacterData original)
    {
        SetSavedPlayerCharacterData(original);
    }

    public void SetSavedPlayerCharacterData(CharacterData original)
    {
        Index = original.Index;
        MaxHp = original.Hp;
        CurrentHp = original.Hp;
        MaxAp = 100;
        CurrentAp = 0;
    }

    public CharacterData ConvertToCharacterData()
    {
        CharacterData original = DataManager.Instance.GetPlayableCharacterInfo(Index);
        original.Hp = CurrentHp;
        // CharacterData data = new CharacterData(Index, true, original.Name, (int)original.Class, original.Type, CurrentHp, original.Atk, original.Avd, original.ApRecovery, original.SkillId, original.MoveRangeId, original.AttackRangeId);

        return original;
    }

}