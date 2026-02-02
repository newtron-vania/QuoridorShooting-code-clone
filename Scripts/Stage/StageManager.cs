
using System.Collections.Generic;
using HM;
using Unity.VisualScripting;
using UnityEngine;


/*
 * 씬이 전환되도 사라지면 안되는 정보들을 다루는 클래스입니다.
 * 
 * ToDo: 
 * 프로젝트에서 보편적으로 쓰일 싱글턴 슈퍼클래스가 생기면 수정하시면 됩니다. 
 * 만약 수정되면 Awake도 같이 수정해주세용
 * 
 * 테스트를 위해 monobehaviour를 상속시켜 자체적으로 동작할 수 있게 만들었지만
 * 엔진 코드를 사용하지 않아도 될 것 같아 수정 후 게임로직으로써 사용되면 좋을 것 같습니다.
 */
public class StageManager : Singleton<StageManager>, IEventListener
{
    private int _currentChapterLevel = 1;

    //클리어된 마지막 스테이지의 Id
    public int CurStageId { get; set; }
    public int CurrentChapterLevel
    {
        get => _currentChapterLevel;
        set
        {
            _currentChapterLevel = value;
            CurStageId = _currentChapterLevel * 1000;
        }
    } //테스트용으로 일단 public으로 풀어둠

    private StageProbabilityStyle _mainProbabilityStyle;
    //private ChallengeStageProbability ChallengeProbabilityStyle;

    public Dictionary<int, Stage> StageDic;

    //For Temporal Release
    public Vector3 CurrentPlayerPos = Vector3.zero;

    //도전모드에서 층마다 독립적으로 생성하지 않는다면 반영할 변수
    //private const int MAX_SHOP_PER_FLOOR = 2;
    //private const int MAX_ELITE_PER_FLOOR = 2;
    //private const int MAX_REST_PER_FLOOR = 2;

    public override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            Debug.Log($"[INFO] StageManager::Awake - Duplicate detected. Destroying self. InstanceID: {GetInstanceID()}");
            return;
        }

        CurrentChapterLevel = 1;
        CurStageId = CurrentChapterLevel * 1000;
        if (StageDic == null) StageDic = new Dictionary<int, Stage>();
        else StageDic.Clear();

        InitChapterStart();
    }

    private void OnDestroy()
    {
        Debug.Log($"[INFO] StageManager::OnDestroy - InstanceID: {GetInstanceID()}");
    }

    private void Start()
    {
        EventManager.Instance.AddEvent(HM.EventType.OnGameFinish, this);
    }

    public void OnEvent(HM.EventType eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case HM.EventType.OnGameFinish:
                Debug.Log("[INFO] StageManager::OnEvent - Cleared Stage " + CurStageId);
                CompleteStage(CurStageId);
                break;
            default:
                break;
        }
    }

    //새로운 챕터가 실행될 때 실행해주세요.
    public void InitChapterStart()
    {
        // CurStageId = CurrentChapterLevel * 1000;
        // StageDic = new Dictionary<int, Stage>();
        // StageDic.Clear();
        if (StageDic == null)
            StageDic = new Dictionary<int, Stage>();
        else
            StageDic.Clear();
        //MainProbabilityStyle = new BaseStageProbability(15 + CurrentChapterLevel, 10, 10, 2, 10, 0);
        //TODO: 기존 BaseStage를 대체하는지 선택하는지 결정
        _mainProbabilityStyle = new ChallengeStageProbability(10, CurrentChapterLevel);
    }


    ///<summary>
    ///해당 스테이지 불러오기, 만약 없다면 생성해서 반환합니다.
    ///스테이지 Id 는 현재 챕터 + 스테이지 생성시의 idx를 조합하여 생성
    ///</summary>
    public Stage GetCurChapterStage(int idx, int fieldLevel)
    {
        int stageId = idx + CurrentChapterLevel * 1000;
        if (StageDic.ContainsKey(stageId))
            return StageDic[stageId];

        /*
         ToDo: 현재 층에서의 스테이지 타입 제한 로직을 추가할 수 있습니다
         */

        Stage.StageType type = _mainProbabilityStyle.GetRandomStageType(fieldLevel);
        Stage stage = new Stage(type, stageId, fieldLevel);
        StageDic.Add(stage.Id, stage);

        return stage;
    }


    ///<summary>
    ///스테이지 완료시 업데이트 되야할 정보를 업데이트합니다.
    ///</summary>
    public void CompleteStage(int clearedStageId)
    {
        // CurStageId = clearedStageId;
        StageDic[clearedStageId].SetCleared();
    }

    //도전모드에서 미스터리 타입의 타일을 밟았을 때 호출해서 타입을 다시 결정
    public Stage.StageType GetMysteryEventType()
    {
        return _mainProbabilityStyle.GetMysteryEventType();
    }
}
