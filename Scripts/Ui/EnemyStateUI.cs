using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyStateUI : BaseUI
{
    public override UIName ID => UIName.EnemyStateUI;

    // 상태창 리스트
    private readonly List<EnemyStateBoxUI> _enemyStates = new List<EnemyStateBoxUI>();

    // 컨테이너 및 레이아웃 관련
    [SerializeField] private RectTransform _container; // 상태창 컨테이너
    [SerializeField] private float _spacing = 10f;     // 상태창 간격

    // 턴 정보 텍스트
    [SerializeField] private Text _turnText;

    // 애니메이션 관련
    [SerializeField] private float _animationDuration = 0.2f;
    private bool _isAnimating;

    // 정렬 필요 여부
    protected override bool IsSorting => true;

    public override void Init()
    {
        base.Init();

        // 컨테이너가 없을 경우 생성
        if (_container == null)
        {
            //_container = gameObject.GetOrAddComponent<RectTransform>();
            _container.SetParent(this.transform, false);
        }
    }

    // 적 상태창 추가.
    public void AddEnemyStateBox(EnemyStateBoxUI enemyStateBox)
    {
        if (enemyStateBox == null) return;

        _enemyStates.Add(enemyStateBox);

        // 상태창 컨테이너에 배치
        enemyStateBox.transform.SetParent(_container, false);

        // 레이아웃 업데이트
        UpdateLayout();
    }

    // 적 상태창 제거.
    public void RemoveEnemyStateBox(EnemyStateBoxUI enemyStateBox)
    {
        if (enemyStateBox == null || !_enemyStates.Contains(enemyStateBox)) return;

        _enemyStates.Remove(enemyStateBox);
        Destroy(enemyStateBox.gameObject);

        // 레이아웃 업데이트
        UpdateLayout();
    }

    // 모든 적 상태창 초기화.
    public void ClearAllStates()
    {
        foreach (var state in _enemyStates)
        {
            Destroy(state.gameObject);
        }
        _enemyStates.Clear();
    }

    // 상태창 레이아웃 업데이트.
    public void UpdateLayout()
    {
        if (_isAnimating) return;

        float yOffset = 0f;

        foreach (var state in _enemyStates)
        {
            var rect = state.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, yOffset);
            yOffset -= rect.sizeDelta.y + _spacing;
        }
    }

    // 적 상태창 정렬.
    public void SortEnemyStates()
    {
        // 정렬 로직 추가 가능 (예: 행동력 순서)
        _enemyStates.Sort((a, b) => b.CurrentAction.CompareTo(a.CurrentAction));

        // 정렬 애니메이션 실행
        StartCoroutine(AnimateLayout());
    }

    // 정렬 애니메이션 처리.
    private IEnumerator AnimateLayout()
    {
        _isAnimating = true;

        float yOffset = 0f;

        foreach (var state in _enemyStates)
        {
            var rect = state.GetComponent<RectTransform>();
            rect.DOAnchorPos(new Vector2(0, yOffset), _animationDuration);

            yOffset -= rect.sizeDelta.y + _spacing;
            yield return new WaitForSeconds(0.05f);
        }

        _isAnimating = false;
    }

    // 현재 턴 정보 업데이트.
    public void UpdateTurnInfo(int turn)
    {
        if (_turnText != null)
        {
            _turnText.text = $"Stage n / {turn}의 턴";
        }
    }
}
