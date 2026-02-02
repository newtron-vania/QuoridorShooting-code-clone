using UnityEngine;

namespace CharacterSystem
{
    public class CharacterStateMachine
    {
        public readonly EnemyCharacter Agent;
        public readonly BattleSystem BattleSystem;
        
        public State[] states;
        public CharacterStateId currentState = CharacterStateId.Idle;

        public bool CanAttack => Agent.CanAttack && Agent.TargetCharacter != null;
        public bool ShouldDamage => GameManager.Instance.BattleSystem.CellManager.GetDamageGraphCellCharacters(Agent.Position).Count > 0 ;
        public bool IsMyHpLow => Agent.Hp * 5 <= Agent.MaxHp;
        public bool IsTargetHpLow => Agent.TargetCharacter != null && (Agent.TargetCharacter.Hp * 5 <= Agent.TargetCharacter.MaxHp);

        public bool CanUseSkill => Agent.CanUseSkill;
        
        public CharacterStateMachine(BaseCharacter agent, BattleSystem controller)
        {
            this.Agent = agent as EnemyCharacter;
            BattleSystem = controller;
            int numState = System.Enum.GetNames(typeof(CharacterStateId)).Length;
            states = new State[numState];
        }

        public void RegisterState(State state)
        {
            int index = (int)state.StateId;
            states[index] = state;
        }

        public State GetState(CharacterStateId stateId)
        {
            int index = (int)stateId;
            return states[index];
        }

        public void Update()
        {
            GetState(currentState)?.Update(Agent);
        }

        public void ChangeState(CharacterStateId newState)
        {
            GetState(currentState)?.Exit(Agent);
            currentState = newState;
            GetState(currentState)?.Enter(Agent);
        }
    }

}