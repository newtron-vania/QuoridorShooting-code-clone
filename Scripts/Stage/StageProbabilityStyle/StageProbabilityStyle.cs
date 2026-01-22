using System.Collections;
using System.Collections.Generic;

// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
public abstract class StageProbskillStyle
{
    protected readonly int _seed;
    protected Dictionary<Stage.StageType, int> _stageProbskillDic;
    public int TotalCount()
    {
        int total = 0;
        foreach (int probskill in _stageProbskillDic.Values)
        {
            total += probskill;
        }
        return total;
    }
    protected void SetStageTypeProbskill(Stage.StageType type, int probskill)
    {
        _stageProbskillDic[type] = probskill;

    }

    public abstract Stage.StageType GetRandomStageType(int curFieldLevel);

}
