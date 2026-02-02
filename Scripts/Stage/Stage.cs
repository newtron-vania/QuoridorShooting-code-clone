// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
// 일반전투, 엘리트전투, 휴식, 상점, 보스, 미스터리, 기습전투, 보물상자
public class Stage
{
    public enum StageType { Normal, Elite, Rest, Shop, Boss, Mystery, Raid, Treasure, Ambush }

    public StageType Type { get; private set; }

    public readonly int Id;

    public bool IsCleared { get; private set; }

    public int FieldLevel { get; private set; } // 몇 층에 있는 건지

    public Stage(StageType type, int id, int fieldLevel)
    {
        Type = type;
        Id = id;
        IsCleared = false;
        FieldLevel = fieldLevel;
    }

    public void SetCleared()
    {
        IsCleared = true;
    }
    public void MysteryChangeType(StageType newType)
    {
        Type = newType;
    }
}
