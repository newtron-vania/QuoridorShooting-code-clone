
using System.Collections.Generic;
using HM;
using UnityEngine;
using EventType = HM.EventType;


public class EnemyController : IEventListener
{
    private CharacterController _controller;

    private List<EnemyCharacter> EnemyList;

    private List<EnemyCharacter> SortedList;

    private int _selectedEnmeyIndex = 0;

    private bool isStart = false;

    public EnemyController(CharacterController controller)
    {
        _controller = controller;
        EventManager.Instance.AddEvent(EventType.OnTurnEnd, this);
        EnemyList = new();
        SortedList = new();
    }

    public void Sort()
    {
        for (int i = SortedList.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i);
            (SortedList[i], SortedList[rand]) = (SortedList[rand], SortedList[i]);
        }
    }

    public void Clear()
    {
        EnemyList.Clear();
        SortedList.Clear();
    }

    public void ResetAllEnemy()
    {
        _selectedEnmeyIndex = 0;
        foreach (var enemy in SortedList)
        {
            enemy.Reset();
        }
        SortedList.Clear();
        isStart = false;
    }

    public void Start()
    {
        isStart = true;
        SortedList.Clear();
        foreach (var enemyCharacter in EnemyList)
        {
            enemyCharacter.Start();
            SortedList.Add(enemyCharacter);
        }
        Sort();
    }


    public void Update()
    {
        if (!isStart)
        {
            Start();
            return;
        }

        // 인덱스가 리스트 범위를 벗어난 경우 처리
        if (_selectedEnmeyIndex >= SortedList.Count)
        {
            if (_controller.IsCommandQueueEmpty())
            {
                GameManager.Instance.EndTurn();
            }

            return; // 추가 실행 방지
        }

        // 정상 범위 내에서 처리
        BaseCharacter currentSelectBaseCharacter = SortedList[_selectedEnmeyIndex];
        currentSelectBaseCharacter.Update();

        if (currentSelectBaseCharacter.IsFinished)
        {
            _selectedEnmeyIndex++;
            if(_selectedEnmeyIndex < SortedList.Count ) Debug.Log($"[INFO] EnemyController::Update - next enemy {SortedList[_selectedEnmeyIndex].CharacterObject.transform.name} start!");
        }
    }


    public bool RegisterEnemy<T>(T character) where T : EnemyCharacter
    {
        Init();

        if (!EnemyList.Contains(character))
        {
            EnemyList.Add(character);
            return true;
        }

        Debug.Log($"That enemy {character.CharacterName} is already registered!");
        return false;
    }


    public bool RegisterEnemy<T>(List<T> characters) where T : EnemyCharacter
    {
        Init();

        foreach (var character in characters)
        {
            RegisterEnemy(character);
        }

        return true;
    }


    public void Init()
    {
        if (EnemyList == null) EnemyList = new();
    }

    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EventType.OnTurnEnd:
                if (GameManager.Instance.Turn % 2 == 0)
                {
                    ResetAllEnemy();
                    isStart = false;
                }
                break;
        }
    }
}