using System;
using System.Collections.Generic;
using System.Linq;
using CharacterDefinition;
using HM.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

// Enemy Skill 사용 위치 판단용 클래스
public class SkillTargetSelector
{
    // 능력 사용 위치 선택 전략 정의
    private enum SkillTargetingStrategy
    {
        BaseSingleTargetSelect,            // HP가 가장 낮은 단일 적 유닛을 선택
        BaseAreaEffect,                    // 범위 내 모든 적 유닛을 선택
        BasePlacement,                     // 특정 위치에 설치 (미구현)
        SingleTargetSelectMindCotrol,      // 가장 가까운 적 유닛 선택 (마인드컨트롤)
        PlacementJelly                     // 아군 근처 특정 위치에 젤리 배치
    }

    private BattleSystem _controller;

    // 능력 ID별 전략 매핑
    private Dictionary<int, SkillTargetingStrategy> _strategyBySkillId = new Dictionary<int, SkillTargetingStrategy>();
    public SkillTargetSelector(BattleSystem controller)
    {
        _controller = controller;
        _strategyBySkillId.Add(11, SkillTargetingStrategy.SingleTargetSelectMindCotrol);
        _strategyBySkillId.Add(12, SkillTargetingStrategy.BaseAreaEffect);
        _strategyBySkillId.Add(13, SkillTargetingStrategy.BaseSingleTargetSelect);
        _strategyBySkillId.Add(14, SkillTargetingStrategy.PlacementJelly);
    }

    // 외부에서 능력 ID와 캐스터, 타겟 가능한 영역을 받아 위치 리스트 반환
    public Vector2Int GetSkillTargetPositions(int skillID, ISkillParticipant caster, List<Vector2Int> targetableZone)
    {
        var resultPositionList = new List<Vector2Int>();
        var strategy = _strategyBySkillId[skillID];
        
        return strategy switch
        {
            SkillTargetingStrategy.BaseSingleTargetSelect => GetSingleTargetSelectPositions(caster, targetableZone),
            SkillTargetingStrategy.BaseAreaEffect => GetZoneEffectPosition(caster, targetableZone),
            SkillTargetingStrategy.BasePlacement => GetPlacementPosition(caster, targetableZone),
            SkillTargetingStrategy.SingleTargetSelectMindCotrol => GetMindControlTargetPosition(caster, targetableZone),
            SkillTargetingStrategy.PlacementJelly => GetJellyPlacementPosition(caster, targetableZone),
            _ => caster.Position
        };
    }
    
    // 전략: HP가 가장 낮은 적 유닛을 찾아 단일 타겟으로 선택
    private Vector2Int GetSingleTargetSelectPositions(ISkillParticipant caster, List<Vector2Int> targetableZone)
    {
        // BaseSingleTargetSelect 전략 처리 로직
        Vector2Int resultTarget = caster.Position;
        var targetCandidates = _controller.StageCharacter[caster.Identification == CharacterIdentification.Player ? CharacterIdentification.Enemy : CharacterIdentification.Player];
        
        List<ISkillParticipant> candidates = targetCandidates
            .Where(candidate => targetableZone.Contains(candidate.Position))
            .Cast<ISkillParticipant>()
            .ToList();

        int lowestHp = Int32.MaxValue;
        foreach (var candidate in candidates)
        {
            if (candidate.CharacterStat.Hp < lowestHp)
            {
                lowestHp = candidate.CharacterStat.Hp;
                resultTarget = candidate.Position;
            }
        }
        
        return resultTarget;
    }

    // 전략: 타겟 가능한 영역 내에 있는 모든 적 유닛 위치 반환 (범위 공격)
    private Vector2Int GetZoneEffectPosition(ISkillParticipant caster, List<Vector2Int> targetableZone)
    {
        // BaseAreaEffect 전략 처리 로직
        var resultPosition = caster.Position;

        return resultPosition;
    }

    // 전략: 특정 위치에 오브젝트를 설치하는 전략 (아직 구현되지 않음)
    private Vector2Int GetPlacementPosition(ISkillParticipant caster, List<Vector2Int> targetableZone)
    {
        // BasePlacement 전략 처리 로직
        var resultPosition = caster.Position;
        
        
        return resultPosition;
    }

    // 전략: 가장 가까운 적 유닛을 선택하여 마인드컨트롤 대상 지정
    private Vector2Int GetMindControlTargetPosition(ISkillParticipant caster, List<Vector2Int> targetableZone)
    {
        // SingleTargetSelectMindCotrol 전략 처리 로직
        Vector2Int resultTarget = caster.Position;

        var targetCandidates = _controller.StageCharacter[
            caster.Identification == CharacterIdentification.Player
                ? CharacterIdentification.Enemy
                : CharacterIdentification.Player];
        
        List<ISkillParticipant> candidates = targetCandidates
            .Where(candidate => targetableZone.Contains(candidate.Position))
            .Cast<ISkillParticipant>()
            .ToList();
        
        
        float lowestDistance = Single.MaxValue;
        foreach (var candidate in candidates)
        {
            float distance = Mathf.Pow(caster.Position.x - candidate.Position.x, 2) + Mathf.Pow(caster.Position.y - candidate.Position.y, 2);
            if (distance < lowestDistance)
            {
                lowestDistance = distance;
                resultTarget = candidate.Position;
            }
        }
        
        return resultTarget ;
    }

    // 전략: 무작위 아군 캐릭터 주변 유효한 위치에 젤리 설치
    private Vector2Int GetJellyPlacementPosition(ISkillParticipant caster, List<Vector2Int> targetableZone)
    {
        // PlacementJelly 전략 처리 로직
        var resultPosition = caster.Position;

        var targetCandidates = _controller.StageCharacter[caster.Identification];

        int targetIndex = Random.Range(0, targetCandidates.Count);

        Vector2Int targetPosition = targetCandidates[targetIndex].Position;
        if (PathFindingUtils.IsPositionValidWithOffset(targetPosition + new Vector2Int(0, -1)))
        {
            targetPosition += new Vector2Int(0, -1);
        }
        
        resultPosition = targetPosition;
        
        return resultPosition;
    }
}