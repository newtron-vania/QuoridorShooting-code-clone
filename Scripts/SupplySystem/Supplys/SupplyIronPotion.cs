using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyIronPotion : BaseSupply
{
    public override int ID => 8;
    public override string Name => "아이언 포션";
    public override string Description => "1라운드 동안 받는 피해량이 1만큼 줄어든다";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Defense;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetChar;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Normal;
    public override int DurationRound => 1;
    public override int EffectAmount => 1;
    public override Sprite Image => null;
    public override CharacterStat SupplyCharacterStat { get => _useTargetBaseCharacter; set => _useTargetBaseCharacter = value; }

    private CharacterStat _useTargetBaseCharacter;
    private int _saveTurn;

    public override void UseSupply()
    {
        base.UseSupply();
        if (DurationRound != 0)
        {
            SupplyManager.Instance._saveDurationSupply.Add(this);
        }
        // TEMP : QA이후 업데이트 될 예정
        _saveTurn = GameManager.Instance.Turn;
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
        if (SupplyCharacterStat == null) return false;
        if (_saveTurn + (DurationRound * 2) == GameManager.Instance.Turn)
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