using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using HM;
using HM.Utils;
using UnityEngine;
using UnityEngine.UI;
using EventType = HM.EventType;
using Battle;

// 기물 저장, 생성, 동작 관리하는 스크립트
public partial class BattleSystem : MonoBehaviour, IEventListener, ITurnEventHandler
{
    // -----------------------------
    // 턴 이벤트 관리
    // -----------------------------
    public BattleTurnController TurnController { get; private set; } // 턴 이벤트 중앙 관리자
    public string HandlerName => "BattleSystem"; // ITurnEventHandler 구현


    // -----------------------------
    // 프리팹 관련 변수
    // -----------------------------
    [Header("Prefabs Section")] public GameObject PlayerCharacterPrefab; // 플레이어 캐릭터 프리팹
    public GameObject EnemyCharacterPrefab; // 적 캐릭터 프리팹
    public PlayerPrefabs PlayerPrefabs; // 플레이어 관련 프리팹 모음

    [Header("Wall Prefabs Section")]
    [SerializeField] private GameObject _wallPrefab; // 벽 프리팹
    [SerializeField] private GameObject _previewWallPrefab; // 벽 미리보기 프리팹
    private PreviewWall _previewWall; // 벽 미리보기 인스턴스
    public GameObject PreviewWall => _previewWall.gameObject; // 벽 미리보기 GameObject 접근

    // -----------------------------
    // 기물 생성 및 관리용 변수
    // -----------------------------
    public Dictionary<CharacterIdentification, List<BaseCharacter>> StageCharacter = new(); // 현재 전장에 존재하는 캐릭터 (플레이어/적)
    public Dictionary<CharacterIdentification, List<BaseCharacter>> StageDeadCharacter = new(); // 사망한 캐릭터 목록 (플레이어/적)

    // -----------------------------
    // 셀 관리용 변수
    // -----------------------------
    public CellManager CellManager { get; private set; } // 셀 상태 관리자

    // -----------------------------
    // 전투 시스템 상태 및 판정 변수
    // -----------------------------
    public EnemyController _enemyController { get; set; } // 적 행동 제어용 컨트롤러

    private VictoryCondition _victoryCondition;

    private FailedCondition _failedCondition;

    // -----------------------------
    // 벽 건설 관련 변수 (Battle-specific)
    // -----------------------------

    public GameObject WallHighlightingParent;
    public int NeededBuildPoint = 100; // 벽 건설에 필요한 행동력
    public int EnemyWallCount = 0; // 적이 설치한 벽 개수
    public int RemainedPlayerCharacterCount => StageCharacter[CharacterIdentification.Player].Count; // 남은 플레이어 캐릭터 수
    public int RemainedEnemyCharacterCount => StageCharacter[CharacterIdentification.Enemy].Count; // 남은 적 캐릭터 수
    private bool IsGameEnd => _victoryCondition.IsCheck() || _failedCondition.IsCheck(); // 게임 종료 여부

    private bool IsPlayerWin => _victoryCondition.IsCheck() && !_failedCondition.IsCheck(); // 플레이어 승리 여부


    // Enemy or Player 턴 관리 변수들
    public bool IsPlayerTurn => GameManager.Instance.Turn % 2 == 1 ? true : false;

    // -----------------------------
    // 터치 및 입력 관련 변수
    // -----------------------------
    public PlayerControlStatus PlayerControlStatus
    {
        get
        {
            return _playerControlStatus;
        }
        set
        {
            OnChangeCharacterState(_playerControlStatus, value);
            _playerControlStatus = value;
        }
    }
    private PlayerControlStatus _playerControlStatus = PlayerControlStatus.None; // 플레이어 입력 상태
    public TouchUtils.TouchState TouchState = TouchUtils.TouchState.None; // 현재 터치 상태
    public Vector2 touchPosition = new(0, 0); // 터치된 화면 좌표
    public TouchUtils.TouchType TouchType = TouchUtils.TouchType.CharacterSelect; // 현재 터치의 목적 타입

    // -----------------------------
    // UI 관련 변수
    // -----------------------------
    public List<PlayerActionUI> CharacterActionUis = new(); // 캐릭터별 행동 UI 리스트
    public BaseCharacter CurrentSelectBaseCharacter; // 현재 선택된 캐릭터(BaseCharacter)
    public GameObject CurrentSelectCharacter; // 현재 선택된 캐릭터의 GameObject
    public Transform SaveCurrentTargetPoint // 이동 관련 위치 저장
    {
        get
        {
            return _saveCurrentTargetPoint;
        }
        set
        {
            // case 1 : 처음 셀이 지정되었을 때
            if (!_saveCurrentTargetPoint)
            {
                // 새로 들어온 값이 null일 경우
                if (!value) return;
                value.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 0.4f);
                _saveCurrentTargetPoint = value;
                return;
            }
            // case 2 : 셀 지정이 풀렸을 때
            if (!value)
            {
                _saveCurrentTargetPoint.GetComponent<SpriteRenderer>().color = new Color(0.278f, 0.714f, 1f, 0.4f);
                _saveCurrentTargetPoint = value;
                return;
            }
            // case 3 : 선택한 셀을 변경할 때
            // 이전 셀 색상 변경
            _saveCurrentTargetPoint.GetComponent<SpriteRenderer>().color = new Color(0.278f, 0.714f, 1f, 0.4f);
            // 선택된 셀 색상 변경
            value.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 0.4f);
            _saveCurrentTargetPoint = value;
        }
    }
    public BaseCharacter SaveTargetBaseCharacter // 공격관련 하이라이팅 및 타겟 정보 저장
    {
        get
        {
            return _saveTargetBaseCharacter;
        }
        set
        {
            // case 1 : 새로운 공격 캐릭터 지정
            if (_saveTargetBaseCharacter == null)
            {
                if (value == null) return;
                UIManager.Instance.MakeHighlighting<TokenHighLighting>(GetObjectToPosition(GameManager.ToWorldPosition(value.Position)).transform);
                _saveTargetBaseCharacter = value;
                return;
            }
            // case 2 : 공격 캐릭터 지정 해제
            if (value == null)
            {
                UIManager.Instance.CloseAllGroupUI(UIName.CharacterHighLighting);
                _saveTargetBaseCharacter = value;
                return;
            }
            // case 3 : 공격 캐릭터 재지정
            UIManager.Instance.CloseAllGroupUI(UIName.CharacterHighLighting);
            UIManager.Instance.MakeHighlighting<TokenHighLighting>(GetObjectToPosition(GameManager.ToWorldPosition(value.Position)).transform);
            _saveTargetBaseCharacter = value;
        }
    }
    public Transform SaveCurrentAbilityPoint = null; // 현재 능력 사용 공간 저장
    public bool IsConfirm // 확인버튼 활성화 여부 저장
    {
        get
        {
            return _saveCurrentTargetPoint || _saveTargetBaseCharacter != null || SaveCurrentAbilityPoint || (_previewWall != null && _previewWall.gameObject.activeInHierarchy && _previewWall.CanBuild);
        }
    }
    private Transform _saveCurrentTargetPoint = null;
    private BaseCharacter _saveTargetBaseCharacter = null;
    private bool _isEndFlag = true;

    // -----------------------------
    //  내부 시스템 관련 변수
    // -----------------------------
    public SkillSystem SkillSystem { get; private set; } // 어빌리티 시스템 인스턴스

    private void Awake()
    {
        if (WallHighlightingParent == null)
        {
            WallHighlightingParent = GameObject.Find("WallHighlightingParent");
        }
    }

    private void Start()
    {
        // ===== TurnController 초기화 (가장 먼저) =====
        TurnController = new BattleTurnController(this);
        TurnController.Initialize();

        //어빌리티 시스템 초기화
        //전투 새롭게 시작될 때마다 새로 생성해야 됩니다.
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.PlayerFieldSetting);
        SkillSystem = GameObject.Find("SkillSystem").GetComponent<SkillSystem>();
        SkillSystem.Init();
        CellManager = new CellManager(this);

        _enemyController = new EnemyController(this);

        // // 셀 관리자 초기화
        // CellManager = new CellManager(this);

        RegisterEvents();

        // 승리 및 패배 조건 초기화
        InitializeConditions();

        SpawnCharacters();

        // 기물 Start함수 실행
        PlayersStart();
        EnemyStart();

        // wall 관리 관련 코드 (추후 이동 예정)
        // wall 및 wallPreview 초기화
        GameObject previewWall = Instantiate(_previewWallPrefab, Vector3.zero, Quaternion.identity);
        _previewWall = previewWall.GetComponent<PreviewWall>();
        _previewWall.gameObject.SetActive(false);

        // TEMP : 보급품 상자 생성 임의로 2개
        SupplyManager.Instance.SpawnSupplyBox(2);

        // ===== 모든 시스템을 TurnController에 등록 =====
        RegisterSystemsToTurnController();

        // 디버깅용: 등록된 핸들러 출력
        TurnController.LogRegisteredHandlers();
    }


    private void Update()
    {
        OnUpdateQueue();

        if (!(TriggerManager.Instance.IsTriggerActive(GameTrigger.PlayerFieldSetting) && TriggerManager.Instance.IsTriggerActive(GameTrigger.EnemyFieldSetting)))
        {
            return;
        }

        if (IsGameEnd)
        {
            // Debug.Log($"[INFO] CharacterController::Update() - Game is Over! Winner is {(IsPlayerWin ? "Player" : "Enemy")}");
            // DestroyImmediate(this); // 승리 패배 UI 추가 전까지 상남자식 종료
            if (_isEndFlag)
            {
                UIManager.Instance.ShowPopupUI<StageEndPanelUI>().IsPlayerWin = IsPlayerWin;
                _isEndFlag = false;
            }

            return;
        }

        GetCilckObject();

        UpdateCommand();
        // Player 턴일 때 Player 기물 업데이트 실시
        if (IsPlayerTurn)
        {
            if (CurrentSelectBaseCharacter != null && CurrentSelectBaseCharacter.Playerable)
                CurrentSelectBaseCharacter.Update();
        }
        // Enemy 턴
        else
        {
            _enemyController.Update();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            var enemyList = StageCharacter[CharacterIdentification.Enemy].ToList();
            foreach (var enemy in enemyList)
            {
                BaseCharacter player = StageCharacter[CharacterIdentification.Player][0];
                InsertCharacterCommand(new AttackCommand(player, enemy));
                enemy?.TakeDamage(player, 100);
                InsertCharacterCommand(new WaitForTimeCommand(0.1f));
            }
        }
    }

    public void RegisterEvents()
    {
        // BattleTurnController가 EventManager에 구독하므로
        // BattleSystem은 직접 구독하지 않음
        // EventManager.Instance.AddEvent(EventType.OnTurnStart, this);
        // EventManager.Instance.AddEvent(EventType.OnTurnEnd, this);
    }

    /// <summary>
    /// 모든 시스템을 TurnController에 등록
    /// </summary>
    private void RegisterSystemsToTurnController()
    {
        // BattleSystem 자신을 등록 (캐릭터 이벤트 처리)
        TurnController.RegisterTurnStartEvent(this, TurnEventPriority.TURN_START_CHARACTER_RESET);
        TurnController.RegisterTurnEndEvent(this, TurnEventPriority.TURN_END_CHARACTER_CLEANUP);

        // StatManager 등록 (싱글톤)
        if (StatManager.Instance != null)
        {
            TurnController.RegisterTurnEndEvent(StatManager.Instance, TurnEventPriority.TURN_END_STAT_DURATION);
        }

        // DurableEffectRegistry 등록 (싱글톤)
        if (DurableEffectRegistry.Instance != null)
        {
            TurnController.RegisterTurnStartEvent(DurableEffectRegistry.Instance, TurnEventPriority.TURN_START_DURABLE_EFFECT);
            TurnController.RegisterTurnEndEvent(DurableEffectRegistry.Instance, TurnEventPriority.TURN_END_DURABLE_EFFECT);
        }

        // CellManager 등록
        if (CellManager != null)
        {
            TurnController.RegisterTurnEndEvent(CellManager, TurnEventPriority.TURN_END_CELL_DURATION);
        }

        Debug.Log("[INFO] BattleSystem::RegisterSystemsToTurnController - All systems registered to TurnController");
    }

    /// <summary>
    /// ITurnEventHandler 구현 - 턴 시작 시 캐릭터 이벤트 처리
    /// </summary>
    public void OnTurnStart()
    {
        var currentTurnCharacterList = IsPlayerTurn
            ? StageCharacter[CharacterIdentification.Player]
            : StageCharacter[CharacterIdentification.Enemy];

        Debug.Log($"[INFO] BattleSystem::OnTurnStart - Processing {currentTurnCharacterList.Count} characters");

        foreach (var character in currentTurnCharacterList)
            character.OnTurnStart();
    }

    /// <summary>
    /// ITurnEventHandler 구현 - 턴 종료 시 캐릭터 이벤트 처리
    /// </summary>
    public void OnTurnEnd()
    {
        var currentTurnCharacterList = IsPlayerTurn
            ? StageCharacter[CharacterIdentification.Player]
            : StageCharacter[CharacterIdentification.Enemy];

        Debug.Log($"[INFO] BattleSystem::OnTurnEnd - Processing {currentTurnCharacterList.Count} characters");

        foreach (var character in currentTurnCharacterList)
            character.OnTurnEnd();

        // CellManager.ProcessTurnEnd()는 TurnController를 통해 자동 호출됨 (Priority: 30)
    }

    /// <summary>
    /// EventManager에서 호출되는 이벤트 핸들러 (현재는 사용하지 않음)
    /// TurnController가 EventManager와 통신함
    /// </summary>
    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        // TurnStart/TurnEnd는 TurnController를 통해 처리되므로
        // 여기서는 다른 이벤트만 처리
        switch (eventType)
        {
            case EventType.OnTurnStart:
            case EventType.OnTurnEnd:
                // TurnController가 처리하므로 무시
                break;
            // 다른 이벤트 처리...
        }
    }

    private void OnChangeCharacterState(PlayerControlStatus beforeState, PlayerControlStatus currentState)
    {
        if (beforeState == currentState) return;

        // Build에서 다른 상태로 변경
        if (beforeState == PlayerControlStatus.Build)
        {
            foreach (BaseCharacter child in StageCharacter[CharacterIdentification.Player])
            {
                child.CharacterObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            }
            foreach (BaseCharacter child in StageCharacter[CharacterIdentification.Enemy])
            {
                child.CharacterObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
            }
            foreach (Transform child in WallHighlightingParent.transform)
            {
                child.gameObject.SetActive(false);
            }
        }
        // Build 상태로 돌입
        else if (currentState == PlayerControlStatus.Build)
        {
            foreach (BaseCharacter child in StageCharacter[CharacterIdentification.Player])
            {
                child.CharacterObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
            foreach (BaseCharacter child in StageCharacter[CharacterIdentification.Enemy])
            {
                child.CharacterObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
            }
            foreach (Transform child in WallHighlightingParent.transform)
            {
                if (child.GetComponent<WallHighlightingData>().CanBuild) child.gameObject.SetActive(true);
            }
        }
    }


    #region Enemy&PlayerTotalFunc

    private void GetCilckObject()
    {
        // touchState, touchPos 값 변경 자세한 내용은 TouchSetUp 정의피킹
        TouchUtils.TouchSetUp(ref TouchState, ref touchPosition);

        // 현재 터치하는게 보급품을 선택하기 위함인지 판단
        if (TouchType == TouchUtils.TouchType.UseSupply)
        {
            if (TouchState == TouchUtils.TouchState.Began)
            {
                // LayerMask에 따라 "기물"인지 아닌지 판별
                Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out var raycastHit, 30f,
                    LayerMask.GetMask("Ground"));
                var hit = Physics2D
                    .RaycastAll(Camera.main.ScreenToWorldPoint(touchPosition), Camera.main.transform.forward, 30f,
                        LayerMask.GetMask("Token"))
                    .Where(hit =>
                        Vector2.Distance(GameManager.ToWorldPosition(GameManager.ToGridPosition(hit.point)),
                            raycastHit.point) <= GameManager.GridSize / 2)
                    .OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault();

                // 적 아군 상관없이 기물을 선택했을 때
                if (hit.collider)
                {
                    switch (SupplyManager.Instance._supplyTarget)
                    {
                        case TargetType.TargetEnemy:
                            if (hit.collider.gameObject.tag == "Enemy")
                            {
                                CurrentSelectCharacter = hit.transform.gameObject;
                                // 클릭한 캐릭터 + _ + id  형태에서 고유한 값 id만 추출
                                var correctID = int.Parse(CurrentSelectCharacter.name
                                    .Substring(CurrentSelectCharacter.name.IndexOf("_") + 1).Trim());
                                // player 캐릭터 중에서 알맞은 id 찾기
                                foreach (var child in StageCharacter[CharacterIdentification.Enemy])
                                    if (child.Id == correctID)
                                        CurrentSelectBaseCharacter = child;
                            }

                            break;
                        case TargetType.Allies:
                            if (hit.collider.gameObject.tag == "Player")
                            {
                                CurrentSelectCharacter = hit.transform.gameObject;
                                // 클릭한 캐릭터 + _ + id  형태에서 고유한 값 id만 추출
                                var correctID = int.Parse(CurrentSelectCharacter.name
                                    .Substring(CurrentSelectCharacter.name.IndexOf("_") + 1).Trim());
                                // player 캐릭터 중에서 알맞은 id 찾기
                                foreach (var child in StageCharacter[CharacterIdentification.Player])
                                    if (child.Id == correctID)
                                        CurrentSelectBaseCharacter = child;
                            }

                            break;
                        default:
                            Debug.LogWarning("[WARN]CharacterController GetCilckObject - 잘못된 기물을 선택했습니다.");
                            break;
                    }

                    if (CurrentSelectBaseCharacter != null)
                    {
                        if (!SupplyManager.Instance.CanUseSupply(SupplyManager.Instance.CurrentUseBaseSupply.ID,
                                CurrentSelectBaseCharacter.characterStat.Index))
                            UIManager.Instance.ShowPopupUI<SupplyReUsePopUpUI>();
                        else
                            SupplyManager.Instance.CurrentUseBaseSupply.UseSupply();
                    }

                    TouchType = TouchUtils.TouchType.CharacterSelect;
                    // UIManager.Instance.CloseAllGroupUI(UIName.CharacterHighLighting);
                }
                // 기물이 아닌 다른곳을 클릭했다면 보급품 사용 취소
                else
                {
                    SupplyManager.Instance._currentUseSupply = 0;
                    SupplyManager.Instance._supplyTarget = TargetType.None;
                    TouchType = TouchUtils.TouchType.CharacterSelect;
                    // TEMP : 기물 하이라이팅 기법을 SD캐릭터가 나오면 다르게 변경할 예정 그때까지는 임시로 작성
                    // UIManager.Instance.CloseAllGroupUI(UIName.CharacterHighLighting);
                }

                UIManager.Instance.CloseAllGroupUI(UIName.CharacterHighLighting);
            }
        }

        // 아직 어떤 기물도 선택하지 않았을 때 + 단순하게 캐릭터 선택하는 턴일 때
        else if (PlayerControlStatus == PlayerControlStatus.None && TouchType == TouchUtils.TouchType.CharacterSelect)
        {
            if (TouchState == TouchUtils.TouchState.Began)
            {
                // LayerMask에 따라 "기물"인지 아닌지 판별
                Physics.Raycast(Camera.main.ScreenPointToRay(touchPosition), out var raycastHit, 30f,
                    LayerMask.GetMask("Ground"));
                var hit = Physics2D
                    .RaycastAll(Camera.main.ScreenToWorldPoint(touchPosition), Camera.main.transform.forward, 30f,
                        LayerMask.GetMask("Token"))
                    .Where(hit =>
                        Vector2.Distance(GameManager.ToWorldPosition(GameManager.ToGridPosition(hit.point)),
                            raycastHit.point) <= GameManager.GridSize / 2)
                    .OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault();
                Debug.DrawRay(Camera.main.ScreenToWorldPoint(touchPosition), Camera.main.transform.forward * 30f,
                    Color.red, 1f);

                if (hit.collider != null)
                {
                    if (CurrentSelectBaseCharacter != null) CurrentSelectBaseCharacter.ResetPreview();
                    CurrentSelectCharacter = hit.transform.gameObject;
                    var correctID = int.Parse(CurrentSelectCharacter.name
                         .Substring(CurrentSelectCharacter.name.IndexOf("_") + 1).Trim());
                    // 플레이어 기물일때
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        // player 캐릭터 중에서 알맞은 id 찾기
                        foreach (var child in StageCharacter[CharacterIdentification.Player])
                            if (child.Id == correctID)
                                CurrentSelectBaseCharacter = child;
                        // Player 턴일 때 작동할 UI 화면
                        if (IsPlayerTurn)
                            // 캐릭터 선택 인보크 실행
                            EventManager.Instance.InvokeEvent(EventType.OnCharacterSelected, this);
                    }
                    else //적 기물일 때
                    {
                        // Enemy 캐릭터 중에서 알맞은 id 찾기
                        foreach (var child in StageCharacter[CharacterIdentification.Enemy])
                            if (child.Id == correctID)
                                CurrentSelectBaseCharacter = child;

                    }

                    CurrentSelectBaseCharacter.ShowPreviewsOnCharacterTouch();
                }

                // 어떤 기물도 선택되지 않았을 때 -> 비어있는 화면을 눌렀을때 화면에 보이는 모든 UI 제거 (기본 UI 제외)
                else
                {
                    if (CurrentSelectBaseCharacter != null) CurrentSelectBaseCharacter.ResetPreview();
                    CurrentSelectBaseCharacter = null;
                    CurrentSelectCharacter = null;
                    SaveCurrentTargetPoint = null;
                    // 모든 actionUis 초기화
                    UIManager.Instance.CloseUI(UIName.CharInfoHighLighting);
                    EventManager.Instance.InvokeEvent(EventType.OnCharacterDeselected, this);
                    //uiManager.PassiveEnemyInfoUI();
                }
            }
        }
    }

    public void AllCharacterReset(CharacterIdentification charDefine)
    {
        foreach (var child in StageCharacter[charDefine]) child.Reset();
    }

    #endregion

    #region EnemyFunc

    private void EnemyStart()
    {
        _enemyController.Start();
    }

    #endregion

    #region PlayerFunc

    private void PlayersStart()
    {
        foreach (var playerCharacter in StageCharacter[CharacterIdentification.Player]) playerCharacter.Start();
    }

    #endregion

    #region UseBaseCharacterFunc

    public GameObject GetObjectToPosition(Vector3 pos)
    {
        foreach (var character in CharacterObjects)
            if (character.transform.position == pos)
                return character;
        return null;
    }

    public GameObject SetObjectToParent(GameObject child, GameObject parent = null, Vector3? pos = null, Quaternion? rot = null)
    {
        if (parent)
            return ResourceManager.Instance.Instantiate(child, pos ?? Vector3.zero, rot ?? Quaternion.identity, parent.transform);
        return ResourceManager.Instance.Instantiate(child, pos ?? Vector3.zero, rot ?? Quaternion.identity);
    }

    public List<Vector2Int> GetBaseCharactersVec2IntPosition(List<BaseCharacter> baseCharacters)
    {
        var characterPositions = new List<Vector2Int>();

        foreach (var character in baseCharacters) characterPositions.Add(character.Position);

        return characterPositions;
    }

    public List<Vector2Int> GetAllCharactersPosition()
    {
        var characterPositions = new List<Vector2Int>();

        foreach (var characterKv in StageCharacter)
            foreach (var baseCharacter in characterKv.Value)
                characterPositions.Add(baseCharacter.Position);
        return characterPositions;
    }

    #endregion

    #region CheckCharacterControlEndFunc

    public void KillPlayerCharacterById(int characterId)
    {
        if (StageCharacter.TryGetValue(CharacterIdentification.Player, out var playerList))
        {
            var characterToRemove = playerList.FirstOrDefault(c => c.Id == characterId);
            if (characterToRemove != null)
            {
                playerList.Remove(characterToRemove);
                StageDeadCharacter[CharacterIdentification.Player].Add(characterToRemove);
                Debug.Log(
                    $"[INFO] CharacterController::KillPlayerCharacterById : [RemovePlayerCharacterById] Removed character with Id: {characterId}");
            }
            else
            {
                Debug.LogWarning(
                    $"[Warning] CharacterController::KillPlayerCharacterById : [RemovePlayerCharacterById] Character with Id {characterId} not found.");
            }
        }
    }

    public void KillEnemyCharacterById(int characterId)
    {
        if (StageCharacter.TryGetValue(CharacterIdentification.Enemy, out var enemyList))
        {
            var characterToRemove = enemyList.FirstOrDefault(c => c.Id == characterId);
            if (characterToRemove != null)
            {
                enemyList.Remove(characterToRemove);
                StageDeadCharacter[CharacterIdentification.Enemy].Add(characterToRemove);
                Debug.Log(
                    $"[INFO] CharacterController::KillEnemyCharacterById :[RemoveEnemyCharacterById] Removed character with Id: {characterId}");
            }
            else
            {
                Debug.LogWarning(
                    $"[Warning] CharacterController::KillEnemyCharacterById :[RemoveEnemyCharacterById] Character with Id {characterId} not found.");
            }
        }
    }

    #endregion

    #region ResetFunc

    public void Reset()
    {
        // TurnController 정리 (가장 먼저)
        TurnController?.Clear();

        _readyToSpawnCharacterData.Clear();
        StageCharacter.Clear();
        GameManager.Instance.BattleSystem = null;
        EventManager.Instance.EnsureIntegrity();
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.PlayerFieldSetting);
    }

    #endregion

    #region ConditionInitialization

    /// <summary>
    /// 승리 및 패배 조건 초기화
    /// </summary>
    private void InitializeConditions()
    {
        // 승리 조건: 적이 모두 사망
        _victoryCondition = new NoneEnemyVictoryCondition(null, this);

        // 패배 조건: 플레이어가 모두 사망
        _failedCondition = new NonePlayerFailedCondition(null, this);

        Debug.Log("[INFO] CharacterController::InitializeConditions() - Victory and Failed conditions initialized");
    }

    #endregion

    #region WallManagement

    // 벽 미리보기 생성
    public void SetWallPreview(Vector2Int position, bool isHorizontal)
    {
        _previewWall.transform.position = GameManager.ToWorldPosition((Vector2)position + new Vector2(0.5f, 0.5f));
        _previewWall.transform.rotation = Quaternion.Euler(0, 0, isHorizontal ? 90 : 0);
        _previewWall.gameObject.SetActive(true);
    }

    // 벽 생성
    public bool SetWall()
    {
        if (_previewWall == null)
        {
            return false;
        }

        if (!_previewWall.gameObject.activeSelf)
        {
            Debug.Log("[WARN] CharacterController::SetWall() - WallPreview is not active");
            return false;
        }

        if (!_previewWall.CanBuild)
        {
            Debug.Log("[WARN] CharacterController::SetWall() - Can't build wall");
            return false;
        }

        PrefabControllerBase wallPrefab = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.Wall).GetController(Vector2Int.zero);
        wallPrefab.transform.position = _previewWall.transform.position;
        wallPrefab.transform.rotation = _previewWall.transform.rotation;

        GameManager.Instance.WallList.Add(new WallData(wallPrefab.transform.position, _previewWall.transform.rotation.eulerAngles.z == 90));
        return true;
    }

    // 벽 즉시 생성 (디버깅용)
    public void SetWallInstantly(Vector2Int position, bool isHorizontal)
    {
        PrefabControllerBase wallPrefab = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.Wall).GetController(Vector2Int.zero);
        wallPrefab.transform.position = GameManager.ToWorldPosition((Vector2)position + new Vector2(0.5f, 0.5f));
        wallPrefab.transform.rotation = Quaternion.Euler(0, 0, isHorizontal ? 90 : 0);

        GameManager.Instance.WallList.Add(new WallData(wallPrefab.transform.position, isHorizontal));
    }

    #endregion
}