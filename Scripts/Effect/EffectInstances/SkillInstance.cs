using CharacterDefinition;
using System.Collections.Generic;
using UnityEngine;

public enum SkillStateType
{
    Normal,
    TimeDistortion,
    Rotten,
}

public class SkillInstance : EffectInstance
{
    /*
     //필요해 보였는데 당장 안 필요한 것들
     //public readonly SkillData SkillData;
    */

    private SkillSystem _skillSystem;

    public readonly Vector2Int SkillPosition;

    private HashSet<Vector2Int> _area = new HashSet<Vector2Int>();

    public HashSet<Vector2Int> Area { get { return new HashSet<Vector2Int>(_area); } }

    public int Duration { get; set; }

    public bool IsStackable { get; private set; }

    public SkillStateType State { get; set; } = SkillStateType.Normal;


    //이것도 유틸 함수?
    private bool CheckStageInside(Vector2Int pos)
    {
        if (pos.x < -4 || pos.y < -4 || pos.x > 4 || pos.y > 4)
            return false;
        return true;
    }
    private void InitArea(SkillData skillData)
    {
        List<Vector2Int> zonePosList = DataManager.Instance.GetRangeData(skillData.EffectRangeId, true);
        foreach (Vector2Int zonePos in zonePosList)
        {
            Vector2Int AreaPos = zonePos + SkillPosition;

            if (!CheckStageInside(AreaPos))
                continue;

            Vector3 skillWorldPos = GameManager.ToWorldPosition(SkillPosition);

            bool[] result = HM.Physics.RayUtils.CheckWallRay(skillWorldPos, zonePos);

            if (result[1] || result[0])
                continue;

            _area.Add(AreaPos);
        }
    }

    private void InitInstanceEffects(SkillData skillData)
    {
        foreach (var effectData in skillData.EffectDataList)
        {
            _effectDataList.Add(effectData);
        }
    }

    public SkillInstance(SkillSystem skillSystem,EffectSystem effectSystem, SkillData skillData, BaseCharacter source, Vector2Int skillPosition):base(source,effectSystem)
    {
        _skillSystem = skillSystem;
        SkillPosition = skillPosition;
        Duration = skillData.Duration;

        //스택가능 기본값
        IsStackable = false; //TODO: 스택가능 여부 설정

        //effectList초기화
        InitInstanceEffects(skillData);

        //ActiveArea 초기화
        InitArea(skillData);

        Debug.Log($"[Info]SkillInstance::SkillInstance SkillId: {skillData.Id}");
        
        
     
    }

    #region Funcs for TargetFind

    private List<BaseCharacter> GetTargetTypeCharacterList(TargetType targetType)
    {
        Dictionary<CharacterIdentification, List<BaseCharacter>> stageCharacterDict = GameManager.Instance.CharacterController.StageCharacter;

        List<BaseCharacter> targetTypeCharacterList = new List<BaseCharacter>();

        switch(targetType)
        {
            case TargetType.Self:
                break;
            case TargetType.Ally:
                targetTypeCharacterList.AddRange(stageCharacterDict[Source.Identification]);
                break;
            case TargetType.Enemy:
                if(Source.Identification==CharacterIdentification.Player)
                    targetTypeCharacterList.AddRange(stageCharacterDict[CharacterIdentification.Enemy]);
                else
                    targetTypeCharacterList.AddRange(stageCharacterDict[CharacterIdentification.Player]);
                break;
            case TargetType.All:
                targetTypeCharacterList.AddRange(stageCharacterDict[CharacterIdentification.Player]);
                targetTypeCharacterList.AddRange(stageCharacterDict[CharacterIdentification.Enemy]);
                break;
            default:
                Debug.LogWarning("[WARN]SkillInstance::GetTargetTypeCharacterList: 유효하지않은 TargetType");
                break;
        }

        return targetTypeCharacterList;
    }

    //타겟 찾기 //필수
    public override List<IEffectableProvider> FindEffectTargetList(TargetType effectTargetType)
    {
        List<IEffectableProvider> targetharacterList = new List<IEffectableProvider>();
        
        //타입이 자기자신이면 범위안에없더라도 포함되게 변경
        if (effectTargetType == TargetType.Self)
        {
            targetharacterList.Add(Source);
            return  targetharacterList;
        }
        
        List<BaseCharacter> targetTypeCharacterList = GetTargetTypeCharacterList(effectTargetType);

        foreach (var target in targetTypeCharacterList)
        {
            if (_area.Contains(target.Position))
            {
                targetharacterList.Add(target);
            }
        }
        return targetharacterList;
    }
    #endregion

    #region EventHandle 

    //게임 이벤트
    public void OnGameEvent(HM.EventType eventType)
    {
        //능력을 사용한 주체가 플레이어인데 플레이어턴이 아니거나 반대일 경우 무시되게
        if ((Source.Playerable && GameManager.Instance.Turn % 2 == 0) ||
            (!Source.Playerable && GameManager.Instance.Turn % 2 == 1))
            return;
        
        switch (eventType)
        {
            case HM.EventType.OnTurnEnd:
                Duration--;
                break;
        }

        foreach (EffectData effectData in _effectDataList)
        {
            List<IEffectableProvider> targetList = FindEffectTargetList(effectData.Target);
            IBaseEffectLogic effectLogic = _effectSystem.EffectLogicDict[effectData.Type];
            effectLogic.EffectByGameEvent(eventType, this, effectData, targetList);
        }
    }

    //무버블 이벤트 이건 붙이는 용도일 가능성 높음 스킬 시스템에서 객체 생성할 때 모든 Movable에 대해 붙이기? ToDo: 만약 붙이면 지워주는 것도?
    public void InvokeMovableEventEffect(IMovable movableTarget)
    {
        MovableEvent movableEvent;

        bool isInArea = _area.Contains(movableTarget.Position);
        bool wasInArea = _area.Contains(movableTarget.PrevPosition);

        //영역에 진입했을 경우 was:false is:true
        if (!wasInArea && isInArea)
            movableEvent = MovableEvent.OnTileEnter;
        //영역을 빠져나올 경우 was:true is:false
        else if (wasInArea && !isInArea)
            movableEvent = MovableEvent.OnTileExit;
        //둘 다 아니면 이벤트 없음
        else
            return;

        //타겟이 효과 대상인지 확인
        if (movableTarget is not IEffectableProvider target)
            return;

        //확인되었으면 관련 이펙트 실행 //InvokeMovableEvent
        foreach (EffectData effectData in _effectDataList)
        { 
            //타겟 타입 맞는지만 확인 // 전부 타겟이 아니고 타겟 타입도 안맞으면 컨티뉴
            CharacterIdentification targetIdentification = target.GetEffectable<ISkillParticipant>().Identification;

            switch (effectData.Target)
            {
                case TargetType.Self:
                    if (target != Source)
                        continue;
                    break;
                case TargetType.Ally:
                    if (targetIdentification != Source.Identification)
                        continue;
                    break;
                case TargetType.Enemy:
                    if (targetIdentification == Source.Identification)
                        continue;
                    break;
                case TargetType.All:
                    break;
                default:
                    Debug.LogWarning("[WARN]SkillInstance::InvokeMovableEventEffect: 유효하지않은 TargetType");
                    break;
            }

            var areaEffectLogic = _effectSystem.EffectLogicDict[effectData.Type] as IMovableEventEffectLogic;
            areaEffectLogic?.EffectByMovableEvent(movableEvent, this, effectData, target);
        }
    }
    #endregion

    #region Funcs for SkillSystem
    public void UpdateArea(HashSet<Vector2Int> newSkillArea)
    {
        if (!IsStackable)
        {
            HashSet<Vector2Int> newArea = new HashSet<Vector2Int>(_area);

            newArea.ExceptWith(newSkillArea);

            //영역에서 나가진 타겟들에 대해 OnTileExit 이벤트 발생
            foreach (EffectData effectData in _effectDataList)
            {
                IBaseEffectLogic effectLogic = _effectSystem.EffectLogicDict[effectData.Type];
                if (effectLogic is not IMovableEventEffectLogic movableEventEffect)
                    continue;

                foreach(var target in FindEffectTargetList(effectData.Target))
                {
                    //새로워진 영역에 포함되어 있으면 무시
                    if (newArea.Contains(target.GetEffectable<BaseCharacter>().Position))
                        continue;
                    movableEventEffect.EffectByMovableEvent(MovableEvent.OnTileExit, this, effectData, target);
                }
            }
            _area = newSkillArea;
        }      
    }
    #endregion

    #region Funcs for SkillEffectLogic
    public void ChangeInstanceState(SkillStateType stateType)
    {
        State = stateType;
        //상태 변경에 따른 SkillSystem에 Instance들 Area업데이트 요청
        _skillSystem.UpdateSkillInstanceArea(this);

    }
    #endregion

}
