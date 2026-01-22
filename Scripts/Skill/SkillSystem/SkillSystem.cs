using CharacterDefinition;
using HM;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public partial class SkillSystem : MonoBehaviour, IEventListener
{
    private EffectSystem _effectSystem;

    //캐싱된 SkillData
    public ReadOnlyDictionary<int, SkillData> SkillDataDict { get; private set; }

    private LinkedList<SkillInstance> _skillIntanceList = new LinkedList<SkillInstance>();

    #region Init
    public void Init()
    {
        InitSkillData();

        //이벤트 등록?
        EventManager.Instance.AddEvent(HM.EventType.OnTurnEnd, this);

        //EffectSystem등록
        _effectSystem = GameObject.Find("EffectSystem").GetComponent<EffectSystem>();
    }

    public void InitSkillData()
    {
        Dictionary<int, SkillData> skillDataDict = new Dictionary<int, SkillData>();

        //어빌리티 데이터 캐싱 
        for(int i =1;i<=DataManager.Instance.SkillDataCount;i++)
        {
            var data = DataManager.Instance.GetSkillData(i);
            skillDataDict.Add(i, data);
        }

        SkillDataDict = new ReadOnlyDictionary<int, SkillData>(skillDataDict);
    }


    #endregion

    #region Funcs For Character
    //Skill 사용 전 필요한 기능
    //현재 사용가능한지 여부
    public bool CheckSkillActivable(int skillId, ISkillParticipant source)
    {
        bool isCostEnough = SkillDataDict[skillId].ApCost <= source.CharacterStat.Ap;
        bool isTargetable = GetTargetablePositionList(skillId, source.Position).Count > 0;
        return isCostEnough && isTargetable;
    }

    public bool CheckSkillCostEnough(int skillId, ISkillParticipant source)
    {
        return SkillDataDict[skillId].ApCost <= source.CharacterStat.Ap;
    }

    //능력 선택 범위 반환
    public List<Vector2Int> GetRangePositionList(int skillId, Vector2Int characterPos)
    {
        List<Vector2Int> posList = new List<Vector2Int>();

        Debug.Log($"[INFO] SkillSystem::GetRangePositionList RangeID: {SkillDataDict[skillId].TargetRangeId}, Counts: {DataManager.Instance.GetRangeData(SkillDataDict[skillId].TargetRangeId, true).Count} ,");

        foreach (var pos in DataManager.Instance.GetRangeData(SkillDataDict[skillId].TargetRangeId, true))
        {
            //벽 통과 검사 아마 외벽이랑 벽 검사
            bool[] wallCheck = HM.Physics.RayUtils.CheckWallRay(GameManager.ToWorldPosition(characterPos), pos);
            if (wallCheck[0] || wallCheck[1])
                continue;

            posList.Add(characterPos + pos);
        }

        return posList;
    }

    //능력 사용이 가능한 위치들을 반환
    public List<Vector2Int> GetTargetablePositionList(int skillId, Vector2Int characterPos)
    {
        List<Vector2Int> posList = new List<Vector2Int>();
        SkillData skillData = SkillDataDict[skillId];

        Dictionary<CharacterIdentification, List<BaseCharacter>> stageCharacterDict = GameManager.Instance.CharacterController.StageCharacter;


        //타겟타입이 셀프면 자기자신만 등록
        if(skillData.TargetType==SkillTargetType.Self)
        {
            posList.Add(characterPos);
        }
        //아니면 스킬 범위 검사
        else
        {
            foreach (var pos in DataManager.Instance.GetRangeData(skillData.TargetRangeId, true))
            {
                //벽 통과 검사 아마 외벽이랑 벽 검사
                bool[] wallCheck = HM.Physics.RayUtils.CheckWallRay(GameManager.ToWorldPosition(characterPos), pos);
                if (wallCheck[0] || wallCheck[1])
                    continue;

                switch (skillData.TargetType)
                {
                    case SkillTargetType.Ally:

                        //아군 캐릭터 위치만 추가
                        if (stageCharacterDict[CharacterIdentification.Player].Exists(c => c.Position == characterPos + pos))
                        {
                            posList.Add(characterPos + pos);
                        }

                        break;
                    case SkillTargetType.Enemy:

                        //적군 캐릭터 위치만 추가
                        if (stageCharacterDict[CharacterIdentification.Enemy].Exists(c => c.Position == characterPos + pos))
                        {
                            posList.Add(characterPos + pos);
                        }

                        break;

                    case SkillTargetType.All:

                        //모든 캐릭터 위치 추가
                        if (stageCharacterDict[CharacterIdentification.Player].Exists(c => c.Position == characterPos + pos) ||
                            stageCharacterDict[CharacterIdentification.Enemy].Exists(c => c.Position == characterPos + pos))
                        {
                            posList.Add(characterPos + pos);
                        }

                        break;
                    case SkillTargetType.Tile:

                        posList.Add(characterPos + pos);

                        break;
                }
            }
        }

            

        return posList;
    }

    //능력 효과 범위 위치 리스트 반환
    public List<Vector2Int> GetSkillZonePositionList(int skillId, Vector2Int skillPos)
    {
        List<Vector2Int> posList = new List<Vector2Int>();
        foreach (var pos in DataManager.Instance.GetRangeData(SkillDataDict[skillId].EffectRangeId, true))
        {
            posList.Add(skillPos + pos);
        }
        return posList;
    }

    //어빌리티 사용
    public void ExecuteSkill(int skillId, Vector2Int skillPos, BaseCharacter source)
    {
        SkillData skillData = SkillDataDict[skillId];

        //능력 인스턴스 생성
        SkillInstance newInstance = new SkillInstance(this,_effectSystem ,skillData, source, skillPos);//SkillInstanceMaker.CreateSkillInstance(skillData, source, skillPos);

        

        newInstance.InvokeInstanceEvent(EffectInstanceEvent.Start);
        
        //인스턴스 start 후 실행 해야 범위 공격이 보임;
        ShowSkillHit(newInstance);

        _skillIntanceList.AddLast(newInstance);

        source.CharacterStat.Ap -= skillData.ApCost;

    }


    #endregion

    #region Funcs for SkillInstance
    public void UpdateSkillInstanceArea(SkillInstance skillInstance)
    {
        if (skillInstance.IsStackable)
            return;

        LinkedListNode<SkillInstance> node = _skillIntanceList.First;
        while (node != null)
        {
            LinkedListNode<SkillInstance> nextNode = node.Next; // 다음 노드를 미리 저장
            if (node.Value != skillInstance)
            {
                node.Value.UpdateArea(skillInstance.Area);

                if (!node.Value.IsStackable)
                    ReturnSkillArea(node.Value, skillInstance.Area);

                if (node.Value.Area.Count == 0)
                {
                    _areaPrefabDict.Remove(node.Value);
                    _skillIntanceList.Remove(node);
                }
                node = nextNode; // 다음 노드로 이동
            }

        }
    }
    #endregion

    #region Game EventHandle 
    private void OnTurnEnd()
    {
        LinkedListNode<SkillInstance> node = _skillIntanceList.First;
        while (node != null)
        {
            LinkedListNode<SkillInstance> nextNode = node.Next; // 다음 노드를 미리 저장
            node.Value.OnGameEvent(HM.EventType.OnTurnEnd);
            if (node.Value.Duration < 0)
            {
                _areaPrefabDict[node.Value].ClearAll();
                _areaPrefabDict.Remove(node.Value);

                node.Value.InvokeInstanceEvent(EffectInstanceEvent.End);
                _skillIntanceList.Remove(node);
            }
            node = nextNode; // 다음 노드로 이동
        }
    }
    
    public void BindCharacterEvent(BaseCharacter character)
    {
        character.OnPositionChanged += OnMovableEvent;
    }
    public void OnMovableEvent(IMovable player)
    {
        LinkedListNode<SkillInstance> node = _skillIntanceList.First;
        while (node != null)
        {
            LinkedListNode<SkillInstance> nextNode = node.Next; // 다음 노드를 미리 저장
            node.Value.InvokeMovableEventEffect(player);
            node = nextNode; // 다음 노드로 이동
        }
    }

    public void OnEvent(HM.EventType eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case HM.EventType.OnTurnEnd:
                OnTurnEnd();
                break;
        }
    }
    #endregion
}