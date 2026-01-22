using System.Collections.Generic;
using UnityEngine;

namespace CharacterSystem
{

    public enum CharacterStateId
    {
        None,
        Idle,
        Attack,
        Move,
        MoveAttack,
        MoveEvade,
        BuildWall,
        UseAbillity
    }

    public abstract class State
    {
        protected Dictionary<CharacterStateId, ITransitionCondition> Transitions = new();

        protected CharacterStateMachine _stateMachine;

        protected EnemyCharacter _agentCharacter;

        public State(CharacterStateMachine stateMachine)
        {
            _stateMachine = stateMachine;

            _agentCharacter = stateMachine.Agent as EnemyCharacter;
            // ReSharper disable once VirtualMemberCallInConstructor
            RegisterConditions();
        }

        public abstract CharacterStateId StateId { get; }
        protected abstract void RegisterConditions();

        // 상태 선택 시 초기화 메서드
        public virtual void Enter(BaseCharacter character)
        {
            Debug.Log($"[INFO] AState::Enter - {this.GetType().Name} Input!");
        }

        // 상태 실시간 동작 메서드
        public abstract void Update(BaseCharacter character);

        // 상태 탈출 시 초기화 메서드
        public abstract void Exit(BaseCharacter character);
    }
}
