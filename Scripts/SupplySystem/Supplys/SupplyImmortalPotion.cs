using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyImmortalPotion : BaseSupply
{
    public override int ID => 9;
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
    }

    public override void TargetHighLight()
    {
        base.TargetHighLight();
    }

    public override bool UpdateSupply()
    {
        base.UpdateSupply();
        if (SupplyBaseCharacter == null) return false;
        if (GameManager.Instance.Turn / 2 == 1)
        {
            SupplyBaseCharacter.characterStat.Hp += 5;
        }
        if (_saveTurn + (EffectDataList[0].Get<int>("Duration") * 2) == GameManager.Instance.Turn)
        {
            return false;
        }
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}