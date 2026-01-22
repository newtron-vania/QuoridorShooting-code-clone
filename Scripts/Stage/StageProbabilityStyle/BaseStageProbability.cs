
using System.Collections;
using System.Collections.Generic;

// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
public class BaseStageProbskill : StageProbskillStyle
{
    private int _maxFieldLevel;
    public BaseStageProbskill(int maxFieldLevel, int normal, int elite, int rest, int shop, int boss)
    {
        _stageProbskillDic = new Dictionary<Stage.StageType, int>();
        SetStageTypeProbskill(Stage.StageType.Normal, normal);
        SetStageTypeProbskill(Stage.StageType.Elite, elite);
        SetStageTypeProbskill(Stage.StageType.Rest, rest);
        SetStageTypeProbskill(Stage.StageType.Shop, shop);
        SetStageTypeProbskill(Stage.StageType.Boss, boss);
        _maxFieldLevel = maxFieldLevel;
    }
    /// <summary>
    /// 마지막과 그 전 스테이지를 제외하고 확률적으로 스테이지를 반환합니다.
    /// </summary>
    public override Stage.StageType GetRandomStageType(int curFieldLevel)
    {
        if (curFieldLevel == _maxFieldLevel)
            return Stage.StageType.Boss;
        else if (curFieldLevel == _maxFieldLevel - 1)
            return Stage.StageType.Rest;

        int random = new System.Random().Next(TotalCount());
        foreach (var probskill in _stageProbskillDic)
        {
            if (random < probskill.Value)
                return probskill.Key;
            random -= probskill.Value;
        }
        return Stage.StageType.Normal;
    }
}
