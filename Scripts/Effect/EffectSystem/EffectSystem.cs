using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class EffectSystem : MonoBehaviour
{
    //Statuseffect도 관리해주세요?
    public ReadOnlyDictionary<int, StatuseffectData> StatuseffectDataDict { get; private set; }

    //EffectData,EffectLogic들 초기화 하고 관리하는 시스템?

    //EffectLogic 캐싱
    public ReadOnlyDictionary<EffectType, IBaseEffectLogic> EffectLogicDict { get; private set; }

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        //캐싱들 필요없어보이긴 함; 테스트 환경을 위한..
        InitEffectLogicDict();
        InitStatuseffectData();
    }




    public void InitStatuseffectData()
    {
        Dictionary<int, StatuseffectData> statusEffectDataDict = new Dictionary<int, StatuseffectData>();

        //StatuseffectData 캐싱
        for (int i = 1; i <= DataManager.Instance.StatuseffectDataCount; i++)
        {
            var data = DataManager.Instance.GetStatuseffectData(i);
            statusEffectDataDict.Add(i, data);
        }

        StatuseffectDataDict = new ReadOnlyDictionary<int, StatuseffectData>(statusEffectDataDict);
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
                //Common
                //SkillSimpleAttack가 기본값
                case EffectType.CommonSimpleDamage:
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

                case EffectType.CommonStatBaseDamage_Atk:

                    if (effectLogicDict.TryGetValue(EffectType.CommonStatBaseDamage_Atk, out originalEffect))
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
                        effectLogicDict.Add(effectType, new CommonStatuseffectCreationEffect(this));
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
                        effectLogicDict.Add(effectType, new CommonStatuseffectCreationAreaEffect(this));
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

                case EffectType.CommonStatAdd_ApRecovery:
                case EffectType.CommonStatAdd_Avd:

                    if (effectLogicDict.TryGetValue(EffectType.CommonStatAdd_Avd, out originalEffect))
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

                case EffectType.CommonSimpleHeal:
                    if (effectLogicDict.TryGetValue(EffectType.CommonSimpleHeal, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new CommonSimpleHealEffect());
                    }
                    break;


                //Skill
                case EffectType.SkillStateChange:

                    if (effectLogicDict.TryGetValue(EffectType.SkillStateChange, out originalEffect))
                    {
                        effectLogicDict.Add(effectType, originalEffect);
                    }
                    else
                    {
                        effectLogicDict.Add(effectType, new SkillStateChangeEffect());
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
                        effectLogicDict.Add(effectType, new StatuseffectDamageStatuseffectCreationEffect(this));
                    }
                    break;

                case EffectType.StatuseffectStatAdd_Avd:
                case EffectType.StatuseffectStatAdd_Atk:

                    if (effectLogicDict.TryGetValue(EffectType.StatuseffectStatAdd_Atk, out originalEffect))
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

                default:
                    Debug.LogError($"[ERROR]EffectSystem::InitEffectLogicDict(): EffectLogicDict 초기화 실패, EffectType {effectType}에 해당하는 EffectLogic이 없습니다.");
                    break;
            }
        }

        EffectLogicDict = new(effectLogicDict); 
    }



}
