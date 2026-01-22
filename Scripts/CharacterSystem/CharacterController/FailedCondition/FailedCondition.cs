public abstract class FailedCondition
{
    protected FailedCondition _nextFailedCondition;

    protected FailedCondition(FailedCondition condition)
    {
        _nextFailedCondition = condition;
    }
    
    public abstract bool IsCheck();
}