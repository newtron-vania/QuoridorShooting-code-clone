using System.Collections.Generic;
using UnityEngine;
using UI.Util;

public enum UIName
{
    None,
    EnemyStateUI,
    EnemyStateBoxUI,
    EnemyActionInfoUI,
    PlayerActionUI,
    StageEndPopUpUI,    // 승리 패배 확인 UI (팝업)
    ChallengeBattleUI,  // 도전모드 메인 UI (고정)
    CharInfoUI,         // 도전모드 캐릭터 정보판 (프레임)
    PausePanelUI,       // 일시정지 UI (팝업)
    OptionPanelUI,      // 옵션 창 UI (팝업)
    SupplyItemUI,       // 보급품 등장 itemUI (프레임)
    SupplyPanelUI,      // 보급품 등장시 뒷배경 Panel이 위에 SupplyItemUI가 생김 (팝업)
    InventoryItemUI,        // 인벤토리 아이템 UI (프레임)
    SupplyUsePanelUI,   // 인벤에서 보급품 선택 화면 (팝업)
    SupplyReUsePopUpUI, // 보급품 중복 사용
    CharInfoHighLighting,  // 캐릭터 인포 UI 하이라이팅 (하이라이팅)
    CharacterHighLighting, // 기물 하이라이팅 (하이라이팅)
    SupplyUseShowPopUp, // 보급품 사용시 설명 팝업 추후 사라질 예정 (임시)
    SetWallUI,           // 벽 설치 여부 확인 UI (팝업)
    ReportUI,          // 버그 리포트 UI (임시)
    PlayerTurnStartUI,  // 플레이어 턴 시작 시 실행될 UI
    SelectWallPosPanelUI, // 벽 건설 요청 UI (팝업)
    SkillUsePanelUI,  // 능력 사용 설명 및 사용 여부 판단 UI (팝업)
    CharDetailFrameUI,  // 캐릭터 디테일 창 UI (프레임)
    ToastItemUI,        // ToastPopUp 아이템 (프레임)
    EnemyTurnProgessUI, // 적턴 진행 중 팝업 창 (팝업)
    CharacterPopUpTextUI, // 캐릭터 토큰 피격, 회피, 회복 Text UI (프레임)
    SelectSkillPosUI,   // 스킬 선택 팝업 (팝업)
    AlertPopUpUI,       // 경고 팝업 UI (팝업)
    RewardUI,           // 보상 확인 (팝업)
    RewardDropMoneyUI,  // 기본 돈 보상 (팝업)
    RewardItemUI,       // 보상 아이템 UI (프레임)
}

public enum LogEvent
{
    Attack,
    SkillUse,
    MissAttack
}

public class UIManager : Singleton<UIManager>
{
    public Queue<string> LogQueue = new Queue<string>(); // 레거시 코드
    private Dictionary<UIName, Stack<BaseUI>> _uiStackDict = new Dictionary<UIName, Stack<BaseUI>>();
    private int _order = 10;

    private Stack<BaseUI> _popupStack = new Stack<BaseUI>(); // 오브젝트 말고 컴포넌트를 담음. 팝업 캔버스 UI 들을 담는다.
    private BaseUI _sceneUI = null; // 현재의 고정 캔버스 UI

    public BaseUI SceneUI { get { return _sceneUI; } private set { _sceneUI = value; } }

    private const string _fallbackSpriteName = "임시적";

    // UI_Root 오브젝트에 계층 구조로 Canvas UI 정렬
    public GameObject Root()
    {
        var root = GameObject.Find("@UI_Root");
        if (root == null)
            root = new GameObject { name = "@UI_Root" };

        return root;
    }

    public void SetCanvas(GameObject go, bool sorting = false)
    {
        var canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;

        if (sorting)
            canvas.sortingOrder = _order++;
        else
            canvas.sortingOrder = 0;
    }

    // 고정 UI 각 씬에 맞는 고정 UI 띄어주기
    public T ShowSceneUI<T>(string name = null) where T : BaseUI
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate($"UI/Scene/{name}");
        T sceneUI = go.GetOrAddComponent<T>();
        _sceneUI = sceneUI;

        go.transform.SetParent(Root().transform);

        return sceneUI;
    }

    // 팝업창 (나중에 사라질 UI들)
    public T ShowPopupUI<T>(string name = null) where T : BaseUI
    {
        if (string.IsNullOrEmpty(name)) // 이름을 안받았을 때
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate($"UI/PopUp/{name}");
        T popup = go.GetOrAddComponent<T>();
        _popupStack.Push(popup);

        go.transform.SetParent(Root().transform);

        return popup;
    }

    public void ClosePopupUI(BaseUI popup) // 안전 차원
    {
        if (_popupStack.Count == 0) // 비어있는 스택이라면 삭제 불가
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!"); // 스택의 가장 위에있는 Peek() 것만 삭제할 수 잇기 때문에 popup이 Peek()가 아니면 삭제 못함
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        BaseUI popup = _popupStack.Pop();
        ResourceManager.Instance.Destroy(popup.gameObject);
        popup = null;
        _order--; // order 줄이기
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : BaseUI
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate($"UI/Frame/{name}", parent);

        return UIUtils.GetOrAddComponent<T>(go);
    }

    public T MakeSubItem<T>(Vector3 position, Quaternion rotation, Transform parent = null, string name = null) where T : BaseUI
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate($"UI/Frame/{name}", position, rotation, parent);

        return UIUtils.GetOrAddComponent<T>(go);
    }
    // 해당 UI에 존재하는 하이라이팅 생성하기
    public T MakeHighlighting<T>(Transform parent, string name = null) where T : BaseUI
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate($"UI/HighLighting/{name}", parent);

        BaseUI baseUI = UIUtils.GetOrAddComponent<T>(go);
        // 하이라이팅 그룹 추가
        if (!_uiStackDict.ContainsKey(baseUI.ID))
        {
            _uiStackDict.Add(baseUI.ID, new Stack<BaseUI>());
        }
        _uiStackDict[baseUI.ID].Push(baseUI);

        return UIUtils.GetOrAddComponent<T>(go);
    }
    // 오브젝트에 생성
    public T MakeUIToParent<T>(Transform parent, string name = null) where T : BaseUI
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate($"UI/Frame/{name}", parent);

        BaseUI baseUI = UIUtils.GetOrAddComponent<T>(go);
        if (!_uiStackDict.ContainsKey(baseUI.ID))
        {
            _uiStackDict.Add(baseUI.ID, new Stack<BaseUI>());
        }
        _uiStackDict[baseUI.ID].Push(baseUI);

        return UIUtils.GetOrAddComponent<T>(go);
    }

    public void CloseUI(UIName uiType)
    {
        if (!_uiStackDict.TryGetValue(uiType, out var popupStack)
            || _uiStackDict[uiType].Count == 0)
            return;

        var popup = _uiStackDict[uiType].Pop();
        ResourceManager.Instance.Destroy(popup.gameObject);
        popup = null;

        CheckUICountAndRemove();
    }

    public void CloseUI(BaseUI baseUI)
    {
        var uiType = baseUI.ID;
        if (!_uiStackDict.TryGetValue(uiType, out var popupStack)
            || _uiStackDict[uiType].Count == 0)
            return;

        if (baseUI != popupStack.Peek())
        {
            Debug.Log("Close Popup Failed");
            return;
        }

        CloseUI(uiType);
    }

    public void CloseAllUI()
    {
        foreach (var kv in _uiStackDict)
        {
            var uiType = kv.Key;
            var uiStack = kv.Value;
            while (uiStack.Count != 0)
            {
                var popup = uiStack.Pop();
                ResourceManager.Instance.Destroy(popup.gameObject);
                popup = null;
            }
        }

        CheckUICountAndRemove();
    }

    public void CloseAllGroupUI(UIName uiType)
    {
        if (!_uiStackDict.TryGetValue(uiType, out var popupStack)
            || _uiStackDict[uiType].Count == 0)
            return;

        while (popupStack.Count != 0)
        {
            var popup = popupStack.Pop();
            ResourceManager.Instance.Destroy(popup.gameObject);
            popup = null;
        }

        CheckUICountAndRemove();
    }

    public void InputLogEvent(LogEvent logEvent, GameObject attacker, GameObject victim = null)
    {
        if (victim == null) Debug.Log($"[INFO]UIManger(InputLogEvent) - 로그 이벤트가 등록됐습니다. {logEvent}, {attacker.name}");
        else Debug.Log($"[INFO]UIManger(InputLogEvent) - 로그 이벤트가 등록됐습니다. {logEvent}, {attacker.name}, {victim.name}");

        switch (logEvent)
        {
            case LogEvent.Attack:
                LogQueue.Enqueue(victim.name.Split("_")[0] + "이(가) " + attacker.name.Split("_")[0] + "에게 데미지를 받았습니다.");
                break;
            case LogEvent.SkillUse:
                LogQueue.Enqueue(attacker.name.Split("_")[0] + "이(가) 능력을 사용했습니다.");
                break;
            case LogEvent.MissAttack:
                LogQueue.Enqueue(victim.name.Split("_")[0] + "이(가) " + attacker.name.Split("_")[0] + "의 공격을 회피했습니다.");
                break;
            default:
                Debug.LogWarning("[WARN]UIManager(InputLogEvent) - 등록되지않은 로그 이벤트입니다.");
                break;
        }
    }

    public T FindUI<T>(string name = null) where T : BaseUI
    {
        T sceneUI = null;
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        foreach (Transform child in Root().transform)
        {
            if (name == child.name)
            {
                sceneUI = child.gameObject.GetComponent<T>();
            }
        }

        return sceneUI;
    }

    public Sprite GetCharacterImage(string name)
    {
        Sprite sprite = ResourceManager.Instance.LoadSprite(name);
        if (sprite == null)
        {
            sprite = ResourceManager.Instance.LoadSprite(_fallbackSpriteName);
        }
        return sprite;
    }

    private void CheckUICountAndRemove()
    {
        var uiType = new List<UIName>();
        foreach (var popupUI in _uiStackDict.Keys) uiType.Add(popupUI);
        for (var i = 0; i < _uiStackDict.Count; i++)
            if (_uiStackDict.GetValueOrDefault(uiType[i]).Count == 0)
                _uiStackDict.Remove(uiType[i]);
        CheckUICountInScene();
    }

    private void CheckUICountInScene()
    {
        Debug.Log($"[INFO] UIManager::CheckUICountInScene() - popupCount : {_uiStackDict.Count}");
        foreach (var popupKey in _uiStackDict.Keys) Debug.Log($"[INFO] UIManager::CheckUICountInScene() - uiList : {popupKey}");
    }

    public int GetAllUICount()
    {
        return _uiStackDict.Count;
    }

    public void Clear()
    {
        CloseAllUI();
    }
}
