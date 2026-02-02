using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* Dictionary에 타입과 가중치를 넣어두고 뽑는 방식.
* 따라서 SetStageTypeProbability(Stage.StageType.Type, 0)은 하지 않아도 됩니다
*/
public class ChallengeStageProbability : StageProbabilityStyle
{
    private int _maxFieldLevel; //=floor
    private int _currentChapter; // 현재 챕터 (1~7)

    public ChallengeStageProbability(int maxFieldLevel, int currentChapter)
    {
        _maxFieldLevel = maxFieldLevel;
        _currentChapter = currentChapter; //GameManager에서 주는 값
        _stageProbabilityDict = new Dictionary<Stage.StageType, int>();
    }

    /// <summary>
    /// 챕터와 현재 층에 따라 확률적으로 스테이지를 반환합니다.
    /// </summary>
    public override Stage.StageType GetRandomStageType(int curFieldLevel)
    {
        _stageProbabilityDict.Clear();
        if (curFieldLevel == _maxFieldLevel)
            return Stage.StageType.Boss; //10층은 무조건 보스
        if (curFieldLevel == _maxFieldLevel - 1)
            return Stage.StageType.Rest; //보스 이전 9층은 무조건 휴식

        if (curFieldLevel == 1)
            return Stage.StageType.Normal; //1층은 무조건 일반전투
        if (_currentChapter >= 5 && curFieldLevel == 5)
            return Stage.StageType.Elite; //5 챕터부터 5층은 무조건 엘리트

        //2-3층은 챕터 상관 없이 확률 동일(엘리트와 상점 금지층)
        //(Normal: 70%, Elite: 0%, Shop: 0%, Mystery: 25%, Rest: 5%)
        if (curFieldLevel == 2 || curFieldLevel == 3)
        {
            //일반 엘리트 상점 휴식 미스터리
            SetTileProbability(normal : 700,0,0,50,250);
        }
        //4~8층은 챕터마다 확률 상이
        else 
        {
            SetTileProbabilitiesByChapter(_currentChapter);
        }

        // 설정된 가중치에 따라 랜덤 뽑기 실행
        return ReturnRandomStage();
    }

    private void SetTileProbabilitiesByChapter(int chapter)
    {
        // 챕터별 일반/엘리트 전투 확률 분기
        switch (chapter)
        {
            case 1:
                SetTileProbability(normal: 600, elite: 100, shop: 140, mystery: 110, rest: 50); break;
            case 2:
                SetTileProbability(normal: 561, elite: 139, shop: 140, mystery: 110, rest: 50); break;
            case 3:
                SetTileProbability(normal: 522, elite: 178, shop: 140, mystery: 110, rest: 50); break;
            case 4:
                SetTileProbability(normal: 483, elite: 217, shop: 140, mystery: 110, rest: 50); break;
            case 5:
                SetTileProbability(normal: 631, elite: 69, shop: 140, mystery: 110, rest: 50); break;
            case 6:
                SetTileProbability(normal: 582, elite: 118, shop: 140, mystery: 110, rest: 50); break;
            case 7:
                SetTileProbability(normal: 367, elite: 333, shop: 140, mystery: 110, rest: 50); break;
            default:
                SetTileProbability(normal: 0, elite: 700, shop: 140, mystery: 110, rest: 50); break; //예외
        }
    }

    // 절차적 맵 생성이 끝났을 때 미스터리 타일을 포함해 모든 스테이지 타입이 결정됩니다.
    // 미스터리 타일은 밟았을 때 호출되어 랜덤으로 스테이지 타입을 반환합니다.
    public override Stage.StageType GetMysteryEventType()
    {
        //엘리트, 기습, 보물, 상점, 휴식
        _stageProbabilityDict.Clear();
        /// 테스트용 수정
        SetTileProbability(ambush: 1000, treasure: 0, shop: 0, rest: 0);
        return ReturnRandomStage();

        if (_currentChapter == 1)
            SetTileProbability(ambush: 150, treasure: 250, shop: 300, rest: 300);
        else if (_currentChapter == 2 || _currentChapter == 3)
            SetTileProbability(elite: 100, ambush: 200, treasure: 200, shop: 300, rest: 200);
        else if (_currentChapter == 4 || _currentChapter == 5)
            SetTileProbability(elite: 200, ambush: 150, treasure: 200, shop: 250, rest: 200);
        else 
            SetTileProbability(elite: 300, ambush: 200, treasure: 100, shop: 200, rest: 200);
        return ReturnRandomStage();
    }

    private void SetTileProbability(int normal=0, int elite=0, int ambush=0, int treasure=0, int shop=0, int rest=0, int mystery=0)
    {
        if(normal > 0) SetStageTypeProbability(Stage.StageType.Normal, normal);
        if(elite > 0) SetStageTypeProbability(Stage.StageType.Elite, elite);
        if(ambush > 0) SetStageTypeProbability(Stage.StageType.Ambush, ambush);
        if(treasure > 0) SetStageTypeProbability(Stage.StageType.Treasure, treasure);
        if(shop > 0) SetStageTypeProbability(Stage.StageType.Shop, shop);
        if(rest > 0) SetStageTypeProbability(Stage.StageType.Rest, rest);
        if(mystery > 0) SetStageTypeProbability(Stage.StageType.Mystery, mystery);
    }

    private Stage.StageType ReturnRandomStage()
    {
        //소수점 한자리까지 사용할 때, 보통 가중치 합이 1000
        int total = GetStageTypeTotalWeight();
        int random = Random.Range(0, total);

        foreach (var probskill in _stageProbabilityDict)
        {
            if (random < probskill.Value)
                return probskill.Key;
            random -= probskill.Value;
        }

        return Stage.StageType.Rest; //예외
    }
}