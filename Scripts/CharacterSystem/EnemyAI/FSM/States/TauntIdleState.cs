using CharacterDefinition;

namespace CharacterSystem
{
    public class TauntIdleState : State
    {

        public TauntIdleState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
            
        }

        public override CharacterStateId StateId => CharacterStateId.Idle;

        protected override void RegisterConditions()
        {
            
        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            // 공격 대상자가 있는지 확인
            _agentCharacter.SetAttackTargetCharacter(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);
            if(_stateMachine.CanAttack) _stateMachine.ChangeState(CharacterStateId.Attack);
            
            // 이동 가능할 시 이동 실시
            if (_agentCharacter.CanMove)
            {
                _stateMachine.ChangeState(CharacterStateId.MoveAttack);
            }
            
            _agentCharacter.ReduceAttackCount();
            _agentCharacter.ReduceMoveCount();
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}