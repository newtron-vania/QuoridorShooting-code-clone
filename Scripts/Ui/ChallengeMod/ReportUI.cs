using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.CompilerServices;
using CharacterDefinition;

public class ReportUI : BaseUI
{
    protected override bool IsSorting => true;
    public override UIName ID => UIName.ReportUI;

    enum InputFields
    {
        TitleInput,      // 보고서 제목
        DescriptionInput, // 부연설명
        NameInput,      // 작성자 이름
        EmailInput,     // 작성자 이메일
    }
    enum Buttons
    {
        SubmitButton,     // 제출버튼
        Blocker
    }
    enum Dropdowns
    {
        TypeDropdown,     // 버그 종류
    }
    void Awake()
    {
        Init();
    }
    public override void Init()
    {
        base.Init();
        Bind<InputField>(typeof(InputFields));
        Bind<Button>(typeof(Buttons));
        Bind<Dropdown>(typeof(Dropdowns));

        GetButton((int)Buttons.SubmitButton).gameObject.BindEvent(OnSubmitButtonClicked);
        GetButton((int)Buttons.Blocker).gameObject.BindEvent(OnBlockerClicked);
    }
    public void OnSubmitButtonClicked(PointerEventData pointerEventData)
    {
        string title = GetInputField((int)InputFields.TitleInput).text;
        string description = GetInputField((int)InputFields.DescriptionInput).text;
        string name = GetInputField((int)InputFields.NameInput).text;
        string email = GetInputField((int)InputFields.EmailInput).text;

        Dropdown typeDropdown = GetDropdown((int)Dropdowns.TypeDropdown);
        string type = typeDropdown.options[typeDropdown.value].text;

        LogManager.Instance.GetComponent<CrashReport>().Send(name, email, type, title, description);
        UIManager.Instance.ClosePopupUI(this);
    }
    public void OnBlockerClicked(PointerEventData pointerEventData)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}