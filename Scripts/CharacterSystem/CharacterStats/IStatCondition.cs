using System;

public interface IStatCondition
{
    bool IsConditionMet();
}

// CharacterStat 기반 조건 인터페이스
public interface ICharacterStatCondition
{
    bool IsMet(CharacterStat characterStat);
}

public class AlwaysTrueCondition : IStatCondition
{
    public bool IsConditionMet() => true;
}

public class ModifierExistsCondition : IStatCondition
{
    private StatModifierController _controller;
    private string _requiredModifierId;

    public ModifierExistsCondition(StatModifierController controller, string requiredModifierId)
    {
        _controller = controller;
        _requiredModifierId = requiredModifierId;
    }

    public bool IsConditionMet()
    {
        return _controller.HasModifier(_requiredModifierId);
    }
}

public class CustomCondition : IStatCondition
{
    private Func<bool> _condition;

    public CustomCondition(Func<bool> condition)
    {
        _condition = condition;
    }

    public bool IsConditionMet()
    {
        return _condition?.Invoke() ?? false;
    }
}
