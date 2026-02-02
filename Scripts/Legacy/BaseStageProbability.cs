
using System.Collections;
using System.Collections.Generic;

// BeseStageProbability.cs와 함께 사용되는 절차적 맵생성 코드입니다.
// 여기서는 스테이지 타입 확률을 설정합니다.
// Legacy 폴더에 넣은 이유는 현재 사용되지는 않지만, 나중에 참고될 여지 때문입니다.

// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
public class BaseStageProbability : StageProbabilityStyle
{
    private int _maxFieldLevel;
    public BaseStageProbability(int maxFieldLevel, int normal, int elite, int rest, int shop, int boss)
    {
        _stageProbabilityDict = new Dictionary<Stage.StageType, int>();
        SetStageTypeProbability(Stage.StageType.Normal, normal);
        SetStageTypeProbability(Stage.StageType.Elite, elite);
        SetStageTypeProbability(Stage.StageType.Rest, rest);
        SetStageTypeProbability(Stage.StageType.Shop, shop);
        SetStageTypeProbability(Stage.StageType.Boss, boss);
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

        int random = new System.Random().Next(GetStageTypeTotalWeight());
        foreach (var prob in _stageProbabilityDict)
        {
            if (random < prob.Value)
                return prob.Key;
            random -= prob.Value;
        }
        return Stage.StageType.Normal;
    }

    //오류 때문에 일부러 넣어둔 코드.
    public override Stage.StageType GetMysteryEventType()
    {
        return Stage.StageType.Normal;
    }
}
