using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyBluebutterflyBrooch : BaseSupply
{
    public override int ID => 19;
    public override BaseCharacter SupplyBaseCharacter { get => _useTargetBaseCharacter; set => _useTargetBaseCharacter = value; }

    private BaseCharacter _useTargetBaseCharacter;
    private int _saveTurn;

    public override void UseSupply()
    {
        base.UseSupply();
        // 해당 방식 통합해서 판단하는 함수 제작하는게 나아보임
        if (EffectDataList[0].Get<int>("Duration") != 0)
        {
            SupplyManager.Instance._saveDurationSupply.Add(this);
        }
        _saveTurn = GameManager.Instance.Turn;
        SupplyBaseCharacter.characterStat.Atk += EffectDataList[0].Get<int>("DamageValue");
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
        if (_saveTurn + (EffectDataList[0].Get<int>("Duration") * 2) == GameManager.Instance.Turn)
        {
            SupplyBaseCharacter.characterStat.Atk -= EffectDataList[0].Get<int>("DamageValue");
            return false;
        }
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}