using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class EffectLogicProvider : MonoBehaviour
{
    //EffectData,EffectLogic들 초기화 하고 관리하는 시스템?

    //EffectLogic 캐싱
    public ReadOnlyDictionary<EffectType, IBaseEffectLogic> EffectLogicDict { get; private set; }

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        InitEffectLogicDict();
    }

    /// <summary>
    /// EffectLogic 조회 메서드
    /// </summary>
    public IBaseEffectLogic GetEffectLogic(EffectType type)
    {
        if (EffectLogicDict != null && EffectLogicDict.TryGetValue(type, out var logic))
        {
            return logic;
        }
        Debug.LogError($"[ERROR]EffectLogicProvider::GetEffectLogic() - EffectType {type} not found in EffectLogicDict");
        return null;
    }

    //모든 효과 로직 초기화
    public void InitEffectLogicDict()
    {
        Dictionary<EffectType, IBaseEffectLogic> effectLogicDict = new();
        foreach (var effectTypeObj in Enum.GetValues(typeof(EffectType)))
        {
            EffectType effectType = (EffectType)effectTypeObj;
            IBaseEffectLogic originalEffect = null;
            switch (effectType)
            {
                // None은 실제 효과가 없으므로 스킵
                case EffectType.None:
                    continue;

                //Common
                //SkillSimpleAttack가 기본값
                case EffectType.CommonSimpleDamage:
                case EffectType.CommonSimpleDamage_FirstHit:
                case EffectType.CommonSimpleDamage_OnTurnStart:

                    if (effectLogicDict.TryGetValue(EffectType.CommonSimpleDamage, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonSimpleDamageEffect());
                    }
                    break;

                case EffectType.CommonStatBaseDamage_AttackDamage:  // 원본: CommonStatBaseDamage_Atk

                    if (effectLogicDict.TryGetValue(EffectType.CommonStatBaseDamage_AttackDamage, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonStatBaseDamageEffect());
                    }
                    break;

                case EffectType.CommonStatuseffectCreation:
                case EffectType.CommonStatuseffectCreation_OnEnd:

                    if (effectLogicDict.TryGetValue(EffectType.CommonStatuseffectCreation, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonStatuseffectCreationEffect(StatManager.Instance));
                    }
                    break;

                case EffectType.CommonStatuseffectCreationArea:
                case EffectType.CommonStatuseffectCreationArea_AreaOnly:

                    if (effectLogicDict.TryGetValue(EffectType.CommonStatuseffectCreationArea, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonStatuseffectCreationAreaEffect(StatManager.Instance));
                    }
                    break;

                case EffectType.CommonActionCountAdd_Attack:
                case EffectType.CommonActionCountAdd_Move:

                    if (effectLogicDict.TryGetValue(EffectType.CommonActionCountAdd_Attack, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonActionCountAddEffect());
                    }
                    break;

                case EffectType.CommonStatAdd_ActionPointPerTurn:  // 원본: CommonStatAdd_ApRecovery
                case EffectType.CommonStatAdd_DamageResistance:    // 원본: CommonStatAdd_Avd

                    if (effectLogicDict.TryGetValue(EffectType.CommonStatAdd_DamageResistance, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonSimpleStatAddEffect());
                    }
                    break;

                case EffectType.CommonStatuseffectClear_ByTag:
                    if (effectLogicDict.TryGetValue(EffectType.CommonStatuseffectClear_ByTag, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonStatuseffectClearEffect());
                    }
                    break;

                case EffectType.Cure:  // 원본: CommonSimpleHeal
                    if (effectLogicDict.TryGetValue(EffectType.Cure, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonSimpleHealEffect());
                    }
                    break;


                //Skill
                case EffectType.SkillTeleport:

                    if (effectLogicDict.TryGetValue(EffectType.SkillTeleport, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new SkillTeleportEffect());
                    }

                    break;

                case EffectType.SkillCellStateChange:

                    if (effectLogicDict.TryGetValue(EffectType.SkillCellStateChange, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CellStateChangeEffect());
                    }

                    break;

                //Statuseffect
                case EffectType.StatuseffectDamageStatuseffectCreation_OnAttack:

                    if (effectLogicDict.TryGetValue(EffectType.StatuseffectDamageStatuseffectCreation_OnAttack, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new StatuseffectDamageStatuseffectCreationEffect(StatManager.Instance));
                    }
                    break;

                case EffectType.StatuseffectStatAdd_DamageResistance:  // 원본: StatuseffectStatAdd_Avd
                case EffectType.StatuseffectStatAdd_AttackDamage:      // 원본: StatuseffectStatAdd_Atk

                    if (effectLogicDict.TryGetValue(EffectType.StatuseffectStatAdd_AttackDamage, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new StatuseffectStatAddEffect());
                    }
                    break;

                case EffectType.StatuseffectDamage_OnTurnStart:

                    if (effectLogicDict.TryGetValue(EffectType.StatuseffectDamage_OnTurnStart, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new StatuseffectDamageEffect());
                    }
                    break;

                // Legacy types - 구버전 JSON 호환용, 실제 효과 없음
                case EffectType.Damage:
                case EffectType.Attack:
                case EffectType.ActionPoint:
                case EffectType.Resistance:
                case EffectType.Resistane:  // 오타까지 지원
                case EffectType.Hp:
                case EffectType.SupplyStatusEffectCreation:
                    continue;

                default:
                    Debug.LogError($"[ERROR]EffectLogicProvider::InitEffectLogicDict(): EffectLogicDict 초기화 실패, EffectType {effectType}에 해당하는 EffectLogic이 없습니다.");
                    break;
            }
        }

        EffectLogicDict = new(effectLogicDict); 
    }



}
