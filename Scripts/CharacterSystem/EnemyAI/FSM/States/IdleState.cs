using CharacterDefinition;
using UnityEngine;

namespace CharacterSystem
{
    public class IdleState : State
    {

        public IdleState(CharacterStateMachine stateMachine) : base(stateMachine)
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
            // 벽 건설 상태 전환 체크
            if (_agentCharacter.CanBuild)
            {
                _agentCharacter.SetEvadeTargetCharacter(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);
                if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow && _agentCharacter.Class != CharacterClass.Dealer) _stateMachine.ChangeState(CharacterStateId.BuildWall);
                if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && !_stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.BuildWall);
                if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && !_stateMachine.IsMyHpLow && !_stateMachine.IsTargetHpLow && _agentCharacter.Class != CharacterClass.Dealer) _stateMachine.ChangeState(CharacterStateId.BuildWall);
                if (!_stateMachine.CanAttack && _stateMachine.ShouldDamage) _stateMachine.ChangeState(CharacterStateId.BuildWall);
            }

            // Skill 상태 전환 체크
            if (_stateMachine.CanUseSkill)
            {
                _stateMachine.ChangeState(CharacterStateId.UseAbillity);
            }

            // 공격 대상자가 있는지 확인
            _agentCharacter.SetAttackTargetCharacter(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);
            GameManager.Instance.BattleSystem.CellManager.SetDamageFieldGraph(_stateMachine.BattleSystem.StageCharacter[CharacterIdentification.Player]);

            // 공격 상태 전환 체크
            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.Attack);
            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && !_stateMachine.IsMyHpLow && _stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.Attack);
            if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && !_stateMachine.IsMyHpLow && !_stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.Attack);
            if (_stateMachine.CanAttack && !_stateMachine.ShouldDamage) _stateMachine.ChangeState(CharacterStateId.Attack);

            // 이동 가능할 시 이동 실시
            if (_agentCharacter.CanMove)
            {
                if (!_stateMachine.CanAttack && !_stateMachine.ShouldDamage)
                    switch (_agentCharacter.Class)
                    {
                        case CharacterClass.Tanker:
                        case CharacterClass.Supporter:
                            _stateMachine.ChangeState(CharacterStateId.MoveAttack);
                            break;
                    }

                // TargetCharacter = 현재 가장 피해를 많이 줄 수 있는 캐릭터
                _agentCharacter.SetEvadeTargetCharacter(_agentCharacter.Playerable ? CharacterIdentification.Enemy : CharacterIdentification.Player);

                if (_stateMachine.CanAttack && _stateMachine.ShouldDamage && _stateMachine.IsMyHpLow && !_stateMachine.IsTargetHpLow) _stateMachine.ChangeState(CharacterStateId.MoveEvade);
                if (!_stateMachine.CanAttack && _stateMachine.ShouldDamage) _stateMachine.ChangeState(CharacterStateId.MoveEvade);

                _stateMachine.ChangeState(CharacterStateId.MoveAttack);
            }

            _agentCharacter.ReduceAttackCount();
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}