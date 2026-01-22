using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyStinger : BaseSupply
{
    public override int ID => 0;
    public override string Name => "독침";
    public override string Description => "적의 체력을 3만큼 감소시킨다.";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Attack;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetEnemy;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Normal;
    public override int DurationRound => 0;
    public override int EffectAmount => 3;
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
        SupplyCharacterStat.Hp -= 3;
        SupplyUseShowPanelUI supplyUseShowPanelUI = UIManager.Instance.ShowPopupUI<SupplyUseShowPanelUI>();
        supplyUseShowPanelUI.SupplyName = Name;
        supplyUseShowPanelUI.TargetName = SupplyCharacterStat.Name;
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