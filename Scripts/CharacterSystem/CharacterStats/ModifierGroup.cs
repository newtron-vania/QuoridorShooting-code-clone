using System;
using System.Collections.Generic;

public class ModifierGroup
{
    public string GroupId { get; private set; }
    public event Func<CharacterStat, bool> Condition;

    private List<ConditionalStatModifier> _modifiers;
    private bool _hasTriggered = false;
    private bool _canRepeat = false;
    private Action _onApply;
    private Action _onRemove;

    public ModifierGroup(
        string groupId,
        bool canRepeat = false,
        Action onApply = null,
        Action onRemove = null
    )
    {
        GroupId = groupId;
        _canRepeat = canRepeat;
        _onApply = onApply;
        _onRemove = onRemove;
        _modifiers = new List<ConditionalStatModifier>();
    }

    // Modifier 추가
    public void AddModifier(ConditionalStatModifier modifier)
    {
        _modifiers.Add(modifier);
    }

    // 조건 확인 및 적용 가능 여부
    public bool CanApply(CharacterStat character)
    {
        if (_hasTriggered && !_canRepeat) return false;
        if (Condition == null) return true;
        return Condition.Invoke(character);
    }

    // 그룹의 모든 Modifier 적용
    public void Apply(CharacterStat character)
    {
        foreach (var modifier in _modifiers)
        {
            // 각 Modifier의 조건도 확인
            if (modifier.CanApply(character))
            {
                character.AddModifier(modifier);
                modifier.MarkTriggered();
            }
        }

        _hasTriggered = true;
        _onApply?.Invoke();
    }

    // 그룹의 모든 Modifier 제거
    public void Remove(CharacterStat character)
    {
        foreach (var modifier in _modifiers)
        {
            character.RemoveModifier(modifier.Id);
        }

        _onRemove?.Invoke();
    }

    // 트리거 상태 초기화
    public void ResetTrigger()
    {
        _hasTriggered = false;
        foreach (var modifier in _modifiers)
        {
            modifier.ResetTrigger();
        }
    }

    public bool HasTriggered => _hasTriggered;
    public bool CanRepeat => _canRepeat;
    public IReadOnlyList<ConditionalStatModifier> Modifiers => _modifiers.AsReadOnly();
}
