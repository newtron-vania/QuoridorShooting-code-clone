using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyRootCane : BaseSupply
{
    public override int ID => 3;
    public override BaseCharacter SupplyBaseCharacter { get => _useTargetBaseCharacter; set => _useTargetBaseCharacter = value; }

    private BaseCharacter _useTargetBaseCharacter;
    private int _saveTurn = 0;

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
        //if (SupplyCharacterStat == null) return false;
        //if (_saveTurn + DurationRound == GameManager.Instance.Turn)
        //{
        //    SupplyCharacterStat.Hp -= EffectAmount;
        //    return false;
        //}
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}