using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupplyItemUI : BaseUI
{
    enum Texts
    {
        SupplyItemNameText,  // 보급품 이름
        SupplyDescriptionText, // 보급품 설명
    }

    enum Images
    {
        SupplyItemImage,     // 보금품 이미지
        SupplyIconImage,     // 보급품 아이콘 이미지
    }

    enum GameObjects
    {
        SupplyEffect    // 보급품 뒷배경 이펙트
    }

    enum Buttons
    {
        SupplyBackGround, // 보급품 선택
    }

    protected override bool IsSorting => false;
    private Tweener _tweener;
    public override UIName ID => UIName.SupplyItemUI;
    public int SupplyId;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<GameObject>(typeof(GameObjects));
        Bind<Button>(typeof(Buttons));
        // 이벤트 연결
        GetButton((int)Buttons.SupplyBackGround).gameObject.BindEvent(OnClickSupplySelect);

        SupplymentData supplyData = DataManager.Instance.GetSupplyData(SupplyId);
        GetText((int)Texts.SupplyItemNameText).text = supplyData.Name;

        GetObject((int)GameObjects.SupplyEffect).SetActive(false);
    }

    public void Reset()
    {
        if (SupplyManager.Instance.SelectedSupplyItem != SupplyId)
        {
            GetObject((int)GameObjects.SupplyEffect).SetActive(false);
        }
    }

    public void OnClickSupplySelect(PointerEventData data)
    {
        SupplyManager.Instance.SelectedSupplyItem = SupplyId;
        GetObject((int)GameObjects.SupplyEffect).SetActive(true);
    }
}
