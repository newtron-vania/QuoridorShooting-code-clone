using System.Collections;
using System.Collections.Generic;

// 새로운 맵 유형을 만들고 싶다면 해당 클래스를 상속하세요.

// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
public abstract class StageProbabilityStyle
{
    protected readonly int _seed;
    protected Dictionary<Stage.StageType, int> _stageProbabilityDict;
    public int GetStageTypeTotalWeight()
    {
        int total = 0;
        foreach (int weight in _stageProbabilityDict.Values) total += weight;
        return total;
    } //GetEnemyCountTotalWeight
    protected void SetStageTypeProbability(Stage.StageType type, int weight)
    {
        _stageProbabilityDict[type] = weight;
    }

    public abstract Stage.StageType GetRandomStageType(int curFieldLevel);

    public abstract Stage.StageType GetMysteryEventType();

}
