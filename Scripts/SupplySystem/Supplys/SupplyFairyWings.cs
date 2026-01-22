using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyFairyWings : BaseSupply
{
    public override int ID => 19;
    public override string Name => "요정의 날개";
    public override string Description => "3라운드 동안 공격력이 2 증가한다.";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Enhance;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetChar;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Unique;
    public override int DurationRound => 3;
    public override int EffectAmount => 2;
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
        SupplyCharacterStat = GameManager.Instance.CharacterController.CurrentSelectBaseCharacter.characterStat;
        SupplyCharacterStat.Atk += EffectAmount;
        _saveTurn = GameManager.Instance.Turn;
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
        if (SupplyCharacterStat == null) return false;
        if (_saveTurn + (DurationRound * 2) == GameManager.Instance.Turn)
        {
            SupplyCharacterStat.Atk -= EffectAmount;
            return false;
        }
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}