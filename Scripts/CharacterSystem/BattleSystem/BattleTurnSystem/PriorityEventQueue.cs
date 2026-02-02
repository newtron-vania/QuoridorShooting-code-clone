using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 우선순위 기반 이벤트 큐 (Iterator 패턴 구현)
/// 낮은 우선순위 값일수록 먼저 실행됨 (오름차순 정렬)
/// </summary>
/// <typeparam name="T">이벤트 핸들러 타입</typeparam>
public class PriorityEventQueue<T> : IEnumerable<T> where T : class
{
    /// <summary>
    /// 우선순위와 핸들러를 함께 저장하는 내부 엔트리 클래스
    /// </summary>
    private class PriorityEntry : IComparable<PriorityEntry>
    {
        public T Handler { get; }
        public int Priority { get; }

        public PriorityEntry(T handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }

        /// <summary>
        /// 우선순위 기반 비교 (오름차순 정렬)
        /// </summary>
        public int CompareTo(PriorityEntry other)
        {
            if (other == null) return 1;
            return Priority.CompareTo(other.Priority);
        }
    }

    private List<PriorityEntry> _entries = new List<PriorityEntry>();

    /// <summary>
    /// 등록된 핸들러 개수
    /// </summary>
    public int Count => _entries.Count;

    /// <summary>
    /// 핸들러 등록 (우선순위 순으로 자동 정렬)
    /// </summary>
    /// <param name="handler">등록할 핸들러</param>
    /// <param name="priority">실행 우선순위 (낮을수록 먼저 실행)</param>
    public void Register(T handler, int priority)
    {
        if (handler == null)
        {
            Debug.LogError("[ERROR] PriorityEventQueue::Register - Handler cannot be null");
            return;
        }

        // 중복 등록 방지
        if (_entries.Exists(e => e.Handler == handler))
        {
            Debug.LogWarning($"[WARN] PriorityEventQueue::Register - Handler already registered: {handler}");
            return;
        }

        var entry = new PriorityEntry(handler, priority);

        // 이진 탐색으로 삽입 위치 찾기 (정렬 유지)
        int index = _entries.BinarySearch(entry);
        if (index < 0) index = ~index; // 음수면 비트 반전으로 삽입 위치 계산

        _entries.Insert(index, entry);

        Debug.Log($"[INFO] PriorityEventQueue::Register - Handler: {handler}, Priority: {priority}, Index: {index}");
    }

    /// <summary>
    /// 핸들러 등록 해제
    /// </summary>
    /// <param name="handler">해제할 핸들러</param>
    public void Unregister(T handler)
    {
        if (handler == null)
        {
            Debug.LogError("[ERROR] PriorityEventQueue::Unregister - Handler cannot be null");
            return;
        }

        int removed = _entries.RemoveAll(e => e.Handler == handler);

        if (removed > 0)
        {
            Debug.Log($"[INFO] PriorityEventQueue::Unregister - Handler: {handler}, Removed: {removed}");
        }
        else
        {
            Debug.LogWarning($"[WARN] PriorityEventQueue::Unregister - Handler not found: {handler}");
        }
    }

    /// <summary>
    /// 모든 핸들러 제거
    /// </summary>
    public void Clear()
    {
        int count = _entries.Count;
        _entries.Clear();
        Debug.Log($"[INFO] PriorityEventQueue::Clear - Cleared {count} handlers");
    }

    /// <summary>
    /// Iterator 패턴 구현 - 우선순위 순으로 순회
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        foreach (var entry in _entries)
        {
            yield return entry.Handler;
        }
    }

    /// <summary>
    /// Non-generic IEnumerable 구현
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// 디버깅용 - 등록된 모든 핸들러와 우선순위 출력
    /// </summary>
    /// <param name="queueName">큐 이름 (로그 출력용)</param>
    public void LogRegisteredHandlers(string queueName)
    {
        Debug.Log($"[DEBUG] {queueName} - Registered Handlers: {Count}");

        if (Count == 0)
        {
            Debug.Log("  - (No handlers registered)");
            return;
        }

        foreach (var entry in _entries)
        {
            Debug.Log($"  - Priority {entry.Priority}: {entry.Handler}");
        }
    }

    /// <summary>
    /// 특정 핸들러가 등록되어 있는지 확인
    /// </summary>
    /// <param name="handler">확인할 핸들러</param>
    /// <returns>등록되어 있으면 true</returns>
    public bool Contains(T handler)
    {
        if (handler == null) return false;
        return _entries.Exists(e => e.Handler == handler);
    }
}
