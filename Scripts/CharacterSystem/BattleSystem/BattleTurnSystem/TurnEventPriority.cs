/// <summary>
/// 턴 이벤트 핸들러의 실행 우선순위 정의
/// 낮은 값일수록 먼저 실행됨 (오름차순)
///
/// 사용법:
/// turnController.RegisterTurnEndEvent(handler, TurnEventPriority.TURN_END_STAT_DURATION);
/// </summary>
public static class TurnEventPriority
{
    // ==================== TurnStart 우선순위 ====================

    /// <summary>
    /// 캐릭터 상태 초기화 (가장 먼저 실행)
    /// - 캐릭터 행동 가능 상태 리셋
    /// - 턴 시작 플래그 설정
    /// </summary>
    public const int TURN_START_CHARACTER_RESET = 0;

    /// <summary>
    /// 스탯 업데이트 및 회복
    /// - AP(행동력) 회복
    /// - 턴 시작 시 스탯 증가 효과 적용
    /// </summary>
    public const int TURN_START_STAT_UPDATE = 10;

    /// <summary>
    /// 셀 효과 적용
    /// - 셀에 있는 캐릭터에게 효과 적용
    /// - 특수 지형 효과 처리
    /// </summary>
    public const int TURN_START_CELL_EFFECT = 20;

    /// <summary>
    /// 지속 효과 처리 (SkillInstance, StatuseffectInstance)
    /// - 턴 시작 시 발동하는 지속 효과 처리
    /// - 독, 회복 등의 지속 데미지/힐링
    /// </summary>
    public const int TURN_START_DURABLE_EFFECT = 30;

    /// <summary>
    /// 상태이상 효과 처리
    /// - 상태이상에 의한 추가 효과 발동
    /// - 상태이상 아이콘 업데이트
    /// </summary>
    public const int TURN_START_STATUSEFFECT = 40;

    // ==================== TurnEnd 우선순위 ====================

    /// <summary>
    /// 커맨드 큐 정리 (가장 먼저 실행)
    /// - 실행된 커맨드 정리
    /// - 턴 종료 플래그 설정
    /// </summary>
    public const int TURN_END_COMMAND_CLEAR = 0;

    /// <summary>
    /// 스탯 Duration 감소 (StatManager)
    /// - 턴 기반 스탯 modifier Duration 감소
    /// - 만료된 스탯 효과 제거
    ///
    /// 우선순위 이유: 지속 효과가 스탯 변경을 참조하므로 먼저 처리
    /// </summary>
    public const int TURN_END_STAT_DURATION = 10;

    /// <summary>
    /// 지속 효과 Duration 감소 (DurableEffectRegistry)
    /// - SkillInstance Duration 감소
    /// - StatuseffectInstance Duration 감소
    /// - 만료된 효과 제거 및 OnExpire() 호출
    ///
    /// 우선순위 이유: CellManager가 스킬 효과를 참조하므로 먼저 처리
    /// </summary>
    public const int TURN_END_DURABLE_EFFECT = 20;

    /// <summary>
    /// 셀 Duration 감소 (CellManager)
    /// - 셀 상태 Duration 감소
    /// - 만료된 셀 상태 제거
    ///
    /// 우선순위 이유: 캐릭터 정리가 셀 정보를 참조할 수 있으므로 먼저 처리
    /// </summary>
    public const int TURN_END_CELL_DURATION = 30;

    /// <summary>
    /// 캐릭터 정리 작업 (가장 마지막 실행)
    /// - 캐릭터 턴 종료 처리
    /// - 사망 캐릭터 정리
    /// - 턴 종료 상태 업데이트
    /// </summary>
    public const int TURN_END_CHARACTER_CLEANUP = 40;

    // ==================== 커스텀 우선순위 범위 ====================

    /// <summary>
    /// 사용자 정의 낮은 우선순위 범위 (TurnStart: 50-99)
    /// 기본 시스템보다 나중에 실행되어야 하는 커스텀 핸들러용
    /// </summary>
    public const int TURN_START_CUSTOM_LOW = 50;

    /// <summary>
    /// 사용자 정의 낮은 우선순위 범위 (TurnEnd: 50-99)
    /// 기본 시스템보다 나중에 실행되어야 하는 커스텀 핸들러용
    /// </summary>
    public const int TURN_END_CUSTOM_LOW = 50;

    /// <summary>
    /// 사용자 정의 높은 우선순위 범위 (TurnStart: -10 ~ -1)
    /// 기본 시스템보다 먼저 실행되어야 하는 커스텀 핸들러용
    /// </summary>
    public const int TURN_START_CUSTOM_HIGH = -10;

    /// <summary>
    /// 사용자 정의 높은 우선순위 범위 (TurnEnd: -10 ~ -1)
    /// 기본 시스템보다 먼저 실행되어야 하는 커스텀 핸들러용
    /// </summary>
    public const int TURN_END_CUSTOM_HIGH = -10;
}
