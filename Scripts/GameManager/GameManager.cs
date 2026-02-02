using System;
using System.Collections;
using System.Collections.Generic;
using HM.Containers;
using HM.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class GameManager : MonoBehaviour
{
    // GameManager 임시 작성...(회륜)
    private float _pointTime = 1.0f; //1초마다 실행
    private float _nextTime = 0.0f; //다음번 실행할 시간
    // GameManager 수정 얼마든지 가능

    public readonly int MinGridx = -4;
    public readonly int MinGridy = -4;
    public readonly int MaxGridx = 4;
    public readonly int MaxGridy = 4;
    public const float GridSize = 1.3f;
    //TODO: EnemyValues를 나중에 바뀔 CharacterStatus 같은 새로운 구초체가 나올 경우 변경 필요.

    public static int Stage = 1; // 현재 스테이지 (UI/UX 정회륜 구현 때 사용 위치 변경 or 변수명 문제 시 알려주세요)
    public int RandSeed = -1;

    public List<SavedPlayerCharacterData> PlayerList = new List<SavedPlayerCharacterData>();
    public List<WallData> WallList = new List<WallData>();
    public int playerWallCount = 10;
    public int playerMaxBuildWallCount = 10;
    public int playerDestroyedWallCount = 0;
    public int playerMaxDestroyWallCount = 0;

    public LogManager CrashHandler { get; private set; } = new LogManager();

    private BattleSystem _controller;
    public BattleSystem BattleSystem
    {
        get
        {
            if (_controller == null)
            {
                _controller = FindObjectOfType<BattleSystem>().GetComponent<BattleSystem>();
            }

            return _controller;
        }

        set
        {
            _controller = value;
        }
    }


    public int playerCount = 3;
    public int enemyCount = 3;
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(GameManager));
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }
    public static void Reset()
    {
        if (_instance != null)
        {
            Destroy(_instance.gameObject);
            _instance = null;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif

        // 테스트를 위한 랜덤 시드 설정
        if (RandSeed == -1)
        {
            RandSeed = UnityEngine.Random.Range(0, int.MaxValue);
            LogManager.Instance.Init();
            //절차적 맵생성을 위한 MapTempController의 Regenerate버튼이 러닝타임 중에 시드를 수정합니다!
            // RandSeed = 4433;
            Debug.Log($"[INFO] GameManager::Awake() - RandSeed: {RandSeed}");
        }
    }
    List<Vector2Int> _clickVector = new List<Vector2Int>();
    TouchUtils.TouchState _touchState = TouchUtils.TouchState.None;
    Vector2 _touchPosition = Vector2.zero;
    private void Start()
    {
        //  벽 데이터 배치 오류
        // foreach (var VARIABLE in GameObject.FindGameObjectsWithTag("Wall"))
        // {
        //     var wall = new WallData(VARIABLE.transform.position, VARIABLE.transform.rotation.z != 0);
        //     WallList.Add(wall);
        //     Debug.Log($"Wallposition : {wall.Position}");
        // }
        // SetWall();

        //SetWall();

        SetUIForScene();


    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "BattleScene")
        {
            PlayerList = new List<SavedPlayerCharacterData>();
            WallList = new List<WallData>();
            playerWallCount = 0;
            playerMaxBuildWallCount = 10;
            playerDestroyedWallCount = 0;
            playerMaxDestroyWallCount = 0;
            SetUIForScene();
        }
    }
    private void Update()
    {
        //TouchUtils.TouchSetUp(ref _touchState, ref _touchPosition);

        //// 입력 좌표 확인 (디버깅용)
        //if (_touchState == TouchUtils.TouchState.Began)
        //{
        //    Vector2 touchPosition = Camera.main.ScreenToWorldPoint(_touchPosition);
        //    Debug.Log($"[INFO] GameManager::Update() - 클릭한 좌표: {ToGridPosition(touchPosition)}");
        //    _clickVector.Add(ToGridPosition(touchPosition));
        //    if (_clickVector.Count == 2)
        //    {
        //        List<Vector2Int> pathList = PathFindingUtils.FindPath(_clickVector[0], _clickVector[1]);
        //        Debug.Log($"[INFO] GameManager::Update() - 계산된 길찾기 경로: {string.Join(", ", pathList)}");
        //        _clickVector.Clear();
        //    }
        //}

        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            if (Time.time > _nextTime)
            {
                _nextTime = Time.time + _pointTime; //다음번 실행할 시간

                if (UIManager.Instance.LogQueue.Count != 0)
                {
                    UIManager.Instance.FindUI<ChallengeBattleUI>().ShowLogText(UIManager.Instance.LogQueue.Dequeue());
                }
            }
        }
    }

    // 씬에 해당하는 고정 UI 생성
    private void SetUIForScene()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            // 임시 씬 이름에 맞게 변경할 예정
            case "BattleScene":
                UIManager.Instance.ShowSceneUI<ChallengeBattleUI>();
                break;
            default:
                Debug.LogWarning("[WARNING] GameManager::SettingUIForScene() - 해당 씬과 일치하는 UI는 존재하지 않습니다.");
                break;
        }
    }

    // 좌표 변환 함수
    public static Vector3 ToWorldPosition(Vector2 gridPosition)
    {
        return new Vector3(gridPosition.x * GridSize, gridPosition.y * GridSize, 0);
    }
    public static Vector2Int ToGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x / GridSize), Mathf.RoundToInt(worldPosition.y / GridSize));
    }

    // 전체 범위 반환(플레이어 위치 기준)
    public List<Vector2Int> GetFullRange(Vector2Int playerPosition)
    {
        List<Vector2Int> ranges = new List<Vector2Int>();

        int xMax = 8 - playerPosition.x;
        int yMax = 8 - playerPosition.y;
        int xMin = xMax - 8;
        int yMin = yMax - 8;

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                // (0,0 은 제외)
                if (x == 0 && y == 0)
                    continue;

                ranges.Add(new Vector2Int(x, y));
            }
        }

        return ranges;
    }
}