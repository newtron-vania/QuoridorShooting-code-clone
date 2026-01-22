using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HM;
using CharacterDefinition;
using DG.Tweening;

public class CharInfoItemUI : BaseUI
{
    enum Buttons
    {
        CharInfoButton      // 캐릭터 버튼
    }
    enum Texts
    {
        CharAttackText,     // 캐릭터 공격력 ex) 3, 6, 4
        CharDefenceText,    // 캐릭터 회피?피해저항 ex) 10%, 30%
        CharHpText,         // 캐릭터 체력 현재/최대 ex) 15/20
        PopUpText,          // 상성 관계 행동력 충전 여부 우측 상단 PopUpText
        CharTiaText,        // QA용 Text
        CharNameText        // QA용 Text
    }

    enum Images
    {
        CharInfoBackGround, // 캐릭터 박스 뒷배경 이미지
        CharImage,          // 캐릭터 이미지
        CharSynastryImage,  // 캐릭터 상성 이미지
        //CharTiaImage1,      // 캐릭터 행동력칸
        //CharTiaImage2,
        //CharTiaImage3,
    }

    enum GameObjects
    {
        HighLight
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.CharInfoUI;

    public BaseCharacter ItemBaseCharacter;
    public bool IsPlayer;
    public Transform DetailParent;
    public GameObject HighParent;

    private CharacterController _controller;
    private Tween _warnTween;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        SetInfo();
    }

    public override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<GameObject>(typeof(GameObjects));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.CharInfoButton).gameObject.BindEvent(OnClickCharItem);
        HighParent = GetObject((int)GameObjects.HighLight);

        // 오브젝트 찾기
        _controller = GameManager.Instance.CharacterController;

        SetInfo();
        ItemBaseCharacter.OnDamage += OnCharacterTakeDamage;
        GetImage((int)Images.CharImage).sprite = UIManager.Instance.GetCharacterImage(ItemBaseCharacter.CharacterName);
        //GetText((int)Texts.CharNameText).text = ItemBaseCharacter.CharacterName;
    }

    public void HighLighting()
    {
        UIManager.Instance.CloseAllGroupUI(UIName.CharInfoHighLighting);
        if(GetObject((int)GameObjects.HighLight)) UIManager.Instance.MakeHighlighting<CharInfoHighLighting>(GetObject((int)GameObjects.HighLight).transform).HighlightingID = 0;
    }

    public void FullActionHighlighting()
    {
        UIManager.Instance.MakeHighlighting<CharInfoHighLighting>(GetObject((int)GameObjects.HighLight).transform).PopUpText = GetText((int)Texts.PopUpText);
    }

    public void SetInfo()
    {
        GetText((int)Texts.CharAttackText).text = ItemBaseCharacter.Atk.ToString();
        GetText((int)Texts.CharDefenceText).text = (ItemBaseCharacter.characterStat.Avd * 100) + "%";
        string hpString = ItemBaseCharacter.Hp < ItemBaseCharacter.MaxHp ? "<color=#FF3B3B>" : "<color=#000000>";
        GetText((int)Texts.CharHpText).text = hpString + ItemBaseCharacter.Hp + "</color>/" + ItemBaseCharacter.MaxHp;
        GetText((int)Texts.CharTiaText).text = ItemBaseCharacter.characterStat.Ap.ToString();
        // 행동력 관련도 추가해야함
    }

    private void OnCharacterTakeDamage(int damage)
    {
        if (ItemBaseCharacter.MaxHp * 0.8 >= ItemBaseCharacter.Hp)
        {
            if(_warnTween == null)
            {
                _warnTween = GetImage((int)Images.CharInfoBackGround).DOColor(new Color(0.8f, 0, 0), 1).SetLoops(-1, LoopType.Yoyo);
                Debug.LogFormat("[INFO]CharInfoItemUI::OnCharacterTakeDamage - {0}의 피가 20퍼 이하로 떨어졌습니다.", ItemBaseCharacter.CharacterName);
            }
        }
        else
        {
            if(_warnTween != null)
            {
                _warnTween.Kill();
            }
        }
    }

    public void OnClickCharItem(PointerEventData data)
    {
        UIManager.Instance.CloseUI(UIName.CharDetailFrameUI);
        UIManager.Instance.MakeUIToParent<DetailBackGround>(DetailParent).FrameBaseCharacter = ItemBaseCharacter;
    }
}
