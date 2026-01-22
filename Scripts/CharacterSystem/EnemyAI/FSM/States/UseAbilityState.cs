namespace CharacterSystem
{
    public class UseSkillState : State
    {
        public UseSkillState(CharacterStateMachine stateMachine) : base(stateMachine)
        {
        }

        public override CharacterStateId StateId => CharacterStateId.UseAbillity;

        protected override void RegisterConditions()
        {
            
        }

        public override void Enter(BaseCharacter character)
        {
            base.Enter(character);
        }

        public override void Update(BaseCharacter character)
        {
            _agentCharacter.UseSkill();
            // 상태 전환
            _stateMachine.ChangeState(CharacterStateId.Idle);
        }

        public override void Exit(BaseCharacter character)
        {

        }
    }
}