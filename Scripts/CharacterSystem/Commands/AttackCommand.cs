

using UnityEngine;

public class AttackCommand : CharacterCommand
{
    private BaseCharacter _target;

    private bool attackCheck = false;
    public AttackCommand(BaseCharacter character, BaseCharacter targetCharacter)
    {
        CommandedCharacter = character;
        _target = targetCharacter;
    }
    public override bool ExecuteSuccess()
    {
        // 애니메이션 적용 후 추후 구현 예정
        /*
         * 공격 애니메이션 실행 후 약 0.4초 뒤
         */
        return attackCheck;
    }

    public override bool Execute()
    {
        if (_target == null) return false;
        
        if (!attackCheck)
        {
            attackCheck = true;
        }
        return true;
    }

    public override bool Undo()
    {
        _target.TakeDamage(CommandedCharacter, -CommandedCharacter.Atk);
        return true;
    }
}