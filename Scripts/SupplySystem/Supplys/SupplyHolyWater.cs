using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyHolyWater : BaseSupply
{
    public override int ID => 14;
    public override string Name => "성수";
    public override string Description => "캐릭터 1명의 행동 불능 상태를 해제한다. ";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Cure;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetChar;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Rare;
    public override int DurationRound => 0;
    public override int EffectAmount => 0;
    public override Sprite Image => null;
    public override CharacterStat SupplyCharacterStat { get => _useTargetBaseCharacter; set => _useTargetBaseCharacter = value; }

    private CharacterStat _useTargetBaseCharacter;

    public override void UseSupply()
    {
        base.UseSupply();
        if (DurationRound != 0)
        {
            SupplyManager.Instance._saveDurationSupply.Add(this);
        }
        Debug.Log($"{Name} 보급품 사용완");
    }

    public override void TargetHighLight()
    {
        base.TargetHighLight();
    }

    public override bool UpdateSupply()
    {
        base.UpdateSupply();
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}