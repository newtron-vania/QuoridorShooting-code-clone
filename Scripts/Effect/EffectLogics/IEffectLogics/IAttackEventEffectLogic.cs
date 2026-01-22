using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackEvent
{
    OnAttack,
}

public interface IAttackEventEffectLogic
{
    //attacker의 BaseCharacter는 나중에 바꿔도됨
    public void EffectByAttackEvent(AttackEvent attackEvent, EffectInstance effectInstance, EffectData effectData, BaseCharacter attacker, IEffectableProvider target);
}