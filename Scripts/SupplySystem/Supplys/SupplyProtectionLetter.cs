using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyProtectionLetter : BaseSupply
{
    public override int ID => 11;
    public override BaseCharacter SupplyBaseCharacter { get => _useTargetBaseCharacter; set => _useTargetBaseCharacter = value; }

    private BaseCharacter _useTargetBaseCharacter;
    private int _saveTurn;

    public override void UseSupply()
    {
        base.UseSupply();
        if (EffectDataList[0].Get<int>("Duration") != 0)
        {
            SupplyManager.Instance._saveDurationSupply.Add(this);
        }
        _saveTurn = GameManager.Instance.Turn;
        SupplyUseShowPanelUI supplyUseShowPanelUI = UIManager.Instance.ShowPopupUI<SupplyUseShowPanelUI>();
        supplyUseShowPanelUI.SupplyID = ID;
        supplyUseShowPanelUI.TargetName = SupplyBaseCharacter.characterStat.Name;
        SupplyBaseCharacter.characterStat.Avd += (float)EffectDataList[1].Get<int>("DamageValue")/100;
    }

    public override void TargetHighLight()
    {
        base.TargetHighLight();
    }

    public override bool UpdateSupply()
    {
        base.UpdateSupply();
        if (SupplyBaseCharacter == null) return false;
        if (_saveTurn + (EffectDataList[0].Get<int>("Duration") * 2) == GameManager.Instance.Turn)
        {
            SupplyBaseCharacter.characterStat.Avd -= (float)EffectDataList[1].Get<int>("DamageValue")/100;
            return false;
        }
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}