using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlertPopUpUI : BaseUI
{
    enum Buttons
    {
        ExitButton,
    }
    enum Texts
    {
        DescriptionText,
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.AlertPopUpUI;

    public string ShowText = string.Empty;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        GetText((int)Texts.DescriptionText).text = ShowText;

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);
    }

    public void OnClickExitButton(PointerEventData data)
    {
        UIManager.Instance.ClosePopupUI(this);
    }
}
