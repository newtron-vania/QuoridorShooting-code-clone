using System.Collections.Generic;

namespace CharacterDefinition
{
    public enum CharacterIdentification
    {
        None = -1,
        Player,
        Enemy
    };
    public enum CharacterClass
    {
        Tanker,      // 탱커
        Dealer,      // 딜러 (공격수)
        Supporter    // 서포터
    }
    public enum CharacterType
    {
        Successor,      // 계승자
        Freeman,        // 자유인
        Engineer       // 기술자
    }
    public enum PlayerControlStatus
    {
        None = -1,
        Move,
        Build,
        Attack,
        Skill,
        Destroy
    };
}