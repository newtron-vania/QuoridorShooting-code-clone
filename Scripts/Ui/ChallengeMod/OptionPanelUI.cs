using HM.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionPanelUI : BaseUI
{
    enum Buttons
    {
        OptionApplyButton,   // 변경 확정
        OptionExitButton     // 팝업 닫기 버튼
    }

    enum Sliders
    {
        SFXSlider,          // 효과음 사운드 슬라이더
        BGMSlider           // 배경음 사운드 슬라이더
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.OptionPanelUI;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<Button>(typeof(Buttons));
        Bind<Slider>(typeof(Sliders));

        // 소리 저장
        if (!PlayerPrefs.HasKey("SFXSound"))
            PlayerPrefs.SetInt("SFXSound", 0);
        if (!PlayerPrefs.HasKey("BGMSound"))
            PlayerPrefs.SetInt("BGMSound", 0);

        // 버튼 이벤트 연결
        GetButton((int)Buttons.OptionApplyButton).gameObject.BindEvent(OnClickApplyButton);
        GetButton((int)Buttons.OptionExitButton).gameObject.BindEvent(OnClickExitButton);

        // Slider 값 설정
        GetSlider((int)Sliders.SFXSlider).value = PlayerPrefs.GetInt("SFXSound");
        GetSlider((int)Sliders.BGMSlider).value = PlayerPrefs.GetInt("BGMSound");
    }

    public void OnClickApplyButton(PointerEventData data)
    {
        PlayerPrefs.SetInt("SFXSound", (int)GetSlider((int)Sliders.SFXSlider).value);
        PlayerPrefs.SetInt("BGMSound", (int)GetSlider((int)Sliders.BGMSlider).value);
    }

    public void OnClickExitButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
