using UnityEngine;

/// <summary>
/// 순간이동 방식의 캐릭터 이동 Command
/// 애니메이션 없이 즉시 위치를 변경합니다. (스킬, 넉백, 슬라임 미끄러짐 등)
/// </summary>
public class TeleportCommand : CharacterCommand
{
    private Vector3 _startPosition;
    private Vector3 _targetPosition;

    public TeleportCommand(BaseCharacter character, Vector3 targetPosition)
    {
        CommandedCharacter = character;
        _startPosition = character.CharacterObject.transform.position;
        _targetPosition = targetPosition;
    }

    public override bool ExecuteSuccess()
    {
        // 위치가 정확히 목표 지점에 도달했는지 확인
        return Vector3.Distance(CommandedCharacter.CharacterObject.Rigidbody.position, _targetPosition) <= 0.01f;
    }

    public override bool Execute()
    {
        // 즉시 위치 변경 (순간이동)
        CommandedCharacter.CharacterObject.Rigidbody.position = _targetPosition;

        // 한 프레임에 완료
        return true;
    }

    public override bool Undo()
    {
        // Undo 시 원래 위치로 즉시 복귀
        CommandedCharacter.CharacterObject.Rigidbody.position = _startPosition;
        return true;
    }
}
