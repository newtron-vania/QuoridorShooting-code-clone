public abstract class VictoryCondition
{
    protected VictoryCondition _nextVictoryCondition;

    protected VictoryCondition(VictoryCondition condition)
    {
        _nextVictoryCondition = condition;
    }
    
    public abstract bool IsCheck();
}