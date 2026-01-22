using CharacterDefinition;

namespace CharacterSystem
{
    public class MoveAttackState : State
    {
        public MoveAttackState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override CharacterStateId StateId => CharacterStateId.MoveAttack;

        protected override void RegisterConditions()
        {
            
        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            _agentCharacter.SetNextMovingPositionForAttack(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);
            _stateMachine.ChangeState(CharacterStateId.Move);
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}