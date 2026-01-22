using CharacterDefinition;
using UnityEngine;


//Refactor 요소
//Participant가 캐릭터가 아닐 때도 상정해야하긴 함 
//이것도 그래서 대미지를 입히는 기능, 스텟이있는지 이런거 쪼개야 될 것 같음..
public interface ISkillParticipant :IEffectable
{
    public int BattleId { get; }

    public int SkillId { get; }

    public Vector2Int Position { get; }

    public CharacterStat CharacterStat { get; }

    public int TakeSkillDamage(BaseCharacter source , int damage);

    public CharacterIdentification Identification { get; }

    public void SlipForward(Vector2Int targetPrvePos, int value);
    

}
