using UnityEngine;
using HM;
using EventType = HM.EventType;

/// <summary>
/// BattleSystem의 TurnStart/TurnEnd 이벤트를 중앙 집중식으로 관리
/// Iterator 패턴 기반 우선순위 큐로 실행 순서 제어
///
/// 사용법:
/// 1. BattleSystem.Start()에서 Initialize() 호출
/// 2. 각 시스템에서 RegisterTurnStartEvent/RegisterTurnEndEvent로 등록
/// 3. BattleSystem.Reset()에서 Clear() 호출
/// </summary>
public class BattleTurnController : IEventListener
{
    private BattleSystem _battleSystem;

    // 우선순위 큐 (Iterator 패턴)
    private PriorityEventQueue<ITurnEventHandler> _turnStartQueue;
    private PriorityEventQueue<ITurnEventHandler> _turnEndQueue;

    private bool _isInitialized = false;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="battleSystem">소유하는 BattleSystem</param>
    public BattleTurnController(BattleSystem battleSystem)
    {
        _battleSystem = battleSystem;
        _turnStartQueue = new PriorityEventQueue<ITurnEventHandler>();
        _turnEndQueue = new PriorityEventQueue<ITurnEventHandler>();

        Debug.Log("[INFO] BattleTurnController::Constructor - Created successfully");
    }

    /// <summary>
    /// 초기화 - EventManager에 구독
    /// BattleSystem.Start()에서 호출해야 함
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("[WARN] BattleTurnController::Initialize - Already initialized");
            return;
        }

        // EventManager에 TurnStart/TurnEnd 이벤트 구독
        EventManager.Instance.AddEvent(EventType.OnTurnStart, this);
        EventManager.Instance.AddEvent(EventType.OnTurnEnd, this);

        _isInitialized = true;
        Debug.Log("[INFO] BattleTurnController::Initialize - Initialized successfully");
    }

    /// <summary>
    /// TurnStart 이벤트 핸들러 등록
    /// </summary>
    /// <param name="handler">등록할 핸들러</param>
    /// <param name="priority">실행 우선순위 (낮을수록 먼저 실행, TurnEventPriority 상수 사용 권장)</param>
    public void RegisterTurnStartEvent(ITurnEventHandler handler, int priority)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[WARN] BattleTurnController::RegisterTurnStartEvent - Not initialized yet");
        }

        _turnStartQueue.Register(handler, priority);
    }

    /// <summary>
    /// TurnEnd 이벤트 핸들러 등록
    /// </summary>
    /// <param name="handler">등록할 핸들러</param>
    /// <param name="priority">실행 우선순위 (낮을수록 먼저 실행, TurnEventPriority 상수 사용 권장)</param>
    public void RegisterTurnEndEvent(ITurnEventHandler handler, int priority)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[WARN] BattleTurnController::RegisterTurnEndEvent - Not initialized yet");
        }

        _turnEndQueue.Register(handler, priority);
    }

    /// <summary>
    /// TurnStart 이벤트 핸들러 해제
    /// </summary>
    /// <param name="handler">해제할 핸들러</param>
    public void UnregisterTurnStartEvent(ITurnEventHandler handler)
    {
        _turnStartQueue.Unregister(handler);
    }

    /// <summary>
    /// TurnEnd 이벤트 핸들러 해제
    /// </summary>
    /// <param name="handler">해제할 핸들러</param>
    public void UnregisterTurnEndEvent(ITurnEventHandler handler)
    {
        _turnEndQueue.Unregister(handler);
    }

    /// <summary>
    /// EventManager에서 호출되는 이벤트 핸들러
    /// </summary>
    public void OnEvent(EventType eventType, Component sender, object param = null)
    {
        switch (eventType)
        {
            case EventType.OnTurnStart:
                ExecuteTurnStart();
                break;
            case EventType.OnTurnEnd:
                ExecuteTurnEnd();
                break;
        }
    }

    /// <summary>
    /// TurnStart 이벤트 실행 - Iterator 패턴으로 우선순위 순회
    /// </summary>
    private void ExecuteTurnStart()
    {
        Debug.Log($"[INFO] BattleTurnController::ExecuteTurnStart - Executing {_turnStartQueue.Count} handlers");

        int executedCount = 0;
        foreach (var handler in _turnStartQueue)
        {
            try
            {
                Debug.Log($"  → Executing: {handler.HandlerName}");
                handler.OnTurnStart();
                executedCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ERROR] BattleTurnController::ExecuteTurnStart - Handler '{handler.HandlerName}' threw exception: {e}");
                Debug.LogError($"  Stack Trace: {e.StackTrace}");
            }
        }

        Debug.Log($"[INFO] BattleTurnController::ExecuteTurnStart - Completed {executedCount}/{_turnStartQueue.Count} handlers");
    }

    /// <summary>
    /// TurnEnd 이벤트 실행 - Iterator 패턴으로 우선순위 순회
    /// </summary>
    private void ExecuteTurnEnd()
    {
        Debug.Log($"[INFO] BattleTurnController::ExecuteTurnEnd - Executing {_turnEndQueue.Count} handlers");

        int executedCount = 0;
        foreach (var handler in _turnEndQueue)
        {
            try
            {
                Debug.Log($"  → Executing: {handler.HandlerName}");
                handler.OnTurnEnd();
                executedCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ERROR] BattleTurnController::ExecuteTurnEnd - Handler '{handler.HandlerName}' threw exception: {e}");
                Debug.LogError($"  Stack Trace: {e.StackTrace}");
            }
        }

        Debug.Log($"[INFO] BattleTurnController::ExecuteTurnEnd - Completed {executedCount}/{_turnEndQueue.Count} handlers");
    }

    /// <summary>
    /// 정리 - 모든 이벤트 해제 및 EventManager 구독 해제
    /// BattleSystem.Reset()에서 호출해야 함
    /// </summary>
    public void Clear()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[WARN] BattleTurnController::Clear - Not initialized");
            return;
        }

        Debug.Log("[INFO] BattleTurnController::Clear - Clearing all handlers and unregistering from EventManager");

        // EventManager 구독 해제
        EventManager.Instance.RemoveEvent(EventType.OnTurnStart, this);
        EventManager.Instance.RemoveEvent(EventType.OnTurnEnd, this);

        // 모든 핸들러 제거
        _turnStartQueue.Clear();
        _turnEndQueue.Clear();

        _isInitialized = false;
        Debug.Log("[INFO] BattleTurnController::Clear - Cleared successfully");
    }

    /// <summary>
    /// 디버깅용 - 등록된 모든 핸들러 출력
    /// </summary>
    public void LogRegisteredHandlers()
    {
        Debug.Log("========== BattleTurnController - Registered Handlers ==========");
        _turnStartQueue.LogRegisteredHandlers("TurnStart Queue");
        _turnEndQueue.LogRegisteredHandlers("TurnEnd Queue");
        Debug.Log("================================================================");
    }

    /// <summary>
    /// 초기화 상태 확인
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// 등록된 TurnStart 핸들러 개수
    /// </summary>
    public int TurnStartHandlerCount => _turnStartQueue.Count;

    /// <summary>
    /// 등록된 TurnEnd 핸들러 개수
    /// </summary>
    public int TurnEndHandlerCount => _turnEndQueue.Count;
}
