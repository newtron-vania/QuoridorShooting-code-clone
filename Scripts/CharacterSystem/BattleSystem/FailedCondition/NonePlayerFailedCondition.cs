
    using CharacterDefinition;

    public class NonePlayerFailedCondition : FailedCondition
    {
        private BattleSystem _controller;
        public NonePlayerFailedCondition(FailedCondition condition, BattleSystem controller) : base(condition)
        {
            _controller = controller;
        }

        public override bool IsCheck()
        {
            if (_controller.StageCharacter[CharacterIdentification.Player].Count == 0)
            {
                if (_nextFailedCondition == null) return true;
                return _nextFailedCondition.IsCheck();
            }
            return false;
        }
    }