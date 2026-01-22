using UnityEngine;

public class MoveCommand : CharacterCommand
{
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    
    private Vector3 _velocity = Vector3.zero;

    private float _time = 0f;
    
    public MoveCommand(BaseCharacter character, Vector3 nextPosition)
    {
        CommandedCharacter = character;
        _startPosition = character.CharacterObject.transform.position;
        _targetPosition = nextPosition;
    }
    
    public override bool ExecuteSuccess()
    {
        if (Vector3.Distance(CommandedCharacter.CharacterObject.Rigidbody.position, _targetPosition) <= 0.01f)
        {
            CommandedCharacter.CharacterObject.Rigidbody.position = _targetPosition;
            return true;
        }

        return false;
    }
    
    public override bool Execute()
    {
        _time = Mathf.Min(1f, _time + Time.deltaTime);
        Vector3 position = CommandedCharacter.CharacterObject.Rigidbody.position;
        if (Vector3.Distance(CommandedCharacter.CharacterObject.Rigidbody.position, _targetPosition) < 0.01f)
        {
            CommandedCharacter.CharacterObject.Rigidbody.position = _targetPosition;
            return true;
        }
        else
        {
            CommandedCharacter.CharacterObject.Rigidbody.position =
                Vector3.MoveTowards(position, _targetPosition, _time);
            return false;
        }
    }

    public override bool Undo()
    {
        CommandedCharacter.CharacterObject.Rigidbody.position = _startPosition;
        return true;
    }
}