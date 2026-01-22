using System;
using System.Collections.Generic;
using UnityEngine;
using CharacterDefinition;
using EventType = HM.EventType;
using HM;
using UnityEditor.IMGUI.Controls;
using Random = UnityEngine.Random;

public partial class BaseCharacter : IEventListener , IMovable
{
    protected readonly CharacterController Controller;
    protected readonly SkillSystem SkillSystem;

    public delegate void ValueChangedHandler(int newValue);
    public event ValueChangedHandler OnDamage;

    public QuoridorCharacterObject CharacterObject { get; private set; }
    private List<PrefabControllerBase> _moveAttackPreviewList = new();

    #region TokenValues
    private Vector2Int _tokenPosition;

    public List<Vector2Int> AttackPositions { get; private set; } = new List<Vector2Int>() { Vector2Int.zero };

    public List<Vector2Int> SkillPositions { get; protected set; } = new List<Vector2Int>();

    public Sprite CharacterSprite;

    public virtual bool CanBuild
    {
        get
        {
            bool shouldAp = characterStat.Ap >= Controller.NeededBuildPoint;
            bool shouldBuildCount = false;
            switch (Playerable)
            {
                case true:
                    // shouldBuildCount = GameManager.Instance.playerWallCount > 0;
                    shouldBuildCount = GameManager.Instance.playerWallCount <= GameManager.Instance.playerMaxBuildWallCount;
                    break;
                case false:
                    shouldBuildCount = Controller.EnemyWallCount > 0;
                    break;
            }

            return shouldAp && shouldBuildCount;
        }
    }

    // TODO: Skill 추가 시 수정
    public virtual bool CanUseSkill
    {
        // Error
        // 어빌리티 참조 안됨
        get
        {
            return _skillSystem.CheckSkillCostEnough(SkillId, this);
        }
        // set
        // {
        //     CanUseSkill = value;
        // }
    }

    protected int CanAttackCount = 0; // _canAttackCount??
    public bool CanAttack
    {
        get
        {
            int totalCanAttackCount = CanAttackCount + StatuseffectController.AdditionalAttackActionCount;
            return (Mathf.Max(0, totalCanAttackCount) > 0);
        }

    }

    protected int _canMoveCount = 1;

    public void ReduceMoveCount()
    {
        _canMoveCount--;
    }


    public bool CanMove
    {
        get
        {
            int totalCanMoveCount = _canMoveCount + StatuseffectController.AdditionalMoveActionCount;
            return (Mathf.Max(0, totalCanMoveCount) > 0);
        }
    }

    public bool IsFinished => !(CanAttack || CanMove || CanBuild || CanUseSkill)
                              || (StatuseffectController.CheckUnderStatuseffect(StatuseffectInstance.Type.Provocation) && !CanAttack && !CanMove)
                              || isDead;

    public bool isDead { get; set; }

    public virtual bool CanDestroy => GameManager.Instance.playerDestroyedWallCount < GameManager.Instance.playerMaxDestroyWallCount;

    #endregion

    #region LoadToDataSet
    public int Id; // 고유 인덱스
    public bool Playerable; // Player True, Enemy False
    public string CharacterName => characterStat.Name; // 캐릭터 이름

    // 기존 열거형 정의 이름과 같은 문제발생 임의로 앞에 Token 추가 중복되는 의미이므로 변경할 예정
    public CharacterType Type; // 캐릭터 타입
    public CharacterClass Class; // 캐릭터 포지션

    //ToDo: 포지션 변경시 전 위치 변경 필요
    private Vector2Int _prevPosition;
    public Vector2Int PrevPosition => _prevPosition;

    public Action<IMovable> OnPositionChanged { get; set; }
    
    public Vector2Int Position
    {
        get
        {
            return _tokenPosition;
        }

        set
        {
            _prevPosition = _tokenPosition;

            // GameObject currentEnemy = Controller.GetObjectToPosition(GameManager.ToWorldPosition(_tokenPosition));
            GameObject currentObject = CharacterObject.gameObject;
            Debug.Log($"[INFO] BaseCharacter::PositionProperty: Position Set To {value}");
            if (currentObject)
            {
                Debug.LogFormat("[INFO] BaseCharacter::PositionProperty : {0} : Position 변경", CharacterObject.transform.name);
                // currentObject.transform.position = GameManager.ToWorldPosition(value);
                bool check = Controller.InsertCharacterCommand(new MoveCommand(this, GameManager.ToWorldPosition(value)));
                Debug.Log($"[INFO] BaseCharacter::PositionProperty : moveCommand On : {check}");
                _tokenPosition = value;

                //순차적인 움직임을 보여주기 힘든 구조
                OnPositionChanged?.Invoke(this); //이렇게 하면 스킬시스템이 아니더라도 OnPositionChange 참여가능
            }
            else
            {
                _tokenPosition = value;
            }
            SetMoveAttackPreview();
        }
    }
    public CharacterStat characterStat { get; private set; } = new CharacterStat();
    public int Hp => characterStat.Hp;
    public int MaxHp => characterStat.MaxHp;
    public int Atk => characterStat.Atk;
    public int SkillIndex;
    public int MoveRangeId;
    public int AttackRangeId;
    public readonly int BuildWallAp = 100;
    #endregion



    public void SetCharacterGameObject(QuoridorCharacterObject characterObject)
    {
        CharacterObject = characterObject;
        characterObject.Init(this);
    }
    public BaseCharacter(CharacterController controller)
    {
        Controller = controller;
        SkillSystem = controller.SkillSystem;
    }

    public virtual void Start()
    {
        Reset();

        ShowDebugLog();
        SetMoveAttackPreview();
    }

    private void ShowDebugLog()
    {
        Debug.Log($"[INFO] BaseCharacter::ShowDebugLog : skillSystem active is {_skillSystem is not null}");

        // Debug.Log($"[INFO] BaseCharacter::ShowDebugLog : skill name is {_skillSystem.SkillDict[SkillId].Name}");
    }

    public virtual void Update()
    {

    }

    public virtual void Reset()
    {
        CanAttackCount = 1;
        _canMoveCount = 1;
    }

    public virtual void End()
    {

    }

    public virtual void Move()
    {

    }

    public virtual void Attack()
    {

    }

    public virtual void Build()
    {

    }

    public virtual void UseSkill()
    {

    }

    public virtual void RecoverHealth(int recovery)
    {

    }

    public virtual void Die()
    {
        switch (Playerable)
        {
            case true:
                Controller.KillPlayerCharacterById(Id);
                break;
            case false:
                Controller.KillEnemyCharacterById(Id);
                break;
        }
        EventManager.Instance.InvokeEvent(EventType.OnCharacterDead, new Component());
        CharacterObject.gameObject.SetActive(false);
        isDead = true;
    }

    public virtual int TakeDamage(BaseCharacter baseCharacter, int damage = 0)
    {
        Debug.LogFormat("[INFO] BaseCharacter::TakeDamage - {0} 캐릭터 TakeDamage함수 실행", CharacterName);
        if (baseCharacter == null)
        {
            return 0;
        }
        // 일정 데미지를 입력하지 않았을 경우 (능력으로 인한 데미지 예외처리)
        if (damage == 0) damage = baseCharacter.characterStat.Atk;

        if (Random.Range(0.0f, 1.0f) < characterStat.Avd)
        {
            Debug.LogFormat("[INFO] BaseCharacter::TakeDamage - 캐릭터 이름 {0}이 공격을 회피했습니다.", CharacterName);
            UIManager.Instance.InputLogEvent(LogEvent.MissAttack, baseCharacter.CharacterObject.gameObject, CharacterObject.gameObject);
            OnDamage?.Invoke(0);
        }
        else
        {
            int expectDamge = (damage - SupplyManager.Instance.CheckDefenseSupply(CharacterObject.gameObject));
            expectDamge = (expectDamge < 0 ? 0 : expectDamge);
            characterStat.Hp = characterStat.Hp - expectDamge;
            OnDamage?.Invoke(expectDamge);
            UIManager.Instance.InputLogEvent(LogEvent.Attack, baseCharacter.CharacterObject.gameObject, CharacterObject.gameObject);

            if (characterStat.Hp <= 0)
            {
                Die();
            }

            return expectDamge;
        }

        return 0;
    }

    public virtual void ResetPreview()
    {
        HidePreviews();
    }

    // public bool IsAllies(CharacterIdentification identification)
    // {
    //     switch (identification)
    //     {
    //         case CharacterIdentification.Player:
    //             return Playerable;
    //         case CharacterIdentification.Enemy:
    //             return !Playerable;
    //         default:
    //             return false;
    //     }
    //     ;
    // }


    public virtual void OnTurnStart()
    {
        //리셋
        Reset();

        //행동력 회복 불가 상태가 아닐 경우. 행동력 회복 
        //if (!_skillSystem.CheckUnderStatuseffect(this, SkillEffect.StatuseffectTag.Weakness))
        characterStat.Ap += CharacterStat.ApRecovery;

        //캐릭터 컨트롤러에서 적인지 아군인지 타이밍 맞춰 실행중 
        StatuseffectController.InvokeGameEvent(HM.EventType.OnTurnStart);
    }

    public virtual void OnTurnEnd()
    {
        // PlayerActionUI playerActionUi = currentSelectTransform.GetChild(0).GetChild(0).GetComponent<PlayerActionUI>();
        
        StatuseffectController.InvokeGameEvent(HM.EventType.OnTurnEnd);
    }


    #region UseCharacterController
    // BaseCharacter 스크립트안 LoadDataSet부분 변수 채워넣기

    public void SetCharacterData(CharacterData data)
    {
        // CharacterStatBuilder를 사용하여 BaseStat을 설정
        characterStat = new CharacterStatBuilder()
            .SetIndex(data.Index)
            .SetName(data.Name)
            .SetMaxHp(data.Hp)
            .SetAtk((int)data.Atk)
            .SetAvd(data.Avd)
            .SetBaseAp(100)
            .SetApRecovery(data.ApRecovery)
            .Build();

        Id = data.Index;

        // 기타 데이터 설정
        // Playerable = data.IsPlayable;
        Type = data.Type;
        Class = data.Class;
        SkillIndex = data.SkillId;
        MoveRangeId = data.MoveRangeId;
        AttackRangeId = data.AttackRangeId;
        CharacterSprite = Resources.Load<Sprite>("Sprites/Character/" + characterStat.Name);

        // 가시 범위 및 공격 범위 설정
        // _movablePositions = CharacterRangeFrame.SelectFieldProperty(PlayerRangeField.Move, MoveRangeId);
        // _attackablePositions = CharacterRangeFrame.SelectFieldProperty(PlayerRangeField.Attack, AttackRangeId);
        characterStat.MovablePositions = DataManager.Instance.GetRangeData(MoveRangeId, Playerable);
        characterStat.AttackablePositions = DataManager.Instance.GetRangeData(AttackRangeId, Playerable);
        // _skillPositions = SkillSystem.GetTargetablePositionList(data.SkillId, this);

        Debug.LogFormat("[INFO] BaseCharacter::SetCharacterData - {0}이 생성되었습니다.", characterStat.Name);
    }

    // BaseCharacter 복사 후 전달
    public BaseCharacter DeepCopy(bool isPlayer = false)
    {
        BaseCharacter baseCharacter;

        // 플레이어/적 캐릭터 객체 생성
        if (isPlayer)
        {
            baseCharacter = new PlayerCharacter(Controller);
        }
        else
        {
            baseCharacter = new EnemyCharacter(Controller);
        }

        // CharacterStat 복사
        baseCharacter.characterStat = new CharacterStatBuilder()
            .SetIndex(characterStat.Index)
            .SetName(characterStat.Name)
            .SetMaxHp(characterStat.MaxHp)
            .SetAtk(characterStat.Atk)
            .SetAvd(characterStat.Avd)
            .SetBaseAp(characterStat.Ap)
            .SetApRecovery(characterStat.ApRecovery)
            .Build();

        baseCharacter.Id = Id;

        baseCharacter.characterStat.Hp = characterStat.Hp;

        // 기타 속성 복사
        baseCharacter.Playerable = Playerable;
        baseCharacter.Type = Type;
        baseCharacter.Class = Class;
        baseCharacter.SkillIndex = SkillIndex;
        baseCharacter.MoveRangeId = MoveRangeId;
        baseCharacter.AttackRangeId = AttackRangeId;
        baseCharacter.CharacterSprite = CharacterSprite;
        baseCharacter.characterStat.MovablePositions = new List<Vector2Int>(characterStat.MovablePositions);
        baseCharacter.characterStat.AttackablePositions = new List<Vector2Int>(characterStat.AttackablePositions);
        baseCharacter.SkillPositions = new List<Vector2Int>(SkillPositions);

        return baseCharacter;
    }

    private void SetMoveAttackPreview()
    {
        HashSet<Vector2Int> attackablePreviewPositionSet = new();
        HashSet<Vector2Int> movablePreviewPositionSet = new();

        foreach (var preview in _moveAttackPreviewList)
        {
            if(preview is GlowBoxPrefab) PrefabMakerSystem.Instance.GetObjectMaker(MakerType.CharacterPositionPreview).ReturnController(preview);
            else PrefabMakerSystem.Instance.GetObjectMaker(MakerType.MovePreview).ReturnController(preview);
        }
        _moveAttackPreviewList.Clear();
        movablePreviewPositionSet.Add(Position);

        if (CanMove)
        {

            Vector3 characterWorldPos = GameManager.ToWorldPosition(Position);
            foreach (var localMovablePos in characterStat.MovablePositions)
            {
                Vector2Int movablePos = Position + localMovablePos;

                bool[] result = HM.Physics.RayUtils.CheckWallRay(characterWorldPos, localMovablePos);

                if (result[1] || result[0])
                    continue;

                movablePreviewPositionSet.Add(movablePos);
            }
        }
        if (CanAttack)
        {
            foreach (var movablePos in movablePreviewPositionSet)
            {
                Vector3 movableWorldPos = GameManager.ToWorldPosition(movablePos);
                foreach (var localAttackablePos in characterStat.AttackablePositions)
                {
                    Vector2Int AttackablePos = movablePos + localAttackablePos;

                    bool[] result = HM.Physics.RayUtils.CheckWallRay(movableWorldPos, localAttackablePos);

                    if (result[1] || result[0])
                        continue;
                    attackablePreviewPositionSet.Add(AttackablePos);
                }
            }
        }
        attackablePreviewPositionSet.ExceptWith(movablePreviewPositionSet);
        movablePreviewPositionSet.Remove(Position);

        if (Playerable)
        {
            PrefabControllerBase tmpController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.CharacterPositionPreview).GetController(Position);
            tmpController.gameObject.SetActive(false);
            _moveAttackPreviewList.Add(tmpController);
        }
        else
        {
            PrefabControllerBase tmpController = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.EnemyCharacterPositionPreview).GetController(Position);
            tmpController.gameObject.SetActive(false);
            _moveAttackPreviewList.Add(tmpController);
        }

        foreach (var movablePos in movablePreviewPositionSet)
        {
            PrefabControllerBase controller = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.HighlightPreview).GetController(movablePos);
            controller.gameObject.SetActive(false);
            _moveAttackPreviewList.Add(controller);
        }
        foreach (var attackablePreviewPos in attackablePreviewPositionSet)
        {
            PrefabControllerBase controller = PrefabMakerSystem.Instance.GetObjectMaker(MakerType.RedAttackPreview).GetController(attackablePreviewPos);
            controller.gameObject.SetActive(false);
            _moveAttackPreviewList.Add(controller);
        }
    }
    //캐릭터 터치 프리뷰 사용
    public void ShowPreviewsOnCharacterTouch()
    {
        foreach (var preview in _moveAttackPreviewList)
        {
            preview.gameObject.SetActive(true);
        }
    }
    //캐릭터 터치 프리뷰 리셋
    public void HidePreviews()
    {
        foreach (var preview in _moveAttackPreviewList)
        {
            preview.gameObject.SetActive(false);
        }
    }
    #endregion

    #region BaseCharacterValueControlFunc

    public void ReduceAttackCount()
    {
        CanAttackCount--;
    }

    public void IncreaseAttackCount()
    {
        CanAttackCount++;
    }
    #endregion

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        throw new System.NotImplementedException();
    }
}
