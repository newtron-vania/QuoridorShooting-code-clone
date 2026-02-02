/// <summary>
/// 턴 이벤트를 처리하는 핸들러 인터페이스
/// BattleTurnController에 등록하여 TurnStart/TurnEnd 이벤트를 수신할 수 있음
/// </summary>
public interface ITurnEventHandler
{
    /// <summary>
    /// 핸들러의 고유 이름 (디버깅 및 로깅용)
    /// </summary>
    string HandlerName { get; }

    /// <summary>
    /// 턴 시작 시 호출됨
    /// BattleTurnController에 등록된 우선순위 순서대로 실행
    /// </summary>
    void OnTurnStart();

    /// <summary>
    /// 턴 종료 시 호출됨
    /// BattleTurnController에 등록된 우선순위 순서대로 실행
    /// </summary>
    void OnTurnEnd();
}
