using HM;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DetailBackGround : BaseUI
{
    enum Texts
    {
        CharNameText,           // 캐릭터 이름
        SkillNameText,          // 스킬 이름
        SkillDescriptionText,   // 스킬 설명
    }

    enum Images
    {
        CharImage,
        SkillImage,
    }

    enum Buttons // 버튼 클릭 시 해당 버프, 디버프 디테일한 설명창 오픈
    {
        BuffImage_1,
        BuffImage_2,
        BuffImage_3,
        DeBuffImage_1,
        DeBuffImage_2,
        DeBuffImage_3,
        ExitButton,     // 끄기 버튼
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.CharDetailFrameUI;

    public BaseCharacter FrameBaseCharacter = null;
    private SkillData _skillData = null;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);

        _skillData = DataManager.Instance.GetSkillData(FrameBaseCharacter.SkillId);

        SettingPanel();
    }

    private void OnClickExitButton(PointerEventData data)
    {
        UIManager.Instance.CloseUI(UIName.CharDetailFrameUI);
    }

    private void SettingPanel()
    {
        GetText((int)Texts.CharNameText).text = FrameBaseCharacter.CharacterName;
        GetText((int)Texts.SkillNameText).text = _skillData.Name;
        GetText((int)Texts.SkillDescriptionText).text = _skillData.Description;
    }
}
