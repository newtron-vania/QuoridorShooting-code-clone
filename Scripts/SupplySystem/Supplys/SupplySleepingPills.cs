using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplySleepingPills : BaseSupply
{
    public override int ID => 2;
    public override string Name => "수면제";
    public override string Description => "적이 주는 피해량을 3만큼 감소시킨다.";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Attack;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.TargetEnemy;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Rare;
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
        // 행동불능으로 만들 캐릭터값 넣어두기
        SupplyCharacterStat.Atk -= EffectAmount;
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
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}