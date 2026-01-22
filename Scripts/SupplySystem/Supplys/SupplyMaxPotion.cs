using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyMaxPotion : BaseSupply
{
    public override int ID => 17;
    public override string Name => "맥스 포션";
    public override string Description => "체력을 최대치까지 회복한다.";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Cure;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetChar;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Unique;
    public override int DurationRound => 0;
    public override int EffectAmount => 100;
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
        SupplyCharacterStat.Hp = SupplyCharacterStat.MaxHp;
        // TEMP : QA이후 업데이트 될 예정
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