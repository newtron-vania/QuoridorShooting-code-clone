using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CharacterDefinition;
using HM;
using EventType = HM.EventType;

public partial class ChallengeBattleUI : BaseUI, IEventListener
{
    enum Texts
    {
        StageRoundText,     // 라운드 표시 Text
        LogText,
        WallCountText,      // 남은 벽 Text
    }

    enum Buttons
    {
        //ReportButton,     // 버그 리포트 버튼
        TurnSkipButton,     // 턴 넘기기 버튼
        OptionButton,       // 옵션 창 띄우기 버튼
        PauseButton,        //정지 창 띄우기 버튼
        SelectPlayerButton, // 아군 정보 UI로 변경
        SelectEnemyButton,  // 적군 정보 UI로 변경
        #region InventoryButton
        SupplyInventoryButton,  // 인벤토리 보급품 UI 변경
        RelicInventoryButton,   // 유물 보급품 UI 변경
        ShortInvenOpenButton, // 작은 인벤토리 창 열기 버튼
        ShortInvenCloseButton, // 작은 인벤토리 창 닫기 버튼
        #endregion
        #region PlayerInputButton
        MoveButton,                 // 기물 움직임 선택
        AttackButton,               // 기물 공격 선택
        SpecialActionButton,        // 특수행동 선택
        SupplyButton,               // 보급품 선택 버튼
        WallSetButton,              // 벽 건설 버튼
        SkillButton,                // 기물 고유 능력 실행 버튼
        SpecialActionCancelButton,  // 특수행동 취소 버튼
        ActionConfirmButton,        // 행동 확정 버튼
        ActionRejectButton,         // 행동 취소 버튼
        #endregion
    }

    enum GameObjects
    {
        CharInfoBackGround,        // 캐릭터 인포출력 부분
        PlayerTurnStartBackGround, // 임시
        CharDetailBoundary,        // 캐릭터 정보 자세히 보기 경계
        ToastPopUpBoundary,        // 토스트 팝업 경계
        #region SupplyGameObjects
        InventorySupplyContent, // 인벤토리 출력 인벤
        InventoryRelicContent,  // 유물 출력 인벤
        RelicCondition,      // 유물 현 상태 표시 칸
        SupplyInvenBackGround, // 보급품 인벤토리 뒷배경
        RelicInvenBackGround, // 유물 인벤톨 뒷배경
        InvenBox, // 작은 인벤토리 게임 오브젝트 (supply, relic 포함)
        #endregion
        #region PlayerInputGameObjects
        DefalutPlayerInput, // 기물 선택 시 UI
        SpecialActionPlayerInput,   // 특수행동 선택 시 UI
        ConfirmPlayerInput // 행동 선택 시 확정 버튼 UI
        #endregion
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.ChallengeBattleUI;

    private bool _canSkipButton = true; // 두번눌림 방지
    private List<GameObject> _destroyList = new List<GameObject>();

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        InventoryUpdate();

        ToastUpdate();

        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    GameManager.Instance.CharacterController.CurrentSelectCharacter.GetComponent<SpriteRenderer>().material.color = new Color(1, 0, 0);
        //}
    }

    public override void Init()
    {
        base.Init();

        Bind<TMP_Text>(typeof(Texts));  // 텍스트 오브젝트들 가져와 dictionary인 _objects에 바인딩.
        Bind<Button>(typeof(Buttons)); // 버튼 오브젝트들 가져와 dictionary인 _objects에 바인딩.
        Bind<GameObject>(typeof(GameObjects)); // 게임 오브젝트 가져와 바인딩

        // 버튼 이벤트 연결
        GetButton((int)Buttons.TurnSkipButton).gameObject.BindEvent(OnClickTurnSkipButton);
        GetButton((int)Buttons.PauseButton).gameObject.BindEvent(OnClickPauseButton);
        GetButton((int)Buttons.OptionButton).gameObject.BindEvent(OnClickOptionButton);

        // 현재 진행 중인 스테이지 표시
        GetText((int)Texts.StageRoundText).text = "STAGE " + GameManager.Stage;

        // 특정 이벤트 발생시 실행 함수 등록
        RegisterEvent();

        InventoryInit();        // Inventory 파셜 클래스 
        PlayerInputInit();      // PlayerInput 파셜 클래스
        CharacterInfoInit();    // CharacterInfo 파셜 클래스
        ToastInit();            // ToastPopUp 파셜 클래스
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        // 토스트 팝업 이벤트 실행
        OnEventToastPopUp(eventType);
        switch (eventType)
        {
            // 적턴이 시작 됐을 때
            case EventType.OnTurnEnd:
                DeactivePlayerInputButtons();
                UIManager.Instance.ShowPopupUI<EnemyTurnProgressUI>();
                UIManager.Instance.CloseAllGroupUI(UIName.CharInfoHighLighting);
                break;
            // 아군턴이 시작 됐을 때
            case EventType.OnRoundEnd:
                DeactivePlayerInputButtons();
                // 적턴 진행 중 팝업창 닫기
                UIManager.Instance.CloseAllPopupUI();
                ShowPlayerTurnStart();
                CharInfoActionHighlighting();
                break;
            // 특정 기물이 선택됐을 때
            case EventType.OnCharacterSelected:
                // 사용 가능 버튼만 활성화 시킬것 적/ 아군 비교
                if (GameManager.Instance.BattleSystem.CurrentSelectCharacter?.tag == "Enemy")
                {
                    DeactivePlayerInputButtons();
                    UIManager.Instance.CloseAllGroupUI(UIName.CharInfoHighLighting);
                }
                else
                {
                    PartActivePlayerInputButtons();
                    _currentPlayerCharacter = (PlayerCharacter)GameManager.Instance.BattleSystem.CurrentSelectBaseCharacter;
                    CharInfoSelectHighLighting();
                }
                break;
            // PlayerInput 화면 초기화
            case EventType.OnCharacterDeselected:
                // 초기화
                DeactivePlayerInputButtons();
                UIManager.Instance.CloseAllGroupUI(UIName.CharInfoHighLighting);
                break;
            case EventType.OnCharacterDead:
                ConvertCharacterInfoPanel(_isPlayerCharacterPanel);
                break;
        }
    }

    public void RegisterEvent()
    {
        EventManager.Instance.AddEvent(EventType.OnRoundEnd, this);         // 플레이어 턴 시작
        EventManager.Instance.AddEvent(EventType.OnTurnEnd, this);        // 적 턴 시작
        EventManager.Instance.AddEvent(EventType.OnCharacterSelected, this);     // 캐릭터 선택 시
        EventManager.Instance.AddEvent(EventType.OnCharacterDeselected, this);    // 캐릭터 선택 초기화
        EventManager.Instance.AddEvent(EventType.OnCharacterDead, this);    // 캐릭터 기물 사망시
        EventManager.Instance.AddEvent(EventType.OnSupplyGet, this);        // 보급품 획득 시
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveEvent(EventType.OnRoundEnd, this);
        EventManager.Instance.RemoveEvent(EventType.OnTurnEnd, this);
        EventManager.Instance.RemoveEvent(EventType.OnCharacterSelected, this);
        EventManager.Instance.RemoveEvent(EventType.OnCharacterDeselected, this);
        EventManager.Instance.RemoveEvent(EventType.OnCharacterDead, this);
        EventManager.Instance.RemoveEvent(EventType.OnSupplyGet, this);
    }
    // 턴 넘기기 버튼 클릭
    public void OnClickTurnSkipButton(PointerEventData data)
    {
        Debug.Log("[INFO] ChallengeBattleUI::OnClickTurnSkipButton() - 턴 넘기기 버튼 클릭");
        if (GameManager.Instance.BattleSystem.IsPlayerTurn && _canSkipButton)
        {
            if (_currentPlayerCharacter != null)
            {
                _currentPlayerCharacter.ChangePlayerControlStatus(PlayerControlStatus.None);
                _currentPlayerCharacter.ResetPreview();
            }
            _canSkipButton = false;
            GameManager.Instance.EndTurn();
            StartCoroutine(OnSkipButtonInit());
        }
    }
    // 일시정지 버튼 클릭 함수
    public void OnClickPauseButton(PointerEventData data)
    {
        UIManager.Instance.ShowPopupUI<PausePanelUI>();
    }
    // 옵션창 열기 버튼 클릭
    public void OnClickOptionButton(PointerEventData data)
    {
        UIManager.Instance.ShowPopupUI<OptionPanelUI>();
    }
    // TEMP : 임시 로그창 띄우기 위한 창함수
    public void ShowLogText(string showText)
    {
        //GetText((int)Texts.LogText).text = showText;
    }
    // 플레이어 턴 실행 시 띄우는 팝업창
    public void ShowPlayerTurnStart()
    {
        UIManager.Instance.ShowPopupUI<PlayerTurnStartPanelUI>();
    }
    #region OnClickReportButtonFunc
    public void OnClickReportButton(PointerEventData data)
    {
        Debug.Log("[INFO] ChallengeBattleUI::OnClickReportButton() - 리포트 버튼 클릭");
        UIManager.Instance.ShowPopupUI<ReportUI>();
    }
    #endregion
    IEnumerator OnSkipButtonInit()
    {
        yield return new WaitForSeconds(1f);
        _canSkipButton = true;
    }
}