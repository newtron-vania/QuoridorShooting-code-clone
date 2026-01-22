using HM;


/*
 * 게임 턴 관리용 스크립트 입니다.
 * 
 * ToDo
 * OnRoundStart가 필요한지 확인하기
 */
public partial class GameManager 
{
    public int Turn { get; private set; } = 1; // 현재 턴 // 새 스테이지면 리셋 해야
    public void EndTurn()
    {
        EventManager.Instance.InvokeEvent(HM.EventType.OnTurnEnd,this);

        if(Turn%2==0)
        {
            EndRound();
        }

        Turn++;

        StartTurn();
    }

    private void StartTurn()
    {
        EventManager.Instance.InvokeEvent(HM.EventType.OnTurnStart, this);
    }

    private void EndRound()
    {
        EventManager.Instance.InvokeEvent(HM.EventType.OnRoundEnd, this);
    }
}
