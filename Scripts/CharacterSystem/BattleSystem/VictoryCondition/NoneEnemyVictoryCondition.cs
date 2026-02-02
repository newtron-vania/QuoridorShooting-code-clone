
    using CharacterDefinition;

    public class NoneEnemyVictoryCondition : VictoryCondition
    {
        private BattleSystem _controller;
        
        public NoneEnemyVictoryCondition(VictoryCondition condition, BattleSystem controller) : base(condition)
        {
            _controller = controller;
        }

        public override bool IsCheck()
        {
            if (_controller.StageCharacter[CharacterIdentification.Enemy].Count == 0)
            {
                if (_nextVictoryCondition == null) return true;
                return _nextVictoryCondition.IsCheck();
            }
            return false;
        }
    }