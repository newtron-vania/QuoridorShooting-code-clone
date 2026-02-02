using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class StageSelect : MonoBehaviour
{
    [SerializeField]
    private SpriteAtlas _stageSpriteAtlas;

    private Dictionary<Stage.StageType, Sprite> _stageSprites;

    [SerializeField]
    private GameObject _stagePrefab;

    //[SerializeField]
    //private GameObject _linePrefab;
    [SerializeField]
    private float _lineThickness = 0.5f;
    private StageLineDrawer _lineDrawer;

    //스테이지간의 간격
    [SerializeField]
    private int _xInterval = 10;
    [SerializeField]
    private int _yInterval = 10;
    //private float Test_Offset_Y = -5 * 10; //테스트를 위해 낮추기


    [SerializeField]
    private Transform _prefabPocket;

    //플레이어
    [Header("Player")]
    [SerializeField]
    private GameObject _playerPrefab;
    private MapPlayer _mapPlayer;

    //스테이지루트를 생성할 위치
    private Vector3 _rootPosition;

    private StageCreateStyle _stageCreate;

    [Header("Camera Setting")] //카메라 관련 설정
    private BaseCameraMove _cameraMove;

    [SerializeField] private float _cameraFollowingSpeed = 5.0f;
    [SerializeField] private Vector3 _cameraOffset = new Vector3(0, 0, -10f);
    private void Start()
    {
        //InitStageSelect(); //해당 초기화는 MapTempController에서 실행할게요
    }

    private StageComponent _currentlyHoveredStage = null;

    private void Update()
    {
        UpdateHoverState(); //마우스가 스테이지 위에 있다면 확대, hover효과
        if (_mapPlayer != null && _mapPlayer.IsMoving)
        {
            FollowPlayer();
        }
        else
        {
            _cameraMove.CameraUpdate();
            ChooseStage();
        }
    }



    ///<summary>
    ///스프라이트아틀라스에서 이미지 파일명을 이용해서 불러온 뒤 초기화합니다.
    ///</summary>
    private void InitSprite()
    {
        _lineDrawer = new StageLineDrawer(_lineThickness, _prefabPocket);
        _stageSprites = new Dictionary<Stage.StageType, Sprite>();

        _stageSprites.Add(Stage.StageType.Normal, _stageSpriteAtlas.GetSprite("Normal"));
        _stageSprites.Add(Stage.StageType.Elite, _stageSpriteAtlas.GetSprite("Elite"));
        _stageSprites.Add(Stage.StageType.Shop, _stageSpriteAtlas.GetSprite("Shop"));
        _stageSprites.Add(Stage.StageType.Rest, _stageSpriteAtlas.GetSprite("Rest"));
        _stageSprites.Add(Stage.StageType.Boss, _stageSpriteAtlas.GetSprite("Boss"));
        //TODO: 스테이지 타일에 쓰일 이미지 추가로 필요, 테스트용 로그로 일단 보완
        _stageSprites.Add(Stage.StageType.Mystery, _stageSpriteAtlas.GetSprite("Question"));
        //기습 전투는 미리 확인할 수 없음. 보스 아이콘으로 해두겠음.
        _stageSprites.Add(Stage.StageType.Ambush, _stageSpriteAtlas.GetSprite("Boss"));
        _stageSprites.Add(Stage.StageType.Treasure, _stageSpriteAtlas.GetSprite("Treasure"));
    }


    ///<summary>
    ///스테이지 선택창을 초기화합니다
    ///</summary>
    //BattleScene에서 돌아올 때마다 호출됩니다
    public void InitStageSelect()
    {
        InitSprite();

        //StageManager.Instance.InitChapterStart();
        _rootPosition = new Vector3(0, 0, 0);

        _stageCreate = new ChallengeStageCreate(GameManager.Instance.RandSeed);
        
        CreateStage(); //실제로 stage 생성 연결

        //player position
        Vector3 startPos = StageManager.Instance.CurrentPlayerPos != Vector3.zero ? StageManager.Instance.CurrentPlayerPos : new Vector3(_rootPosition.x, _rootPosition.y, 0);
        if (_mapPlayer == null)
        {
            Debug.Log($"[INFO] StageSelect::InitStageSelect() - Instantiate MapPlayer at {startPos}");
            GameObject playerObj = Instantiate(_playerPrefab, startPos, Quaternion.identity);
            playerObj.transform.SetParent(_prefabPocket);
            _mapPlayer = playerObj.GetComponent<MapPlayer>();
        }
        // 미구현된 스테이지 이동 후에도 스테이지와 간선 색상 변경을 위한 이벤트입니다.
        _mapPlayer.Temp_StageProcessCompleted -= CreateStage;
        _mapPlayer.Temp_StageProcessCompleted += CreateStage;
        // 미구현된 스테이지 문제가 전부 해소되면 변수와 함께 system도 제거해주세요.(MapPlayer.cs)
        _mapPlayer.Init(startPos);

        //setcamera - player를 바라보도록
        int curStageIdx = ConvertStageIdToIdx(StageManager.Instance.CurStageId);
        int[] pos = _stageCreate.GetStagePos(curStageIdx);
        _cameraMove = new BaseCameraMove();
        float camera_z = (15 + StageManager.Instance.CurrentChapterLevel - 1) * _yInterval / 2 + 5;
        _cameraMove.SetCamera(new Vector2(startPos.x, startPos.y), camera_z);

    }

    private int ConvertStageIdToIdx(int id)
    {
        return id - StageManager.Instance.CurrentChapterLevel * 1000;
    }

    private int ConvertStageIdxToId(int idx)
    {
        return idx + StageManager.Instance.CurrentChapterLevel * 1000;
    }

    /// <summary>
    /// 현재 스테이지를 생성하고, 각 스테이지의 위치와 상태를 초기화합니다.
    /// </summary>
    public void CreateStage()
    {
        ClearStage();
        _prefabPocket.SetAsLastSibling();

        int curStageIdx = ConvertStageIdToIdx(StageManager.Instance.CurStageId);
        
        HashSet<int> pathList = curStageIdx > 0 ? _stageCreate.GetAllStagePathSet(curStageIdx) : new HashSet<int>();
        int curFieldLevel = curStageIdx > 0 ? _stageCreate.GetStagePos(curStageIdx)[0] : 0;


        for (int idx = 1; idx <= _stageCreate.Count; idx++)
        {
            int[] stagePos = _stageCreate.GetStagePos(idx);
            GameObject stageObj = Instantiate(_stagePrefab, new Vector3(_rootPosition.x + stagePos[1] * _xInterval,
                _rootPosition.y + stagePos[0] * _yInterval, 0), Quaternion.identity);

            if (_prefabPocket != null) stageObj.transform.SetParent(_prefabPocket);

            StageComponent stageComponent = stageObj.GetComponent<StageComponent>();
            Stage stage = StageManager.Instance.GetCurChapterStage(idx, stagePos[0]);


            bool canEnter;
            bool isCurrent = (idx == curStageIdx);

            if (curStageIdx == 0) //처음 맵에 진입했을 때
            {
                canEnter = stage.FieldLevel == 1;
            }
            else
            {
                canEnter = pathList.Contains(idx) && (stage.FieldLevel == curFieldLevel + 1);
            }

            DrawLine(idx, canEnter, isCurrent);
            stageComponent.Init(stage, _stageSprites[stage.Type], canEnter, isCurrent);
        }
    }

    /// <summary>
    /// 부모 스테이지와 자식 스테이지 간의 선을 그립니다.
    /// </summary>
    public void DrawLine(int parentIdx, bool canEnter, bool isNextPath)
    {
        List<int> pathList = _stageCreate.GetStagePathList(parentIdx);
        int[] parentPos = _stageCreate.GetStagePos(parentIdx);
        Vector3 startPos = new Vector3(_rootPosition.x + parentPos[1] * _xInterval,
            _rootPosition.y + parentPos[0] * _yInterval, 0);

        foreach (int childIdx in pathList)
        {
            int[] childPos = _stageCreate.GetStagePos(childIdx);
            Vector3 endPos = new Vector3(_rootPosition.x + childPos[1] * _xInterval,
                _rootPosition.y + childPos[0] * _yInterval, 0);

            _lineDrawer.DrawSegment(startPos, endPos, isNextPath);
        }
    }

    private void ChooseStage()
    {
        if (_cameraMove.TouchState == Legacy.TouchUtil.ETouchState.Ended)
        {
            Vector2 pos = _cameraMove.MovingCamera.ScreenToWorldPoint(_cameraMove.TouchPosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.forward, 100f, LayerMask.GetMask("Stage"));
            if (hit)
            {
                Debug.Log($"[INFO] StageSelect::hit stage at position {hit.transform.position}");
                StageComponent stageComponent = hit.collider.GetComponent<StageComponent>();

                Stage clickedStage = stageComponent.GetStage();
                int clickedIdx = ConvertStageIdToIdx(clickedStage.Id);
                int currentIdx = ConvertStageIdToIdx(StageManager.Instance.CurStageId);

                if (CanMoveStage(clickedStage, clickedIdx, currentIdx))
                {
                    Debug.Log($"[INFO] StageSelect::ChooseStage() - {currentIdx} -> {clickedIdx}");
                    StageManager.Instance.CurStageId = clickedStage.Id;
                    _mapPlayer.MovePlayerToNextStage(hit.transform.position);
                    // StageManager.Instance.CompleteStage(clickedStage.Id);
                    //현재는 경로를 다 그리는 상태인데, 갈 수 있는 길만 따로 색깔로 입히는 경우를 고려할 것
                    //(그럴 경우에 TODO : 이동한 이후에 다시 갈 수 있는 길을 그려야만 합니다.)
                }
                else
                {
                    Debug.Log($"[INFO] StageSelect::ChooseStage() - can`t move to stage, wrong path");
                }

                stageComponent.OnTouchEvent();
            }
        }
    }

    private void FollowPlayer()
    {
        if (_cameraMove == null || _cameraMove.MovingCamera == null) return;
        Transform camTransform = _cameraMove.MovingCamera.transform;
        Vector3 targetPos = _mapPlayer.transform.position + _cameraOffset;
        targetPos.z = -10f; 
        camTransform.position = Vector3.Lerp(camTransform.position, targetPos, Time.deltaTime * _cameraFollowingSpeed);
    }

    bool CanMoveStage(Stage clickedStage, int clickedIdx, int currentIdx)
    {
        if (clickedIdx == currentIdx)
        {
            Debug.Log($"[INFO] CanMoveStage: Failed - Target({clickedIdx}) is same as Current({currentIdx})");
            return false;
        }

        int currentStageId = ConvertStageIdxToId(currentIdx);
        if (StageManager.Instance.StageDic.ContainsKey(currentStageId))
        {
            if (StageManager.Instance.StageDic[currentStageId].IsCleared == false)
            {
                Debug.Log($"[INFO] CanMoveStage: Failed - Current Stage({currentStageId}) is not Cleared.");
                return false;
            }
        }
        
        bool canMove = false;
        if (clickedStage.FieldLevel == 1)
        {
            // 만약 이미 2층에 있는데 1층 누르면 안됨
            int curFloor = 0;
            if (currentIdx > 0 && currentIdx <= _stageCreate.Count)
                curFloor = _stageCreate.GetStagePos(currentIdx)[0];

            if (curFloor == 0) 
            {
                canMove = true;
            }
            else
            {
                Debug.Log($"[INFO] CanMoveStage: Failed - Cannot move to Floor 1 from Floor {curFloor}");
            }
        }
        else
        {
            // 현재 위치에서 갈 수 있는 경로 리스트 가져오기
            List<int> nextPaths = _stageCreate.GetStagePathList(currentIdx);
            if (nextPaths.Contains(clickedIdx))
            {
                canMove = true;
            }
            else
            {
                Debug.Log($"[INFO] CanMoveStage: Failed - Path not found from {currentIdx} to {clickedIdx}. Paths: {string.Join(",", nextPaths)}");
            }
        }
        return canMove;
    }

    //hover효과 구현
    private void UpdateHoverState()
    {
        if (_cameraMove == null || _cameraMove.MovingCamera == null) return;
        Vector2 worldPos = _cameraMove.MovingCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f, LayerMask.GetMask("Stage"));
        if (hit)
        {
            StageComponent hitStage = hit.collider.GetComponent<StageComponent>();
            if (hitStage != null && hitStage != _currentlyHoveredStage)
            {
                if (_currentlyHoveredStage != null)
                {
                    _currentlyHoveredStage.OnHoverExit();
                }

                _currentlyHoveredStage = hitStage;
                _currentlyHoveredStage.OnHoverEnter();
            }
        }
        else
        {
            if (_currentlyHoveredStage != null)
            {
                _currentlyHoveredStage.OnHoverExit();
                _currentlyHoveredStage = null;
            }
        }
    }

    public void ClearStage()
    {
        // _prefabPocket 밑에 생성된 모든 자식 오브젝트 삭제
        if (_prefabPocket != null)
        {
            foreach (Transform child in _prefabPocket)
            {
                if (_mapPlayer != null && child.gameObject == _mapPlayer.gameObject)
                    continue;
                Destroy(child.gameObject);
            }
        }
    }

}