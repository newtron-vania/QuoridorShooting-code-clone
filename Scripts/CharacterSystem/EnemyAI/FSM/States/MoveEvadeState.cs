using CharacterDefinition;

namespace CharacterSystem
{
    public class MoveEvadeState : State
    {
        public MoveEvadeState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override CharacterStateId StateId => CharacterStateId.MoveEvade;

        protected override void RegisterConditions()
        {
            
        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            _agentCharacter.SetNextMovingPositionForEvade(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);
            _stateMachine.ChangeState(CharacterStateId.Move);
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}