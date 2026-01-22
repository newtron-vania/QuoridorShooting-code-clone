using System.Collections.Generic;
using UnityEngine;
using CharacterDefinition;
using HM.Utils;
using HM.Physics;
using System.Linq;
using HM;

public partial class PlayerCharacter : BaseCharacter
{
    private GameObject _wallStorage;
    private GameObject _previewStorage;
    // private GameObject _previewWall;


    private List<PrefabControllerBase> _playerMovePreviews = new List<PrefabControllerBase>();

    private List<PrefabControllerBase> _playerAttackPreviews = new List<PrefabControllerBase>();

    private PrefabControllerBase _playerAttackHighlight;


    public PlayerActionUI ActionUi;

    private Vector2 _touchPosition = new Vector2(0, 0);
    private Ray _touchRay;
    int[] wallInfo = new int[3]; // 벽 위치 정보, 회전 정보 저장
    private Vector2 _wallStartPos = new Vector2(-50, -50);

    public PlayerCharacter(CharacterController controller) : base(controller) { Playerable = true;/*임시?*/ } 

    public override void Start()
    {
        PlayerStart();
        base.Start();
    }

    public override void Update()
    {
        base.Update();
        PlayerUpdate();
    }

    public override void Reset()
    {
        base.Reset();
    }

    public override void Die()
    {
        ActionUi.DestroyPlayerActionUI();
        base.Die();
    }

    // 플레이어 컨트롤 상태 변경
    public void ChangePlayerControlStatus(PlayerControlStatus status)
    {
        Controller.PlayerControlStatus = status;

        switch (status)
        {
            case PlayerControlStatus.None:
                break;

            case PlayerControlStatus.Move:
                _canMoveCount = 1;
                SetPreviewMove();
                break;

            case PlayerControlStatus.Build:
                break;

            case PlayerControlStatus.Attack:
                CanAttackCount = 1;
                SetPreviewAttack();
                break;

            case PlayerControlStatus.Skill:
                SetPreviewSkill();
                break;

            case PlayerControlStatus.Destroy:
                break;

            default:
                break;
        }
    }

    public override void Attack()
    {
        //SetPreviewAttack();
        //SetAttackHighlight();
        if (Controller.TouchState == TouchUtils.TouchState.Ended) //화면 클릭시
        {
            Transform currentSelectTransform = Controller.CurrentSelectCharacter.transform;
            Debug.DrawRay(_touchRay.origin, _touchRay.direction * 30f, Color.red, 10f);
            Physics.Raycast(_touchRay, out RaycastHit raycastHit, 30f, LayerMask.GetMask("Ground"));
            RaycastHit2D previewHit = Physics2D.RaycastAll(_touchRay.origin, _touchRay.direction, 30f, LayerMask.GetMask("Preview")).Where(hit => Vector2.Distance(GameManager.ToWorldPosition(GameManager.ToGridPosition(hit.point)), raycastHit.point) <= (GameManager.GridSize / 2)).OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault();
            if (previewHit)
            {
                if (previewHit.transform.CompareTag("PlayerAttackPreview")) // 클릭좌표에 플레이어공격미리보기가 있다면
                {
                    Vector2Int attackPosition = GameManager.ToGridPosition(previewHit.transform.position);
                    BaseCharacter hitCharacter = GameManager.Instance.CharacterController.StageCharacter[CharacterIdentification.Enemy].Where(c => c.Position == attackPosition).FirstOrDefault();
                    if (hitCharacter != null)
                    {
                        Controller.SaveTargetBaseCharacter = hitCharacter;
                    }
                }
            }
            else //다른데 클릭하면 다시 선택화면으로
            {
                Controller.PlayerControlStatus = PlayerControlStatus.None;
                ResetPreview();
            }
        }
        base.Attack();
    }

    public override void Move()
    {
        //SetPreviewMove();
        if (Controller.TouchState == TouchUtils.TouchState.Began) //화면 클릭시
        {
            Transform currentSelectTransform = Controller.CurrentSelectCharacter.transform;
            Physics.Raycast(_touchRay, out RaycastHit raycastHit, 30f, LayerMask.GetMask("Ground"));
            RaycastHit2D previewHit = Physics2D.RaycastAll(_touchRay.origin, _touchRay.direction, 30f, LayerMask.GetMask("Preview")).Where(hit => Vector2.Distance(GameManager.ToWorldPosition(GameManager.ToGridPosition(hit.point)), raycastHit.point) <= (GameManager.GridSize / 2)).OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault();
            if (previewHit)
            {
                if (previewHit.transform.CompareTag("PlayerPreview")) // 클릭좌표에 플레이어미리보기가 있다면
                {
                    Controller.SaveCurrentTargetPoint = previewHit.transform; // 확정 전 위치 정보 저장
                    return;
                }
            }
            else //다른 곳 클릭 시 다시 선택으로
            {
                Controller.PlayerControlStatus = PlayerControlStatus.None;
                ResetPreview();
            }
        }
        base.Move();
    }

    public void ConfirmUseAbility()
    {
        //능력 실행
        // AbilitySystem.ExecuteAbility(AbilityId, this, GameManager.ToGridPosition(Controller.SaveCurrentAbilityPoint.position));

        if (Controller.PlayerControlStatus == PlayerControlStatus.Move) Controller.PlayerControlStatus = PlayerControlStatus.None;
        ResetPreview();
    }

    public override void UseSkill()
    {
        base.UseSkill();

        if (Controller.TouchState == TouchUtils.TouchState.Began) //화면 클릭시
        {
            Transform currentSelectTransform = Controller.CurrentSelectCharacter.transform;
            Physics.Raycast(_touchRay, out RaycastHit raycastHit, 30f, LayerMask.GetMask("Ground"));
            RaycastHit2D previewHit = Physics2D.RaycastAll(_touchRay.origin, _touchRay.direction, 30f, LayerMask.GetMask("Preview")).Where(hit => Vector2.Distance(GameManager.ToWorldPosition(GameManager.ToGridPosition(hit.point)), raycastHit.point) <= (GameManager.GridSize / 2)).OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault();
            if (previewHit)
            {
                if (previewHit.transform.CompareTag("SkillPreview")) // 클릭좌표에 플레이어미리보기가 있다면 //어택 프리뷰랑 같이써도 될 것 같슴다
                {
                    for (int i = 0; i < _playerMovePreviews.Count; i++)
                    {
                        PrefabMakerSystem.Instance.GetObjectMaker(MakerType.MovePreview).ReturnController(_playerMovePreviews[i]);
                    }
                    _playerMovePreviews.Clear();
                    // 능력 실행
                    SkillSystem.ExecuteSkill(SkillId,  GameManager.ToGridPosition(previewHit.transform.position), this);

                    Controller.PreviewWall.SetActive(false);

                    for (int i = 0; i < _playerAttackPreviews.Count; i++)
                    {
                        PrefabMakerSystem.Instance.GetObjectMaker(MakerType.AttackPreview).ReturnController(_playerAttackPreviews[i]);
                    }
                    _playerAttackPreviews.Clear();

                    Controller.SaveCurrentAbilityPoint = previewHit.transform;
                    UIManager.Instance.ClosePopupUI();
                    SetPreviewAbilityRange();
                    return;
                }
            }
            else //다른 곳 클릭 시 다시 선택으로
            {
                Controller.PlayerControlStatus = PlayerControlStatus.None;
                // if (isDisposableMove)
                // {
                //     moveCount--;
                //     isDisposableMove = false;
                // }
                //ActionUi.ActivateUI();
                ResetPreview();
            }
        }
    }

    public override void Build()
    {
        if (GameManager.Instance.playerWallCount >= GameManager.Instance.playerMaxBuildWallCount) return;
        if (Controller.TouchState == TouchUtils.TouchState.Began || Controller.TouchState == TouchUtils.TouchState.Moved || Controller.TouchState == TouchUtils.TouchState.Ended)
        {
            GameManager.Instance.WallHighlightingParent.SetActive(false);
            SetPreviewWall();
        }
        else
        {
            GameManager.Instance.WallHighlightingParent.SetActive(true);
        }
        base.Build();
    }

    public override void ResetPreview()
    {
        base.ResetPreview();
        // 미리보기들 비활성화

        for (int i = 0; i < _playerMovePreviews.Count; i++)
        {
            PrefabMakerSystem.Instance.GetObjectMaker(MakerType.MovePreview).ReturnController(_playerMovePreviews[i]);
        }
        _playerMovePreviews.Clear();

        Controller.PreviewWall.SetActive(false);

        for (int i = 0; i < _playerAttackPreviews.Count; i++)
        {
            PrefabMakerSystem.Instance.GetObjectMaker(MakerType.AttackPreview).ReturnController(_playerAttackPreviews[i]);
        }
        _playerAttackPreviews.Clear();
        // 기존에 이동 위치 저장 타일 초기화
        Controller.SaveCurrentTargetPoint = null;
        Controller.SaveTargetBaseCharacter = null;
        Controller.SaveCurrentAbilityPoint = null;
        HM.EventManager.Instance.InvokeEvent(HM.EventType.OnCharacterSelected, new Component());

        // ??무슨 코드??
        //foreach (var wallObject in Controller.PreviewWall.GetComponent<PreviewWall>().WallObjectList)
        //{
        //    wallObject.GetComponent<SpriteRenderer>().color = Color.white;
        //}
    }

    public override void RecoverHealth(int recovery)
    {
        base.RecoverHealth(recovery);
    }

    public override int TakeDamage(BaseCharacter baseCharacter, int damage = 0)
    {
        return base.TakeDamage(baseCharacter, damage);
    }

    private void PlayerStart()
    {
        _wallStorage = GameObject.FindGameObjectWithTag("WallStorage");
        _previewStorage = GameObject.FindGameObjectWithTag("PreviewStorage");



        //하이라이트 같은건 캐릭터마다 하는게 아니라 컨트롤러에서 공용으로 관리하면 좋을 듯
        //오브젝트 풀에 넣었다 뺏다 할필요가 없어 보임
        if (!_playerAttackHighlight)
        {
            _playerAttackHighlight = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.HighlightPreview).GetController(Vector2Int.zero);
            _playerAttackHighlight.gameObject.SetActive(false);
        }

        // Controller.PreviewWall = Controller.SetObjectToParent(Controller.PlayerPrefabs.WallPreview, null, GameManager.ToWorldPosition(Position)); // 플레이어 벽 미리보기 -> 미리소환하여 비활성화 해놓기
        // Controller.PreviewWall.SetActive(false);
        _canMoveCount = 1;
        CanAttackCount = 1;
    }

    private void PlayerUpdate()
    {
        _touchRay = Camera.main.ScreenPointToRay(Controller.touchPosition);
        _touchPosition = Camera.main.ScreenToWorldPoint(Controller.touchPosition);
        switch (Controller.PlayerControlStatus)
        {
            case PlayerControlStatus.Move:
                Move();
                break;
            case PlayerControlStatus.Build:
                Build();
                break;
            case PlayerControlStatus.Attack:
                Attack();
                break;
            case PlayerControlStatus.Skill:
                UseSkill();
                break;
            //case EPlayerControlStatus.Destroy:
            //    if (canDestroy) Destroy();
            //    else ResetPreview();
            //    break;

            default:
                break;
        }
    }

    #region PreviewSetting
    private void SetPreviewMove()
    {
        for (int i = 0; i < characterStat.MovablePositions.Count; i++)
        {
            bool[] result = RayUtils.CheckWallRay(Controller.CurrentSelectCharacter.transform.position, characterStat.MovablePositions[i]);
            if (result[0])
            {
                continue;
            }
            if (!result[1])
            {
                if (!result[2])
                {
                    Debug.DrawRay(Controller.CurrentSelectCharacter.transform.position, (Vector2)characterStat.MovablePositions[i] * GameManager.GridSize, Color.green, 0.1f);

                    var prefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.MovePreview).GetController(Controller.CurrentSelectBaseCharacter.Position + characterStat.MovablePositions[i]);
                    _playerMovePreviews.Add(prefabController);
                }
                else
                {
                    Debug.DrawRay(Controller.CurrentSelectCharacter.transform.position, (Vector2)characterStat.MovablePositions[i] * GameManager.GridSize, Color.yellow, 0.1f);
                }
                continue;
            }
            else
            {
                Debug.DrawRay(Controller.CurrentSelectCharacter.transform.position, (Vector2)characterStat.MovablePositions[i] * GameManager.GridSize, Color.red, 0.1f);
            }
        }
    }

    private void SetPreviewAttack()
    {
        for (int i = 0; i < characterStat.AttackablePositions.Count; i++)
        {
            bool canSetPreview = false;
            bool isOuterWall = true;
            Vector2 direction = Quaternion.AngleAxis(Mathf.Atan2(characterStat.AttackablePositions[i].y, characterStat.AttackablePositions[i].x) * Mathf.Rad2Deg, Vector3.forward) * new Vector2(0, 0);

            bool[] result = RayUtils.CheckWallRay(Controller.CurrentSelectCharacter.transform.position, characterStat.AttackablePositions[i] + direction);

            PrefabControllerBase prefabController = null;

            if (!result[0]) isOuterWall = false;
            if (!result[1])
            {
                canSetPreview = true;

                prefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.AttackPreview).GetController(Controller.CurrentSelectBaseCharacter.Position + characterStat.AttackablePositions[i]);
                _playerAttackPreviews.Add(prefabController);


            }
            if (isOuterWall)
            {
                prefabController?.gameObject.SetActive(false);

                continue;
            }
            if (canSetPreview) continue;


            prefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.BlockedPreview).GetController(Controller.CurrentSelectBaseCharacter.Position + characterStat.AttackablePositions[i]);
            _playerAttackPreviews.Add(prefabController);
        }
    }

    private void SetAttackHighlight()
    {
        Physics.Raycast(_touchRay, out RaycastHit raycastHit, 30f, LayerMask.GetMask("Ground"));
        RaycastHit2D previewHit = Physics2D.RaycastAll(_touchRay.origin, _touchRay.direction, 30f, LayerMask.GetMask("Preview")).Where(hit => Vector2.Distance(GameManager.ToWorldPosition(GameManager.ToGridPosition(hit.point)), raycastHit.point) <= (GameManager.GridSize / 2)).OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault();
        //Debug.Log($"[INFO] PlayerCharacter::SetAttackHighlight() - previewHit : {GameManager.ToWorldPosition(GameManager.ToGridPosition(previewHit.point))}, raycastHit : {raycastHit.point} Vector2.Distance : {Vector2.Distance(previewHit.point, raycastHit.point)}");
        //Debug.DrawRay(_touchRay.origin, _touchRay.direction * 30f, Color.red, 10f);
        //Debug.DrawLine(GameManager.ToWorldPosition(GameManager.ToGridPosition(previewHit.point)), raycastHit.point, Color.blue, 10f);
        if (!previewHit)
        {

            _playerAttackHighlight.gameObject.SetActive(false);
            return;
        }
        Vector2 atkDirection = ((Vector2)(previewHit.transform.position - Controller.CurrentSelectCharacter.transform.position) / GameManager.GridSize).normalized;
        Vector2 direction = Quaternion.AngleAxis(Mathf.Atan2(atkDirection.y, atkDirection.x) * Mathf.Rad2Deg, Vector3.forward) * new Vector2(0, 0);
        // Debug.Log((atkDirection + direction).normalized);
        bool[] result = RayUtils.CheckWallRay(Controller.CurrentSelectCharacter.transform.position, atkDirection + direction);
        if (result[0])
        {
            _playerAttackHighlight.gameObject.SetActive(false);
        }


        _playerAttackHighlight.transform.position = previewHit.transform.position + GameManager.GridSize * new Vector3(Mathf.Round(direction.x), Mathf.Round(direction.y), 0);
        _playerAttackHighlight.gameObject.SetActive(true);

        Debug.DrawRay(Controller.CurrentSelectCharacter.transform.position, (atkDirection + direction).normalized * (previewHit.transform.position - Controller.CurrentSelectCharacter.transform.position).magnitude, _playerAttackHighlight.GetComponent<SpriteRenderer>().color, 0.1f);
        if (!result[1])
        {
            // _playerAttackHighlights[0].GetComponent<SpriteRenderer>().color = Color.cyan;
        }
        else
        {
            // _playerAttackHighlights[0].GetComponent<SpriteRenderer>().color = Color.grey;
        }
    }

    private void SetPreviewSkill()
    {
        List<Vector2Int> targetablePositions = SkillSystem.GetTargetablePositionList(SkillId, Position);
        SkillPositions = SkillSystem.GetRangePositionList(SkillId, Position);

        foreach (var pos in SkillPositions)
        {
            if (targetablePositions.Contains(pos))
                continue;
            var prefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.BlockedPreview).GetController(pos);
            _playerAttackPreviews.Add(prefabController);
        }
        
        for (int i = 0; i < targetablePositions.Count; i++)
        {
            var prefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillPreview).GetController(targetablePositions[i]);
            _playerAttackPreviews.Add(prefabController);

            //bool canSetPreview = false;
            //bool isOuterWall = true;
            //Vector2 direction = Quaternion.AngleAxis(Mathf.Atan2(_skillPositions[i].y, _skillPositions[i].x) * Mathf.Rad2Deg, Vector3.forward) * new Vector2(0, 0);

            //bool[] result = RayUtils.CheckWallRay(Controller.CurrentSelectCharacter.transform.position, _skillPositions[i] + direction);
            //if (!result[0]) isOuterWall = false;
            //if (!result[1])
            //{
            //    canSetPreview = true;
            //    _playerSkillPreviews[i].transform.position = Controller.CurrentSelectCharacter.transform.position + GameManager.ToWorldPosition(_skillPositions[i]);
            //    _playerSkillPreviews[i].GetComponent<SpriteRenderer>().color = Color.red;
            //    _playerSkillPreviews[i].GetComponent<BoxCollider2D>().enabled = true;
            //    _playerSkillPreviews[i].SetActive(true);
            //}
            //if (isOuterWall)
            //{
            //    _playerSkillPreviews[i].SetActive(false);
            //    continue;
            //}
            //if (canSetPreview) continue;
            //_playerSkillPreviews[i].transform.position = Controller.CurrentSelectCharacter.transform.position + GameManager.ToWorldPosition(_skillPositions[i]);
            //_playerSkillPreviews[i].GetComponent<SpriteRenderer>().color = Color.grey;
            //_playerSkillPreviews[i].GetComponent<BoxCollider2D>().enabled = false;
            //_playerSkillPreviews[i].SetActive(true);
        }
    }

    // 능력 적용 범위 표현
    private void SetPreviewAbilityRange()
    {
        List<Vector2Int> abilityRanges = new List<Vector2Int>();
        abilityRanges = DataManager.Instance.GetRangeData(DataManager.Instance.GetSkillData(SkillId).TargetRangeId);

        Vector2Int useAbilityPoint = GameManager.ToGridPosition(GameManager.Instance.CharacterController.SaveCurrentAbilityPoint.position);

        for (int i = 0; i < abilityRanges.Count; i++)
        {
            bool[] result = RayUtils.CheckWallRay(Controller.SaveCurrentAbilityPoint.position, abilityRanges[i]);

            if (result[1] || result[0]) continue;

            PrefabControllerBase prefabController = null;
            prefabController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.SkillRangePreview).GetController(useAbilityPoint + abilityRanges[i]);
            _playerAttackPreviews.Add(prefabController);
        }
    }

    private void SetPreviewWall()
    {
        // Debug.Log("Starting SetPreviewWall");
        bool hasHit = Physics.Raycast(_touchRay, out RaycastHit raycastHit, 30f, LayerMask.GetMask("Ground"));
        // RaycastHit2D hit = Physics2D.RaycastAll(_touchRay.origin, _touchRay.direction, 30f, LayerMask.GetMask("Ground")).Where(hit=> Vector2.Distance(hit.point, raycastHit.point) <= (GameManager.GridSize / 2)).OrderBy(hit => Vector2.Distance(hit.point, raycastHit.point)).FirstOrDefault(); // 마우스 위치가 땅 위라면
        Debug.DrawRay(_touchRay.origin, _touchRay.direction * 30f, Color.red, 10f);

        if (hasHit) // 마우스 위치가 땅 위라면
        {
            // Debug.Log(Mathf.Floor((touchPosition / GameManager.gridSize).x));
            Vector2 touchGridPosition = (Vector2)raycastHit.point / GameManager.GridSize;
            float[] touchPosFloor = { Mathf.Floor(touchGridPosition.x), Mathf.Floor(touchGridPosition.y) }; // 벽 좌표
            if (Controller.TouchState == TouchUtils.TouchState.Began)
            {
                UIManager.Instance.CloseUI(UIName.SetWallUI);
                // tempMapGraph = (int[,])gameManager.mapGraph.Clone(); // 맵정보 새로저장
                Controller.PreviewWall.SetActive(false); // 비활성화
                if (touchPosFloor[0] < -4 || touchPosFloor[0] > 3 || touchPosFloor[1] < -4 || touchPosFloor[1] > 3) // 벽 좌표가 땅 밖이라면
                {
                    Controller.PreviewWall.SetActive(false); // 비활성화
                    return;
                }
                if (Mathf.Abs(Mathf.Round(touchGridPosition.x) - touchGridPosition.x) > 0.2f) // 마우스 x 위치가 일정 범위 안이면
                {
                    Controller.PreviewWall.transform.position = GameManager.GridSize * new Vector3(Mathf.Floor(touchGridPosition.x) + 0.5f, Mathf.Floor(touchGridPosition.y) + 0.5f, 0);
                    Controller.PreviewWall.transform.rotation = Quaternion.Euler(0, 0, 0); // 위치 이동 및 회전
                                                                                                     // 벽 위치 정보, 회전 정보 저장
                    _wallStartPos.x = Mathf.FloorToInt((Controller.PreviewWall.transform.position / GameManager.GridSize).x);
                    _wallStartPos.y = Mathf.FloorToInt((Controller.PreviewWall.transform.position / GameManager.GridSize).y);
                    Controller.PreviewWall.SetActive(true); // 활성화
                }
                else if (Mathf.Abs(Mathf.Round(touchGridPosition.y) - touchGridPosition.y) > 0.2f) // 마우스 y 위치가 일정 범위 안이면
                {
                    Controller.PreviewWall.transform.position = GameManager.GridSize * new Vector3(Mathf.Floor(touchGridPosition.x) + 0.5f, Mathf.Floor(touchGridPosition.y) + 0.5f, 0);
                    Controller.PreviewWall.transform.rotation = Quaternion.Euler(0, 0, 90);// 위치 이동 및 회전
                                                                                                     // 벽 위치 정보, 회전 정보 저장
                    _wallStartPos.x = Mathf.FloorToInt((Controller.PreviewWall.transform.position / GameManager.GridSize).x);
                    _wallStartPos.y = Mathf.FloorToInt((Controller.PreviewWall.transform.position / GameManager.GridSize).y);

                    Controller.PreviewWall.SetActive(true); // 활성화
                }
                else // 그 외일땐
                {
                    Controller.PreviewWall.SetActive(false); //비활성화
                    Controller.TouchState = TouchUtils.TouchState.Began;
                }

                if (MathUtils.GetManhattaDistance(GameManager.ToGridPosition(Controller.CurrentSelectCharacter.transform.position), GameManager.ToGridPosition(Controller.PreviewWall.transform.position - new Vector3(0.5f, 0.5f, 0) * GameManager.GridSize)) > 100)
                {
                    Controller.PreviewWall.SetActive(false);
                    return;
                }
                // Debug.Log(_wallStartPos);
            }
            else if (Controller.TouchState == TouchUtils.TouchState.Moved)
            {
                if (_wallStartPos == new Vector2(-50, -50)) return;
                Vector2 wallVector = touchGridPosition - _wallStartPos;
                float wallLength = wallVector.magnitude;
                if (wallLength >= 1)
                {
                    if (Mathf.Abs(wallVector.y / wallVector.x) < 1)
                    {
                        if (wallVector.x >= 0) // 우향
                        {
                            wallInfo[0] = Mathf.FloorToInt(_wallStartPos.x + 1);
                            wallInfo[1] = Mathf.FloorToInt(_wallStartPos.y);
                            wallInfo[2] = 1;
                        }
                        else // 좌향
                        {
                            wallInfo[0] = Mathf.FloorToInt(_wallStartPos.x - 1);
                            wallInfo[1] = Mathf.FloorToInt(_wallStartPos.y);
                            wallInfo[2] = 1;
                        }
                    }
                    else
                    {
                        if (wallVector.y >= 0) // 상향
                        {
                            wallInfo[0] = Mathf.FloorToInt(_wallStartPos.x);
                            wallInfo[1] = Mathf.FloorToInt(_wallStartPos.y + 1);
                            wallInfo[2] = 0;
                        }
                        else //하향
                        {
                            wallInfo[0] = Mathf.FloorToInt(_wallStartPos.x);
                            wallInfo[1] = Mathf.FloorToInt(_wallStartPos.y - 1);
                            wallInfo[2] = 0;
                        }
                    }
                    if (wallInfo[0] < -4 || wallInfo[0] > 3 || wallInfo[1] < -4 || wallInfo[1] > 3) // 벽 좌표가 땅 밖이라면
                    {
                        Controller.PreviewWall.SetActive(false); // 비활성화
                        return;
                    }
                    // Controller.PreviewWall.GetComponent<PreviewWall>().SetWallPreview(wallInfo[0], wallInfo[1], wallInfo[2], ref Controller.PreviewWall);
                    Controller.SetWallPreview(new Vector2Int(wallInfo[0], wallInfo[1]), wallInfo[2] == 1);
                    if (MathUtils.GetManhattaDistance(GameManager.ToGridPosition(Controller.CurrentSelectCharacter.transform.position), GameManager.ToGridPosition(Controller.PreviewWall.transform.position - new Vector3(0.5f, 0.5f, 0) * GameManager.GridSize)) > 100)
                    {
                        Controller.PreviewWall.SetActive(false);
                        return;
                    }
                }
            }
        }
    }
    #endregion
}