using CharacterDefinition;

/// <summary>
/// Duration 기반 효과를 가진 객체를 위한 인터페이스
/// DurableEffectRegistry를 통해 중앙 집중식 Duration 관리
/// </summary>
public interface IDurableEffect
{
    /// <summary>
    /// 현재 남은 턴 수 (음수면 만료)
    /// </summary>
    int Duration { get; set; }

    /// <summary>
    /// 효과의 고유 식별자
    /// </summary>
    string EffectId { get; }

    /// <summary>
    /// 효과의 소유자 (BaseCharacter)
    /// </summary>
    BaseCharacter Owner { get; }

    /// <summary>
    /// 실행 우선순위 (낮을수록 먼저 실행)
    /// JSON에서 지정, 기본값 0
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// 턴 시작 시 효과 처리
    /// DurableEffectRegistry에서 호출됨
    /// 각 효과는 자신이 턴 시작 시 어떤 동작을 할지 결정
    /// </summary>
    void ProcessTurnStart();

    /// <summary>
    /// 턴 종료 시 Duration 감소 처리
    /// DurableEffectRegistry에서 호출됨
    /// </summary>
    void ProcessTurnEnd();

    /// <summary>
    /// 효과 만료 시 호출
    /// Duration이 0 이하가 되었을 때 자동 호출됨
    /// </summary>
    void OnExpire();

    /// <summary>
    /// 효과가 활성 상태인지 확인
    /// Duration이 0 이하면 false를 반환해야 함
    /// </summary>
    bool IsActive { get; }
}
