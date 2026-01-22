namespace CharacterSystem
{
    public class MoveState : State
    {
        public MoveState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override CharacterStateId StateId => CharacterStateId.Move;

        protected override void RegisterConditions()
        {
            
        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            _agentCharacter.Move();
            _stateMachine.ChangeState(CharacterStateId.Idle);
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}