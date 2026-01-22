using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyLandMine : BaseSupply
{
    public override int ID => 4;
    public override string Name => "지뢰";
    public override string Description => "대상 적에게 지뢰를 설치하고 다음 턴에 체력을 2만큼 감소시킨다. ";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Attack;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetEnemy;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Normal;
    public override int DurationRound => 2;
    public override int EffectAmount => 2;
    public override Sprite Image => null;
    public override CharacterStat SupplyCharacterStat { get => _useTargetBaseCharacter; set => _useTargetBaseCharacter = value; }

    private CharacterStat _useTargetBaseCharacter;
    private int _saveTurn = 0;

    public override void UseSupply()
    {
        base.UseSupply();
        if (DurationRound != 0)
        {
            SupplyManager.Instance._saveDurationSupply.Add(this);
        }
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
        if (_saveTurn + DurationRound == GameManager.Instance.Turn)
        {
            SupplyCharacterStat.Hp -= EffectAmount;
            return false;
        }
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}