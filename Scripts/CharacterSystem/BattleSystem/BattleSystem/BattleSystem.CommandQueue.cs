using System.Collections.Generic;
using UnityEngine;

public partial class BattleSystem
{
    // -----------------------------
    //  명령 큐 관련 변수
    // -----------------------------
    private readonly Queue<CharacterCommand> _commandQueue = new(); // 캐릭터 명령 큐
    [SerializeField] private int queueCount;
    
    public bool InsertCharacterCommand(CharacterCommand command)
    {
        if (_commandQueue == null || command == null) return false;

        _commandQueue.Enqueue(command);
        return true;
    }

    public bool IsCommandQueueEmpty()
    {
        return _commandQueue.Count <= 0;
    }

    private bool UpdateCommand()
    {
        if (_commandQueue.Count > 0)
        {
            CharacterCommand command = _commandQueue.Peek();
            command.Execute();
            if (command.ExecuteSuccess()) _commandQueue.Dequeue();
            return true;
        }

        return false;
    }
    
    private void OnUpdateQueue()
    {
        var currentQueueCount = _commandQueue.Count;
        if (queueCount != currentQueueCount)
            // Debug.Log($"[INFO] CharacterController::Update() - queueCount is {(queueCount < currentQueueCount ? "up" : "down")}");
            queueCount = currentQueueCount;
    }
}