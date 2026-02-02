using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CharacterDefinition;
using UI.Define;

// PlayerInput UI Stack 클래스
public class PlayerUIStack
{
    Stack<GameObject> stack = new Stack<GameObject>();

    public int Count
    {
        get { return stack.Count; }
    }

    public void Push(GameObject value)
    {
        // 초기 DefalutPlayerInput들어 왔을 때는 실행 x
        if (stack.Count > 0)
        {
            stack.Peek().SetActive(false);
            value.SetActive(true);
        }
        stack.Push(value);
    }

    public GameObject Pop()
    {
        GameObject popObject = stack.Pop();
        stack.Peek().SetActive(true);
        popObject.SetActive(false);
        return popObject;
    }

    public GameObject Peek()
    {
        return stack.Peek();
    }
}

// 플레이어 기물 클릭시 PlayerInput 고정화면 관리 (PlayerInput)
public partial class ChallengeBattleUI : BaseUI
{
    private PlayerUIStack _playerUIStack = new PlayerUIStack();
    // 0 : Move, 1 : Attack, 2 : SpecialAction, 3 : Supply, 4 : WallSet, 5 : Skill, 6 : ActionConfirm
    private Dictionary<PlayerInputUI.ButtonType, PlayerInputButtonData> _playerInputDatas = new Dictionary<PlayerInputUI.ButtonType, PlayerInputButtonData>();
    private PlayerCharacter _currentPlayerCharacter;
    private BattleSystem _controller;

    private void PlayerInputInit()
    {
        // 버튼 이벤트 연결
        GetButton((int)Buttons.MoveButton).gameObject.BindEvent(OnClickMoveButton);
        GetButton((int)Buttons.AttackButton).gameObject.BindEvent(OnClickAttackButton);
        GetButton((int)Buttons.SpecialActionButton).gameObject.BindEvent(OnClickSpecialActionButton);
        GetButton((int)Buttons.SupplyButton).gameObject.BindEvent(OnClickSupplyButton);
        GetButton((int)Buttons.WallSetButton).gameObject.BindEvent(OnClickSetWallButton);
        GetButton((int)Buttons.SkillButton).gameObject.BindEvent(OnClickUseSkillButton);
        GetButton((int)Buttons.ActionConfirmButton).gameObject.BindEvent(OnClickConfirmButton);
        // 취소 버튼 눌렀을 떄
        GetButton((int)Buttons.ActionRejectButton).gameObject.BindEvent(OnClickPlayerInputBackButton);
        GetButton((int)Buttons.SpecialActionCancelButton).gameObject.BindEvent(OnClickPlayerInputBackButton);

        // PlayerInput 게임 오브젝트 비활성화
        GetObject((int)GameObjects.ConfirmPlayerInput).SetActive(false);
        GetObject((int)GameObjects.SpecialActionPlayerInput).SetActive(false);

        // PlayerInputData.cs 넣어주기
        SetPlayerInputData();

        // 모든 버튼 비활성화
        DeactivePlayerInputButtons();
        // 기본 플레이어 인풋 UI 화면 넣어두기 Size는 항상 1로 고정
        _playerUIStack.Push(GetObject((int)GameObjects.DefalutPlayerInput));

        _controller = GameManager.Instance.BattleSystem;
    }

    private void SetPlayerInputData()
    {
        _playerInputDatas.Add(PlayerInputUI.ButtonType.Move, GetButton((int)Buttons.MoveButton).gameObject.GetComponent<PlayerInputButtonData>());
        _playerInputDatas.Add(PlayerInputUI.ButtonType.Attack, GetButton((int)Buttons.AttackButton).gameObject.GetComponent<PlayerInputButtonData>());
        _playerInputDatas.Add(PlayerInputUI.ButtonType.SpecialAction, GetButton((int)Buttons.SpecialActionButton).gameObject.GetComponent<PlayerInputButtonData>());
        _playerInputDatas.Add(PlayerInputUI.ButtonType.Supply, GetButton((int)Buttons.SupplyButton).gameObject.GetComponent<PlayerInputButtonData>());
        _playerInputDatas.Add(PlayerInputUI.ButtonType.WallSet, GetButton((int)Buttons.WallSetButton).gameObject.GetComponent<PlayerInputButtonData>());
        _playerInputDatas.Add(PlayerInputUI.ButtonType.Skill, GetButton((int)Buttons.SkillButton).gameObject.GetComponent<PlayerInputButtonData>());
        _playerInputDatas.Add(PlayerInputUI.ButtonType.Confirm, GetButton((int)Buttons.ActionConfirmButton).gameObject.GetComponent<PlayerInputButtonData>());
    }

    // 이동버튼 클릭
    private void OnClickMoveButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.Move].IsActive)
        {
            ExecuteAction(0, PlayerControlStatus.Move);
            _playerUIStack.Push(GetObject((int)GameObjects.ConfirmPlayerInput));
        }
    }
    // 공격버튼 클릭
    private void OnClickAttackButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.Attack].IsActive)
        {
            ExecuteAction(1, PlayerControlStatus.Attack);
            _playerUIStack.Push(GetObject((int)GameObjects.ConfirmPlayerInput));
        }
    }
    // 특수행동 버튼 클릭 (벽건설 & 스킬)
    private void OnClickSpecialActionButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.SpecialAction].IsActive)
        {
            _playerUIStack.Push(GetObject((int)GameObjects.SpecialActionPlayerInput));
        }
    }
    // 보급품 선택 화면 이동
    private void OnClickSupplyButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.Supply].IsActive)
        {

        }
    }
    // 한단계 이전 팝업으로 이동
    private void OnClickPlayerInputBackButton(PointerEventData data)
    {
        _controller.SaveCurrentTargetPoint = null;
        UIManager.Instance.ClosePopupUI();
        _controller.PlayerControlStatus = PlayerControlStatus.None;
        _currentPlayerCharacter.ResetPreview();
    }
    // 벽건설 버튼
    private void OnClickSetWallButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.WallSet].IsActive)
        {
            UIManager.Instance.ShowPopupUI<SelectWallPosPopUpUI>();
            ExecuteAction(3, PlayerControlStatus.Build);
            _playerUIStack.Push(GetObject((int)GameObjects.ConfirmPlayerInput));
        }
    }
    // 스킬 사용 버튼
    private void OnClickUseSkillButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.Skill].IsActive)
        {
            SkillUsePanelUI skillUseUI = UIManager.Instance.ShowPopupUI<SkillUsePanelUI>();
            skillUseUI.SkillID = _currentPlayerCharacter.SkillId;
            skillUseUI.ChallengeBattle = this;
            // 능력 설명 팝업창 부터 띄어줘야함
        }
    }
    // 확정 버튼 클릭 EventManager로 수정하는 것도 생각해야할듯
    private void OnClickConfirmButton(PointerEventData data)
    {
        if (_playerInputDatas[PlayerInputUI.ButtonType.Confirm].IsActive)
        {
            switch (GameManager.Instance.BattleSystem.PlayerControlStatus)
            {
                // 확정 버튼을 눌렀을 때 각각 실행되어야하는 이벤트 등록
                case PlayerControlStatus.Move:
                    _controller.CurrentSelectBaseCharacter.Position = GameManager.ToGridPosition(_controller.SaveCurrentTargetPoint.position); //플레이어 위치 이동
                    _currentPlayerCharacter.ReduceMoveCount();
                    if (_controller.PlayerControlStatus == PlayerControlStatus.Move) _controller.PlayerControlStatus = PlayerControlStatus.None;
                    _currentPlayerCharacter.ResetPreview(); // 프리뷰 초기화
                    break;

                // 공격상황에서 확정버튼 클릭 시 작동
                case PlayerControlStatus.Attack:
                    BaseCharacter player = _controller.CurrentSelectBaseCharacter;
                    BaseCharacter enemy = _controller.SaveTargetBaseCharacter;
                    bool check = _controller.InsertCharacterCommand(new AttackCommand(player, enemy));
                    Debug.Log($"[INFO] EnemyCharacter::Attack() - AttackCommand On : {check}");
                    Debug.Log($"[INFO] EnemyCharacter::Attack() - {player.CharacterName} Attack target name & hp : {enemy.CharacterName + $"_" + enemy.Id} & {enemy.Hp}");
                    enemy?.TakeDamage(player, player.Atk);
                    Debug.Log($"[INFO] EnemyCharacter::Attack() - Attack Target! remainedHp = {enemy.Hp}");

                    _controller.InsertCharacterCommand(new WaitForTimeCommand(0.1f));
                    player.ReduceAttackCount();
                    if (_controller.PlayerControlStatus == PlayerControlStatus.Attack)
                    {
                        _controller.PlayerControlStatus = PlayerControlStatus.None;
                    }
                    player.ResetPreview();
                    break;

                case PlayerControlStatus.Build:
                    if (_controller.SetWall()) // 벽 설치
                    {
                        _currentPlayerCharacter.characterStat.Ap -= _controller.NeededBuildPoint; // 행동력 소모
                        GameManager.Instance.playerWallCount++; // 설치한 벽 개수 +1
                    }
                    GameManager.Instance.BattleSystem.PlayerControlStatus = PlayerControlStatus.None;
                    UIManager.Instance.ClosePopupUI();
                    _currentPlayerCharacter.ResetPreview();
                    break;

                case PlayerControlStatus.Skill:
                    _currentPlayerCharacter.ConfirmUseAbility();
                    break;
            }
        }
    }
    #region PlayerInputButtonFunc
    public void UsePlayerSkill()
    {
        ExecuteAction(4, PlayerControlStatus.Skill);
        UIManager.Instance.ShowPopupUI<SelectSkillPosUI>();
        _playerUIStack.Push(GetObject((int)GameObjects.ConfirmPlayerInput));
    }

    // 플레이어 버튼 클릭 활성화 플레이어 턴 활성화 (턴넘기기 + PlayerInput UI)
    private void ActivePlayerInputButtons()
    {
        foreach (KeyValuePair<PlayerInputUI.ButtonType, PlayerInputButtonData> pair in _playerInputDatas)
        {
            pair.Value.IsActive = true;
        }
        ResetPlayerInputUI();
    }
    // 플레이어 버튼 클릭 비활성화 플레이어 턴 비활성화
    private void DeactivePlayerInputButtons()
    {
        foreach (KeyValuePair<PlayerInputUI.ButtonType, PlayerInputButtonData> pair in _playerInputDatas)
        {
            pair.Value.IsActive = false;
        }
        ResetPlayerInputUI();
        if (_currentPlayerCharacter != null)
        {
            _currentPlayerCharacter.ResetPreview();
            _currentPlayerCharacter = null;
        }
    }
    // 기물 선택 or 플레이어 행동 후 실행될 함수
    private void PartActivePlayerInputButtons()
    {
        // 처음 화면으로 초기화
        ResetPlayerInputUI();
        // 선택된 캐릭터가 있을 경우 해당 조건이 참인 경우 BaseCharacter가 존재한다는 가정
        if (GameManager.Instance.BattleSystem.CurrentSelectCharacter)
        {
            BaseCharacter currentSelectCharacter = GameManager.Instance.BattleSystem.CurrentSelectBaseCharacter;
            _playerInputDatas[PlayerInputUI.ButtonType.Attack].IsActive = currentSelectCharacter.CanAttack;
            _playerInputDatas[PlayerInputUI.ButtonType.Move].IsActive = currentSelectCharacter.CanMove;
            _playerInputDatas[PlayerInputUI.ButtonType.SpecialAction].IsActive = (currentSelectCharacter.CanUseSkill || currentSelectCharacter.CanBuild);
            _playerInputDatas[PlayerInputUI.ButtonType.WallSet].IsActive = currentSelectCharacter.CanBuild;
            _playerInputDatas[PlayerInputUI.ButtonType.Skill].IsActive = currentSelectCharacter.CanUseSkill;
            _playerInputDatas[PlayerInputUI.ButtonType.Supply].IsActive = true; // TEMP : 추후 보급품 필터 함수를 이용해 사용가능한 보급품이 있는지 없는지 판단할 예정
        }
        else
        {
            Debug.Log("[WARD] ChallengeBattleUI.PlayerInput(PartActivePlayerInputButtons) - 선택된 기물이 없어 UI를 갱신할 수 없습니다.");
        }
    }
    // DefalutPlayerInputUI로 변경
    private void ResetPlayerInputUI()
    {
        while (_playerUIStack.Count > 1)
        {
            _playerUIStack.Pop();
        }
    }

    // 액션 실행 처리
    private void ExecuteAction(int index, PlayerControlStatus status)
    {
        Debug.Log($"[INFO]PlayerActionUI::ExecuteAction - {status}");
        //ActSelectAnimation(index);
        _currentPlayerCharacter.ResetPreview();
        _currentPlayerCharacter.ChangePlayerControlStatus(status);
    }
    #endregion
}