using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class BaseSupply : MonoBehaviour
{
    public abstract string Name { get; }
    public abstract int ID { get; }                         // 보급품 아이디
    public abstract string Description { get; }             // 보급품 설명
    public abstract SupplymentData.SupplyType Type { get; }     // 보급품 종류
    public abstract SupplymentData.SupplyTarget Target { get; } // 보급품 적용 대상
    public abstract SupplymentData.SupplyGrade Rank { get; }     // 보급품 랭크
    public abstract int DurationRound { get; }              // 보급품 지속시간
    public abstract int EffectAmount { get; }               // 해당 보급품 증가 수치
    public abstract Sprite Image { get; }                   // 보급품 이미지
    public abstract CharacterStat SupplyCharacterStat { get; set; }

    private void Start()
    {
    }

    public virtual void UseSupply()
    {
        if (Target == SupplymentData.SupplyTarget.TargetChar || Target == SupplymentData.SupplyTarget.TargetEnemy)
        {
            SupplyCharacterStat = GameManager.Instance.CharacterController.CurrentSelectBaseCharacter.characterStat;
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
        // 선택해야하는 기물
        if (Target == SupplymentData.SupplyTarget.TargetChar || Target == SupplymentData.SupplyTarget.TargetEnemy)
        {
            SupplyManager.Instance._currentUseSupply = ID;
            SupplyManager.Instance._supplyTarget = Target;
            GameManager.Instance.CharacterController.TouchType = HM.Utils.TouchUtils.TouchType.UseSupply;

            // 지정이 필요한 보급품일 경우 하이라이팅  실시
            if (Target == SupplymentData.SupplyTarget.TargetChar)
            {
                foreach (BaseCharacter child in GameManager.Instance.CharacterController.StageCharacter[CharacterDefinition.CharacterIdentification.Player])
                {
                    UIManager.Instance.MakeHighlighting<TokenHighLighting>(child.CharacterObject.gameObject.transform);
                }
                Debug.Log("Set Player Target");
            }
            else
            {
                foreach (BaseCharacter child in GameManager.Instance.CharacterController.StageCharacter[CharacterDefinition.CharacterIdentification.Enemy])
                {
                    UIManager.Instance.MakeHighlighting<TokenHighLighting>(child.CharacterObject.gameObject.transform);
                }
                Debug.Log("Set Enemy Target");
            }
        }
        // 선택하지 않고 즉시 사용되는 보급품들
        else
        {
            UseSupply();
        }
    }

    public virtual void InitSupply()
    {
        // 아니면 사용가능한 하이라이팅 기물 보여주기
        TargetHighLight();
    }
}
