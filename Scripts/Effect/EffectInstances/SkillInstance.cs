using CharacterDefinition;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 인스턴스 클래스
/// - 영역 기반 지속형 효과를 관리하는 Effect 시스템의 핵심 클래스
/// - IDurableEffect 구현으로 턴 기반 Duration 관리
/// - IMovableEventEffectLogic 통합으로 위치 기반 효과 처리 (Cell 진입/퇴출)
/// - SkillSystem에 의해 생명주기 관리
/// </summary>
public class SkillInstance : EffectInstance, IDurableEffect
{
    /*
     //필요해 보였는데 당장 안 필요한 것들
     //public readonly SkillData SkillData;
    */

    /// <summary>SkillSystem 참조 (상태 변경 시 Area 업데이트 요청용)</summary>
    private SkillSystem _skillSystem;

    /// <summary>스킬이 사용된 그리드 위치</summary>
    public readonly Vector2Int SkillPosition;

    /// <summary>스킬의 실제 효과 범위 (벽 차단 고려)</summary>
    private HashSet<Vector2Int> _area = new HashSet<Vector2Int>();

    /// <summary>
    /// 스킬의 효과 범위 (읽기 전용 복사본)
    /// - 외부에서 직접 수정 불가
    /// </summary>
    public HashSet<Vector2Int> Area { get { return new HashSet<Vector2Int>(_area); } }

    /// <summary>스킬의 지속 턴 수 (0 이상: 활성, 0 미만: 만료)</summary>
    public int Duration { get; set; }

    /// <summary>
    /// 다른 스킬과 영역 중첩 가능 여부
    /// - false: 중첩 시 기존 스킬 영역 축소
    /// - true: 여러 스킬 영역 중첩 가능
    /// </summary>
    public bool IsStackable { get; private set; }

    #region IDurableEffect 구현
    /// <summary>고유 식별자 (위치 기반)</summary>
    public string EffectId => $"Skill_{SkillPosition.x}_{SkillPosition.y}";
    /// <summary>스킬 사용자</summary>
    public BaseCharacter Owner => Source;
    /// <summary>활성 상태 여부 (Duration >= 0)</summary>
    public bool IsActive => Duration >= 0;
    /// <summary>실행 우선순위 (스킬은 고정값 0 사용)</summary>
    public int Priority => 0;
    #endregion

    #region 초기화 메서드

    /// <summary>
    /// 스테이지 범위 내 위치인지 확인
    /// </summary>
    /// <param name="pos">확인할 그리드 위치</param>
    /// <returns>스테이지 범위 내 위치면 true</returns>
    private bool CheckStageInside(Vector2Int pos)
    {
        if (pos.x < -4 || pos.y < -4 || pos.x > 4 || pos.y > 4)
            return false;
        return true;
    }

    /// <summary>
    /// 스킬의 실제 효과 범위(_area) 초기화
    /// - EffectRangeId 데이터 기반 범위 계산
    /// - 벽 차단 검사 (RayUtils.CheckWallRay)
    /// - 스테이지 범위 검증
    /// </summary>
    /// <param name="skillData">스킬 데이터</param>
    private void InitArea(SkillData skillData)
    {
        List<Vector2Int> zonePosList = DataManager.Instance.GetRangeData(skillData.EffectRangeId, true);
        foreach (Vector2Int zonePos in zonePosList)
        {
            Vector2Int AreaPos = zonePos + SkillPosition;

            // 스테이지 범위 밖이면 제외
            if (!CheckStageInside(AreaPos))
                continue;

            Vector3 skillWorldPos = GameManager.ToWorldPosition(SkillPosition);

            // 벽 차단 검사 (외벽/내벽)
            bool[] result = HM.Physics.RayUtils.CheckWallRay(skillWorldPos, zonePos);

            if (result[1] || result[0])
                continue;

            _area.Add(AreaPos);
        }
    }

    /// <summary>
    /// 스킬의 효과 데이터 리스트 초기화
    /// </summary>
    /// <param name="skillData">스킬 데이터</param>
    private void InitInstanceEffects(SkillData skillData)
    {
        foreach (var effectData in skillData.EffectDataList)
        {
            _effectDataList.Add(effectData);
        }
    }

    /// <summary>
    /// SkillInstance 생성자
    /// - 효과 범위 초기화 (벽 차단 고려)
    /// - DurableEffectRegistry 자동 등록 (Duration >= 0인 경우)
    /// </summary>
    /// <param name="skillSystem">SkillSystem 참조</param>
    /// <param name="statManager">StatManager 참조</param>
    /// <param name="skillData">스킬 데이터</param>
    /// <param name="source">스킬 사용자</param>
    /// <param name="skillPosition">스킬 사용 위치</param>
    public SkillInstance(SkillSystem skillSystem,StatManager statManager, SkillData skillData, BaseCharacter source, Vector2Int skillPosition):base(source,statManager)
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

        // DurableEffectRegistry에 등록 (턴 기반 Duration 관리)
        if (Duration >= 0 && DurableEffectRegistry.Instance != null)
        {
            DurableEffectRegistry.Instance.RegisterEffect(this);
        }

        Debug.Log($"[Info]SkillInstance::SkillInstance SkillId: {skillData.Id}");



    }
    #endregion

    #region Funcs for TargetFind

    /// <summary>
    /// TargetType에 해당하는 캐릭터 리스트 반환
    /// </summary>
    /// <param name="targetType">타겟 타입 (Self/Ally/Enemy/All)</param>
    /// <returns>해당 타입의 캐릭터 리스트</returns>
    private List<BaseCharacter> GetTargetTypeCharacterList(TargetType targetType)
    {
        Dictionary<CharacterIdentification, List<BaseCharacter>> stageCharacterDict = GameManager.Instance.BattleSystem.StageCharacter;

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

    /// <summary>
    /// 스킬 효과 범위(_area) 내에 있는 타겟 리스트 반환
    /// - Self 타입: 범위 무관하게 Source만 반환
    /// - 나머지: 범위 내 + TargetType 일치하는 캐릭터 반환
    /// </summary>
    /// <param name="effectTargetType">효과 타겟 타입</param>
    /// <returns>효과를 받을 타겟 리스트</returns>
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

    /// <summary>
    /// 게임 이벤트 발생 시 처리
    /// - 턴 소유권 검증 (Source의 턴일 때만 효과 실행)
    /// - 모든 EffectData에 대해 IBaseEffectLogic.EffectByGameEvent() 호출
    /// </summary>
    /// <param name="eventType">게임 이벤트 타입 (OnTurnEnd 등)</param>
    public void OnGameEvent(HM.EventType eventType)
    {
        //능력을 사용한 주체가 플레이어인데 플레이어턴이 아니거나 반대일 경우 무시되게
        if ((Source.Playerable && GameManager.Instance.Turn % 2 == 0) ||
            (!Source.Playerable && GameManager.Instance.Turn % 2 == 1))
            return;

        foreach (EffectData effectData in _effectDataList)
        {
            List<IEffectableProvider> targetList = FindEffectTargetList(effectData.Target);
            IBaseEffectLogic effectLogic = _statManager.GetEffectLogic(effectData.Type);
            effectLogic.EffectByGameEvent(eventType, this, effectData, targetList);
        }
    }

    /// <summary>
    /// DurableEffectRegistry에서 호출되는 턴 시작 처리
    /// - OnTurnStart 게임 이벤트 실행
    /// </summary>
    public void ProcessTurnStart()
    {
        OnGameEvent(HM.EventType.OnTurnStart);
    }

    /// <summary>
    /// DurableEffectRegistry에서 호출되는 턴 종료 처리
    /// - Duration 1 감소
    /// - OnTurnEnd 게임 이벤트 실행
    /// - Duration 만료 시 OnExpire() 호출
    /// </summary>
    public void ProcessTurnEnd()
    {
        Duration--;
        OnGameEvent(HM.EventType.OnTurnEnd);

        if (Duration <= 0)
        {
            OnExpire();
        }
    }

    /// <summary>
    /// Duration 만료 시 호출
    /// - EffectInstanceEvent.End 발생
    /// - DurableEffectRegistry에서 등록 해제
    /// </summary>
    public void OnExpire()
    {
        InvokeInstanceEvent(EffectInstanceEvent.End);

        if (DurableEffectRegistry.Instance != null)
        {
            DurableEffectRegistry.Instance.UnregisterEffect(this);
        }

        Debug.Log($"[INFO]SkillInstance::OnExpire() - {EffectId} expired");
    }

    /// <summary>
    /// 캐릭터 이동 시 호출되는 위치 기반 효과 처리
    /// - 스킬 영역(_area) 진입/퇴출 감지
    /// - OnCellEnter: 이전에 없었고 현재 있으면
    /// - OnCellExit: 이전에 있었고 현재 없으면
    /// - TargetType 검증 후 IMovableEventEffectLogic.EffectByMovableEvent() 호출
    /// </summary>
    /// <param name="movableTarget">이동한 캐릭터</param>
    public void InvokeMovableEventEffect(IMovable movableTarget)
    {
        MovableEvent movableEvent;

        bool isInArea = _area.Contains(movableTarget.Position);
        bool wasInArea = _area.Contains(movableTarget.PrevPosition);

        //영역에 진입했을 경우 was:false is:true
        if (!wasInArea && isInArea)
            movableEvent = MovableEvent.OnCellEnter;
        //영역을 빠져나올 경우 was:true is:false
        else if (wasInArea && !isInArea)
            movableEvent = MovableEvent.OnCellExit;
        //둘 다 아니면 이벤트 없음
        else
            return;

        //타겟이 효과 대상인지 확인
        if (movableTarget is not IEffectableProvider target)
            return;

        //확인되었으면 관련 이펙트 실행
        foreach (EffectData effectData in _effectDataList)
        {
            //타겟 타입 맞는지만 확인
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

            // IMovableEventEffectLogic 구현체에 위치 기반 효과 처리 위임
            var areaEffectLogic = _statManager.GetEffectLogic(effectData.Type) as IMovableEventEffectLogic;
            areaEffectLogic?.EffectByMovableEvent(movableEvent, this, effectData, target);
        }
    }
    #endregion

    #region Funcs for SkillSystem
    /// <summary>
    /// 스킬 영역 업데이트 (비스택형 스킬 중첩 처리)
    /// - IsStackable이 false인 경우, 새 스킬과 겹치는 영역 제거
    /// - 축소된 영역의 타겟들에게 강제 OnCellExit 이벤트 발생
    /// </summary>
    /// <param name="newSkillArea">새로 생성된 스킬의 영역</param>
    public void UpdateArea(HashSet<Vector2Int> newSkillArea)
    {
        if (!IsStackable)
        {
            HashSet<Vector2Int> newArea = new HashSet<Vector2Int>(_area);

            // 겹치는 영역 제거
            newArea.ExceptWith(newSkillArea);

            //영역에서 나가진 타겟들에 대해 OnCellExit 이벤트 발생
            foreach (EffectData effectData in _effectDataList)
            {
                IBaseEffectLogic effectLogic = _statManager.GetEffectLogic(effectData.Type);
                if (effectLogic is not IMovableEventEffectLogic movableEventEffect)
                    continue;

                foreach(var target in FindEffectTargetList(effectData.Target))
                {
                    //새로워진 영역에 포함되어 있으면 무시
                    if (newArea.Contains(target.GetEffectable<BaseCharacter>().Position))
                        continue;

                    // 축소된 영역에 있던 타겟에게 강제 퇴출 이벤트
                    movableEventEffect.EffectByMovableEvent(MovableEvent.OnCellExit, this, effectData, target);
                }
            }
            _area = newSkillArea;
        }
    }
    #endregion

}
