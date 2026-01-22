using CharacterDefinition;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillUsePanelUI : BaseUI
{
    enum Buttons
    {
        YesButton,  // 능력 사용
        NoButton,   // 능력 미사용
    }
    enum Texts
    {
        SkillNameText,        // 능력 이름에 대한 텍스트
        SkillDescriptionText, // 능력에 대한 설명 텍스트
        SkillTagText,         // 능력 태그 설명 텍스트
    }
    enum Images
    {
        SkillIconImage,   // 능력 아이콘 이미지
        // 스킬 이미지 넣어야함
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.SkillUsePanelUI;

    public int SkillID = -1;
    public ChallengeBattleUI ChallengeBattle;
    private SkillData _skillData;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        GetButton((int)Buttons.YesButton).gameObject.BindEvent(OnClickYesButton);
        GetButton((int)Buttons.NoButton).gameObject.BindEvent(OnClickNoButton);

        _skillData = DataManager.Instance.GetSkillData(SkillID);
        SetSkillPanel();
    }

    private void OnClickYesButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
        ChallengeBattle.UsePlayerSkill();
    }

    private void OnClickNoButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
    }

    private void SetSkillPanel()
    {
        GetText((int)Texts.SkillNameText).text = _skillData.Name;
        GetText((int)Texts.SkillDescriptionText).text = _skillData.Description;
    }
}
