using CharacterDefinition;

namespace CharacterSystem
{
    public class AttackState : State
    {
        public AttackState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override CharacterStateId StateId => CharacterStateId.Attack;

        protected override void RegisterConditions()
        {

        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            if (_agentCharacter.TargetCharacter != null) _agentCharacter.Attack();

            // 이동 가능할 시 이동 실시
            switch (_agentCharacter.CanMove)
            {
                case true:
                    switch (_agentCharacter.Class)
                    {
                        case CharacterClass.Dealer:
                            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.MoveAttack);
                            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && !_stateMachine.IsMyHpLow && !_stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.MoveAttack);
                            break;
                        default:
                            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && !_stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.MoveAttack);
                            if (_stateMachine.CanAttack && !_stateMachine.ShouldDamage) _stateMachine.ChangeState(CharacterStateId.MoveAttack);
                            break;
                    }

                    // TargetCharacter = 현재 가장 피해를 많이 줄 수 있는 캐릭터
                    _agentCharacter.SetEvadeTargetCharacter(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);
                    switch (_agentCharacter.Class)
                    {
                        case CharacterClass.Tanker:
                            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.MoveEvade);
                            if (_stateMachine.CanAttack && !_stateMachine.ShouldDamage) _stateMachine.ChangeState(CharacterStateId.MoveEvade);
                            break;
                        case CharacterClass.Supporter:
                            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.MoveEvade);
                            if (_stateMachine.CanAttack && !_stateMachine.ShouldDamage) _stateMachine.ChangeState(CharacterStateId.MoveEvade);
                            break;
                    }
                    break;
            }

            _stateMachine.ChangeState(CharacterStateId.Idle);
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}