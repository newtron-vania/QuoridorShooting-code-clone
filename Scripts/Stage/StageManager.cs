
using System.Collections.Generic;
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
public class StageManager : MonoBehaviour
{
    //클리어된 마지막 스테이지의 Id
    public int CurStageId { get; private set; }
    public int CurChapterLevel { get; private set; }

    private StageProbskillStyle MainProbskillStyle;

    public Dictionary<int, Stage> StageDic;

    //ToDo: 싱글턴이 되거나 monobehaviour를 제거하게 되면 수정해주세요. 
    private void Awake()
    {
        CurChapterLevel = 1;
        CurStageId = CurChapterLevel * 1000;

        InitChapterStart();
    }

    //새로운 챕터가 실행될 때 실행해주세요.
    public void InitChapterStart()
    {
        StageDic = new Dictionary<int, Stage>();
        StageDic.Clear();

        MainProbskillStyle = new BaseStageProbskill(15 + CurChapterLevel, 10, 10, 2, 10, 0);

    }


    ///<summary>
    ///해당 스테이지 불러오기, 만약 없다면 생성해서 반환합니다.
    ///스테이지 Id 는 현재 챕터 + 스테이지 생성시의 idx를 조합하여 생성
    ///</summary>
    public Stage GetCurChapterStage(int idx, int fieldLevel)
    {
        int stageId = idx + CurChapterLevel * 1000;
        if (StageDic.ContainsKey(stageId))
            return StageDic[stageId];

        Stage.StageType type = MainProbskillStyle.GetRandomStageType(fieldLevel);
        Stage stage = new Stage(type, stageId);
        StageDic.Add(stage.Id, stage);

        return stage;

    }


    ///<summary>
    ///스테이지 완료시 업데이트 되야할 정보를 업데이트합니다.
    ///</summary>
    public void CompleteStage(int clearedStageId)
    {
        CurStageId = clearedStageId;
        StageDic[CurStageId].Clear();
    }


}
