using System;

public class ConditionalStatModifier : StatModifier
{
    public event Func<CharacterStat, bool> Condition;
    private bool _hasTriggered = false;
    private bool _canRepeat = false;

    public ConditionalStatModifier(
        string id,
        StatType targetStat,
        StatModifierType modifierType,
        float value,
        int priority = 0,
        bool canRepeat = false,
        Action onRemove = null
    ) : base(id, targetStat, modifierType, value, priority, onRemove)
    {
        _canRepeat = canRepeat;
    }

    // 조건 확인 및 적용 가능 여부 반환
    public bool CanApply(CharacterStat character)
    {
        if (_hasTriggered && !_canRepeat) return false;
        if (Condition == null) return true;
        return Condition.Invoke(character);
    }

    // 트리거 상태 마킹
    public void MarkTriggered()
    {
        _hasTriggered = true;
    }

    // 트리거 상태 초기화
    public void ResetTrigger()
    {
        _hasTriggered = false;
    }

    public bool HasTriggered => _hasTriggered;
    public bool CanRepeat => _canRepeat;
}
