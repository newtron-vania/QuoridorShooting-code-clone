using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterDefinition.Legacy
{
    // ??, ??? ???? ??????
    public enum CharacterIdentification
    {
        None = -1,
        Player,
        Enemy
    };

    // ?????? ??? ???? (??? ????? ????)
    public enum CharacterType
    {
        Mutu = 0,
        Mana,
        Machine
    };

    // ?????? ?????? ???? (??? ????? ????)
    public enum CharacterClass
    {
        Tanker = 0,
        Attacker,
        Supporter
    };

    public enum PlayerRangeField
    {
        None = -1,
        Move,
        Attack,
        Skill
    };

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
