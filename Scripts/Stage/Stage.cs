
// 게임로직코드입니다 
// 엔진코드는 가급적 삼가해주세요.
public class Stage
{

    public enum StageType { Normal, Elite, Rest, Shop, Boss }

    public StageType Type { get; private set; }

    public readonly int Id;

    public bool IsCleared { get; private set; }

    public Stage(StageType type, int id)
    {
        Type = type;
        Id = id;
        IsCleared = false;
    }

    public void Clear()
    {
        IsCleared = true;
    }

}
