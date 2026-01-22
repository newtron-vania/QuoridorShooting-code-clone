using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyStardust : BaseSupply
{
    public override int ID => 18;
    public override string Name => "별가루";
    public override string Description => "2라운드 동안 모든 아군 캐릭터의 공격력이 1 증가한다.";
    public override SupplymentData.SupplyType Type => SupplymentData.SupplyType.Enhance;
    public override SupplymentData.SupplyTarget Target => SupplymentData.SupplyTarget.Allies;
    public override SupplymentData.SupplyGrade Rank => SupplymentData.SupplyGrade.Normal;
    public override int DurationRound => 2;
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
        foreach (BaseCharacter child in GameManager.Instance.CharacterController.StageCharacter[CharacterDefinition.CharacterIdentification.Player])
        {
            child.characterStat.Atk += EffectAmount;
            _saveTurn = GameManager.Instance.Turn;
        }
        // TEMP : QA이후 업데이트 될 예정
        SupplyUseShowPanelUI supplyUseShowPanelUI = UIManager.Instance.ShowPopupUI<SupplyUseShowPanelUI>();
        supplyUseShowPanelUI.SupplyName = Name;
        supplyUseShowPanelUI.TargetName = "아군전체";
    }

    public override void TargetHighLight()
    {
        base.TargetHighLight();
    }

    public override bool UpdateSupply()
    {
        base.UpdateSupply();
        if (_saveTurn + (DurationRound * 2) == GameManager.Instance.Turn)
        {
            foreach (BaseCharacter child in GameManager.Instance.CharacterController.StageCharacter[CharacterDefinition.CharacterIdentification.Player])
            {
                child.characterStat.Atk -= EffectAmount;
            }
            return false;
        }
        return true;
    }

    public override void InitSupply()
    {
        base.InitSupply();
    }
}