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

    [SerializeField]
    private GameObject _linePrefab;

    //스테이지간의 간격
    [SerializeField]
    private int _xInterval = 10;
    [SerializeField]
    private int _yInterval = 10;

    //Todo: 스테이지매니저를 싱글턴화 할 시 제거 가능합니다.
    [SerializeField]
    private StageManager _stageManager;

    [SerializeField]
    private Transform _prefabPocket;

    //스테이지루트를 생성할 위치
    private Vector3 _rootPosition;

    private StageCreateStyle _stageCreate;

    private BaseCameraMove _cameraMove;
    private void Start()
    {
        InitStageSelect();

    }

    private void Update()
    {
        _cameraMove.CameraUpdate();
        ChooseStage();
    }


    ///<summary>
    ///스프라이트아틀라스에서 이미지 파일명을 이용해서 불러온 뒤 초기화합니다.
    ///</summary>
    private void InitSprite()
    {
        _stageSprites = new Dictionary<Stage.StageType, Sprite>();

        _stageSprites.Add(Stage.StageType.Normal, _stageSpriteAtlas.GetSprite("Normal"));
        _stageSprites.Add(Stage.StageType.Elite, _stageSpriteAtlas.GetSprite("Elite"));
        _stageSprites.Add(Stage.StageType.Shop, _stageSpriteAtlas.GetSprite("Shop"));
        _stageSprites.Add(Stage.StageType.Rest, _stageSpriteAtlas.GetSprite("Rest"));
        _stageSprites.Add(Stage.StageType.Boss, _stageSpriteAtlas.GetSprite("Boss"));
    }


    ///<summary>
    ///스테이지 선택창을 초기화합니다
    ///</summary>
    //루트 포지션은 상황에 맞춰 커스텀 가능하게 만들어 줘도 될 것 같슴다
    public void InitStageSelect()
    {
        InitSprite();


        _rootPosition = new Vector3(0, 0, 0);
        _stageCreate = new BaseStageCreate(15 + _stageManager.CurChapterLevel, 3);
        CreateStage();


        //setcamera
        int curStageIdx = ConvertStageIdToIdx(_stageManager.CurStageId);
        int[] pos = _stageCreate.GetStagePos(curStageIdx);
        _cameraMove = new BaseCameraMove();


        _cameraMove.SetCamera(new Vector2(_rootPosition.x + pos[1] * _xInterval, _rootPosition.y + (pos[0] + 0.5f) * _yInterval), (15 + _stageManager.CurChapterLevel - 1) * _yInterval / 2 + 5);

    }

    //스테이지아이디를 인덱스로 일일이 바꾸기 귀찮습니다.
    private int ConvertStageIdToIdx(int id)
    {
        return id - _stageManager.CurChapterLevel * 1000;
    }


    /// <summary>
    /// 현재 스테이지를 생성하고, 각 스테이지의 위치와 상태를 초기화합니다.
    /// </summary>
    public void CreateStage()
    {
        _prefabPocket.SetAsLastSibling();

        int curStageIdx = ConvertStageIdToIdx(_stageManager.CurStageId);
        HashSet<int> pathList = _stageCreate.GetAllStagePathSet(curStageIdx);

        int curFieldLevel = _stageCreate.GetStagePos(curStageIdx)[0];

        for (int idx = 1; idx <= _stageCreate.Count; idx++)
        {
            int[] stagePos = _stageCreate.GetStagePos(idx);
            GameObject stageObj = Instantiate(_stagePrefab, new Vector3(_rootPosition.x + stagePos[1] * _xInterval, _rootPosition.y + stagePos[0] * _yInterval, 0), Quaternion.identity);


            StageComponent stageComponent = stageObj.GetComponent<StageComponent>();
            Stage stage = _stageManager.GetCurChapterStage(idx, stagePos[0]);


            bool canEnter = pathList.Contains(idx);

            //갈 수 있는길만 그려줍니다.
            if (canEnter || stage.IsCleared)
                DrawLine(idx, canEnter);

            //지금 당장 갈 수 있는지 확인합니다.
            if (stagePos[0] != curFieldLevel + 1)
                canEnter = false;

            stageComponent.Init(stage, _stageSprites[stage.Type], canEnter);

        }

    }

    /// <summary>
    /// 부모 스테이지와 자식 스테이지 간의 선을 그립니다.
    /// </summary>
    public void DrawLine(int parentIdx, bool canEnter)
    {
        List<int> pathList = _stageCreate.GetStagePathList(parentIdx);
        int[] parentPos = _stageCreate.GetStagePos(parentIdx);
        foreach (int childIdx in pathList)
        {
            int[] childPos = _stageCreate.GetStagePos(childIdx);
            Vector3 pos = new Vector3(parentPos[1] * _xInterval, parentPos[0] * _yInterval, 0);
            int diff = childPos[1] - parentPos[1];
            GameObject lineObj = Instantiate(_linePrefab, pos, Quaternion.identity);
            float angle = diff * _stageCreate.ChildAngle;
            lineObj.transform.Rotate(Vector3.forward, angle);
            lineObj.transform.localScale = new Vector3(1, _yInterval / MathF.Cos(angle * Mathf.Deg2Rad), 1);
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
                StageComponent stageComponent = hit.collider.GetComponent<StageComponent>();
                stageComponent.OnTouchEvent();
            }
        }
    }


}
