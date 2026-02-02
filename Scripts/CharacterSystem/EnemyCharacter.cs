using System;
using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using CharacterSystem;
using HM.Utils;
using UnityEngine;

public class EnemyCharacter : BaseCharacter
{
    private enum EnemyStateMachineType
    {
        Default,
        Taunt
    }

    private static readonly CharacterDamageCalculator DamageCalculator = new CharacterDamageCalculator();
    private readonly SkillTargetSelector _targetSelector;
    private readonly Dictionary<EnemyStateMachineType, CharacterStateMachine> _enemyStateMachines;

    public BaseCharacter TargetCharacter { get; protected set; }

    public EnemyCharacter(BattleSystem controller) : base(controller)
    {
        _targetSelector = new SkillTargetSelector(controller);
        _enemyStateMachines = new Dictionary<EnemyStateMachineType, CharacterStateMachine>();
        foreach (var type in (EnemyStateMachineType[])Enum.GetValues(typeof(EnemyStateMachineType)))
        {
            _enemyStateMachines[type] = new CharacterStateMachine(this, Controller);
            var stateMachine = _enemyStateMachines[type];
            switch (type)
            {
                case EnemyStateMachineType.Taunt:
                    stateMachine.RegisterState(new TauntIdleState(stateMachine));
                    break;
                default:
                    stateMachine.RegisterState(new IdleState(stateMachine));
                    break;
            }

            stateMachine.RegisterState(new MoveState(stateMachine));
            stateMachine.RegisterState(new AttackState(stateMachine));
            stateMachine.RegisterState(new MoveAttackState(stateMachine));
            stateMachine.RegisterState(new MoveEvadeState(stateMachine));
            stateMachine.RegisterState(new BuildWallState(stateMachine));
            stateMachine.RegisterState(new UseSkillState(stateMachine));
            stateMachine.currentState = CharacterStateId.Idle;
        }
    }

    public Vector2Int NextMovePosition { get; private set; }

    public override bool CanBuild
    {
        get
        {
            if (characterStat.Ap < BuildWallAp) return false;

            if (Playerable)
                return GameManager.Instance.playerWallCount > 0 && GameManager.Instance.playerMaxBuildWallCount <= 10;
            return Controller.EnemyWallCount > 0;
        }
    }

    public override bool CanDestroy => false;

    public override void Start()
    {
        base.Start();
    }

    public override void Update()
    {
        base.Update();
        if (!isDead) EnemyUpdate();
    }

    private void EnemyUpdate()
    {
        if (!IsFinished)
        {
            if (StatuseffectController.CheckUnderStatuseffect(StatuseffectInstance.Type.Provocation))
                _enemyStateMachines[EnemyStateMachineType.Taunt].Update();
            else
                _enemyStateMachines[EnemyStateMachineType.Default].Update();
        }
    }


    public override void Attack()
    {
        if (!CanAttack) return;
        base.Attack();
        var check = Controller.InsertCharacterCommand(new AttackCommand(this, TargetCharacter));
        Debug.Log($"[INFO] EnemyCharacter::Attack() - AttackCommand On : {check}");
        Debug.Log(
            $"[INFO] EnemyCharacter::Attack() - {CharacterName} Attack target name & hp : {TargetCharacter.CharacterObject.transform.name + "_" + TargetCharacter.Id} & {TargetCharacter.Hp}");
        int damage = DamageCalculator.CalculateDamage(this, TargetCharacter);
        TargetCharacter?.TakeDamage(this, damage);
        Debug.Log($"[INFO] EnemyCharacter::Attack() - Attack Target! remainedHp = {TargetCharacter.Hp}");
        Controller.InsertCharacterCommand(new WaitForTimeCommand(0.2f));
        ReduceAttackCount();
    }

    public override void Move()
    {
        if (!CanMove) return;
        base.Move();
        if (Position != NextMovePosition)
        {
            Debug.Log(
                $"[INFO] EnemyCharacter::Move() - {CharacterObject.transform.name} Move Next Position : {NextMovePosition}");
            Position = NextMovePosition;
        }

        Controller.InsertCharacterCommand(new WaitForTimeCommand(0.2f));
        _canMoveCount--;
    }

    public override void UseSkill()
    {
        base.UseSkill();
        var targetablePositions = _skillSystem.GetTargetablePositionList(SkillId, Position);

        var resultTargetPosition = _targetSelector.GetSkillTargetPositions(SkillId, this, targetablePositions);

        Debug.Log(
            $"[INFO] EnemyCharacter::UseSkill() - {CharacterObject.name} Execute Skill : {SkillId} at {resultTargetPosition}");
        _skillSystem.ExecuteSkill(SkillId,  resultTargetPosition,this);
        Controller.InsertCharacterCommand(new WaitForTimeCommand(1f));
        Debug.Log($"[INFO] EnemyCharacter::UseSkill() - Enemy characterStat : {characterStat.Ap}");
    }

    public override void Build()
    {
        base.Build();
    }

    public override void RecoverHealth(int recovery)
    {
        base.RecoverHealth(recovery);
    }

    public override int TakeDamage(BaseCharacter baseCharacter, int damage = 0)
    {
        return base.TakeDamage(baseCharacter, damage);
    }

    #region MoveFunc

    // 유닛이 이동해야 할 위치(타겟 팀 유닛을 공격할 수 있는 위치로 이동할 경로)를 반환하는 메서드
    // TODO : refactor: Pathfinding 리팩토링 후 새 방식으로 구현(다양한 goal을 받아 실행하는 방식으로 구현)
    public void SetNextMovingPositionForAttack(CharacterIdentification targetIdentification)
    {
        var targetPositions = GetPositionsToAttackCharacters(targetIdentification);

        var targetPath = new List<Vector2Int>();

        var characterPositions = Controller.GetAllCharactersPosition();

        foreach (var targetPosition in targetPositions)
        {
            var usingCharacterPositions = characterPositions
                .Where(pos => pos != Position)
                .ToList();

            var mapGraph = PathFindingUtils.GetMapGraph(usingCharacterPositions, GameManager.Instance.WallList);

            var path = PathFindingUtils.FindPath(mapGraph, Position, targetPosition, characterStat.MovablePositions);

            var pathEnd = path.Count > 0 ? path[^1] : Position;

            if (targetPositions.Contains(pathEnd))
            {
                if (targetPath.Count == 0 || path.Count < targetPath.Count) targetPath = new List<Vector2Int>(path);
            }
            else
            {
                if (targetPath.Count == 0 || !targetPositions.Contains(targetPath[^1]))
                    if (targetPath.Count == 0 || path.Count < targetPath.Count)
                        targetPath = new List<Vector2Int>(path);
            }
        }

        if (targetPath.Count > 0)
        {
            NextMovePosition = targetPath[0];
            Debug.Log(
                $"[INFO] EnemyCharacter::SetNextMovingPositionForAttack - Next moving position set: {NextMovePosition}");
        }
        else
        {
            NextMovePosition = Position;
            Debug.Log(
                "[INFO] EnemyCharacter::SetNextMovingPositionForAttack - No valid path found, staying in current position.");
        }
    }

    // 실제 이동 가능 위치를 반환하는 메서드
    private List<Vector2Int> GetPositionsToMovable()
    {
        var movablePositions = new List<Vector2Int>();
        var characterPositions = Controller.GetAllCharactersPosition();

        var usingCharacterPositions = characterPositions
            .Where(pos => pos != Position)
            .ToList();

        var mapGraph = PathFindingUtils.GetMapGraph(usingCharacterPositions, GameManager.Instance.WallList);

        foreach (var movablePosition in characterStat.MovablePositions)
        {
            var targetPosition = Position + movablePosition;
            var distance = Mathf.Abs(movablePosition.x) + Mathf.Abs(movablePosition.y);
            if (PathFindingUtils.CanReachWithoutStuckInWall(mapGraph, Position, targetPosition, true))
                movablePositions.Add(targetPosition);
        }

        foreach (var VARIABLE in movablePositions)
            Debug.Log($"[INFO] EnemyCharacter::GetPositionsToMovable - movablePosition : {VARIABLE}");
        return movablePositions;
    }

    public void SetNextMovingPositionForEvade(CharacterIdentification targetIdentification)
    {
        var movablePositions = GetPositionsToMovable();
        if (movablePositions.Count == 0)
        {
            // 이동 가능한 위치가 없으면 현재 위치 유지
            NextMovePosition = Position;
            return;
        }

        // 위험도를 저장할 Dictionary (위치, 총 데미지량)
        var positionRiskMap = new Dictionary<Vector2Int, int>();

        foreach (var position in movablePositions)
            // 해당 위치의 총 데미지량 계산
            if (!GameManager.Instance.BattleSystem.CellManager.GetDamageGraphCellCharacters(position).Any())
            {
                // 데미지 필드가 없는 위치는 위험도가 0
                positionRiskMap[position] = 0;
            }
            else
            {
                // 해당 위치에 있는 모든 캐릭터의 공격력 합산
                var totalDamage = GameManager.Instance.BattleSystem.CellManager.GetDamageGraphCellCharacters(position)
                    .Sum(character => character.Atk);
                positionRiskMap[position] = totalDamage;
            }

        // 위험도가 가장 낮은 위치 찾기
        var safestPosition = positionRiskMap.OrderBy(pair => pair.Value).FirstOrDefault();

        // NextMovingPosition에 저장
        NextMovePosition = safestPosition.Key;
    }

    public void SetEvadeTargetCharacter(CharacterIdentification targetIdentification, int index = 0)
    {
        GameManager.Instance.BattleSystem.CellManager.SetDamageFieldGraph(
            Controller.StageCharacter[CharacterIdentification.Player]);

        var damagedList = GameManager.Instance.BattleSystem.CellManager.GetDamageGraphCellCharacters(Position)
            .OrderByDescending(character => character.Atk).ToArray();
        TargetCharacter = damagedList.Length > index ? damagedList[index] : null;
    }

    #endregion

    #region AttackFunc

    // 공격 가능한 캐릭터를 반환
    public void SetAttackTargetCharacter(CharacterIdentification targetIdentification)
    {
        var attackPositions = new List<Vector2Int>();

        // StageCharacter에서 플레이어 데이터를 가져옴
        if (Controller == null || !Controller.StageCharacter.ContainsKey(targetIdentification))
        {
            Debug.LogWarning(
                "[WARN] EnemyCharacter::SetAttackTargetCharacter() - Controller is null or StageCharacter does not contain Player data.");
            TargetCharacter = null;
            return;
        }

        foreach (var attackablePosition in characterStat.AttackablePositions) attackPositions.Add(Position + attackablePosition);

        // 보유 체력이 가장 낮은 타겟을 우선순위로 설정
        BaseCharacter target = null;
        foreach (var playerCharacter in Controller.StageCharacter[targetIdentification])
            if (attackPositions.Contains(playerCharacter.Position))
                if (target == null || target.Hp > playerCharacter.Hp)
                    target = playerCharacter;

        TargetCharacter = target;
    }

    // 캐릭터를 공격 가능한 위치를 반환
    protected List<Vector2Int> GetPositionsToAttackCharacters(CharacterIdentification targetIdentification)
    {
        var positionsToAttack = new List<Vector2Int>();

        // StageCharacter에서 플레이어 데이터를 가져옴
        if (Controller == null || !Controller.StageCharacter.ContainsKey(targetIdentification))
        {
            Debug.LogWarning(
                "[WARN] EnemyCharacter::GetPositionsToAttackCharacters() - Controller is null or StageCharacter does not contain Player data.");
            return positionsToAttack;
        }

        // 상대 진영 유닛을 기준으로 계산
        foreach (var targetCharacters in Controller.StageCharacter[targetIdentification])
        {
            var targetPosition = targetCharacters.Position;

            var characterPositions = Controller.GetAllCharactersPosition();

            foreach (var attackOffset in characterStat.AttackablePositions)
            {
                var possiblePosition = targetPosition - attackOffset;
                var mapGraph = PathFindingUtils.GetMapGraph(characterPositions, GameManager.Instance.WallList, true);
                if (PathFindingUtils.CanReach(mapGraph, Position, possiblePosition, 999, true) &&
                    !positionsToAttack.Contains(possiblePosition)) positionsToAttack.Add(possiblePosition);
            }
        }


        return positionsToAttack;
    }

    #endregion
}