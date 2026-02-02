using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬을 통한 순간이동 효과
/// TeleportCommand를 CommandQueue에 삽입하여 캐릭터를 즉시 이동시킵니다.
/// </summary>
public class SkillTeleportEffect : IBaseEffectLogic
{
    /// <summary>
    /// 스킬 시작 시 대상 캐릭터들을 스킬 위치로 순간이동
    /// </summary>
    public void EffectByEffectEvent(EffectEvent effectEvent, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        if (effectEvent != EffectEvent.Start)
            return;

        if (!(effectInstance is SkillInstance skillInstance))
        {
            Debug.LogError("[Error] SkillTeleportEffect::EffectByEffectEvent - EffectInstance is not SkillInstance!");
            return;
        }

        Vector2Int teleportPosition = skillInstance.SkillPosition;

        foreach (var targetProvider in targetList)
        {
            BaseCharacter character = targetProvider.GetEffectable<BaseCharacter>();
            if (character == null)
                continue;

            // 월드 좌표로 변환
            Vector3 targetWorldPos = GameManager.ToWorldPosition(teleportPosition);

            // TeleportCommand 생성 및 CommandQueue에 삽입
            TeleportCommand teleportCommand = new TeleportCommand(character, targetWorldPos);
            GameManager.Instance.BattleSystem.InsertCharacterCommand(teleportCommand);

            // Position 업데이트 (OnPositionChanged 이벤트 발생)
            character.Position = teleportPosition;

            Debug.Log($"[INFO] SkillTeleportEffect::EffectByEffectEvent - {character.CharacterName} teleported to {teleportPosition}");
        }
    }

    public void EffectByGameEvent(HM.EventType eventType, EffectInstance effectInstance, EffectData effectData, List<IEffectableProvider> targetList)
    {
        // 게임 이벤트에서는 실행하지 않음
    }
}
