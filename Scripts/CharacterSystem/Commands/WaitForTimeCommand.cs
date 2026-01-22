using UnityEngine;

public class WaitForTimeCommand : CharacterCommand
{
    private readonly float inputTime;
    private float _waitingTime;

    public WaitForTimeCommand(float time)
    {
        inputTime = time;
        _waitingTime = inputTime;
    }

    public override bool ExecuteSuccess()
    {
        return _waitingTime <= 0f;
    }

    public override bool Execute()
    {
        _waitingTime -= Time.deltaTime;

        return Time.deltaTime <= 0f;
    }

    public override bool Undo()
    {
        _waitingTime = inputTime;
        return true;
    }
}