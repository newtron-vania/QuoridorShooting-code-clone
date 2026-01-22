using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyStrengthPotion : BaseSupply
{
    public override int ID => 10;
    public override string Name => "강인의 엘릭서";
    public override string Description => "피해 저항의 최대치를 10% 높여준다.";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Defense;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetChar;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Normal;
    public override int DurationRound => 0;
    public override int EffectAmount => 10;
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
        SupplyCharacterStat.Avd += 0.1f;
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