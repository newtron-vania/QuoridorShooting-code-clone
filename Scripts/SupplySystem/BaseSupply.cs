using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseSupply : MonoBehaviour
{
    public abstract int ID { get; }                         // 보급품 아이디
    public abstract BaseCharacter SupplyBaseCharacter { get; set; }

    public List<EffectData> EffectDataList;

    private void Start()
    {
    }

    public virtual void UseSupply()
    {
        TargetType target = DataManager.Instance.GetSupplyData(ID).EffectDataList[0].Target;
        if (target == TargetType.Allies || target == TargetType.TargetEnemy)
        {
            SupplyBaseCharacter = GameManager.Instance.BattleSystem.CurrentSelectBaseCharacter;
        }

        // 보급품을 전부 사용했을 때
        if (--(SupplyManager.Instance._supplyInventory[ID]) == 0)
        {
            SupplyManager.Instance._supplyInventory.Remove(ID);
        }
    }

    public virtual bool UpdateSupply() { return true; }

    // 보급품 선택 후 사용가능한 기물 확인
    public virtual void TargetHighLight()
    {
        TargetType target = DataManager.Instance.GetSupplyData(ID).EffectDataList[0].Target;
        // 선택해야하는 기물
        if (target == TargetType.Allies || target == TargetType.TargetEnemy)
        {
            SupplyManager.Instance._currentUseSupply = ID;
            SupplyManager.Instance._supplyTarget = target;
            GameManager.Instance.BattleSystem.TouchType = HM.Utils.TouchUtils.TouchType.UseSupply;

            // 지정이 필요한 보급품일 경우 하이라이팅  실시
            if (target == TargetType.Allies)
            {
                foreach (BaseCharacter child in GameManager.Instance.BattleSystem.StageCharacter[CharacterDefinition.CharacterIdentification.Player])
                {
                    UIManager.Instance.MakeHighlighting<TokenHighLighting>(child.CharacterObject.gameObject.transform);
                }
                Debug.Log("Set Player Target");
            }
            else
            {
                foreach (BaseCharacter child in GameManager.Instance.BattleSystem.StageCharacter[CharacterDefinition.CharacterIdentification.Enemy])
                {
                    UIManager.Instance.MakeHighlighting<TokenHighLighting>(child.CharacterObject.gameObject.transform);
                }
                Debug.Log("Set Enemy Target");
            }
        }
        // 선택하지 않고 즉시 사용되는 보급품들
        else
        {
            if (target == TargetType.Self && GameManager.Instance.BattleSystem.CurrentSelectBaseCharacter.characterStat != null)
            {
                SupplyBaseCharacter = GameManager.Instance.BattleSystem.CurrentSelectBaseCharacter;
            }
            UseSupply();
        }
    }

    public virtual void InitSupply()
    {
        EffectDataList = DataManager.Instance.GetSupplyData(ID).EffectDataList;
        // 아니면 사용가능한 하이라이팅 기물 보여주기
        TargetHighLight();
    }
}