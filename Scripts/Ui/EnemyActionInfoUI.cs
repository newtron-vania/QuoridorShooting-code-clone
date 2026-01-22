using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyActionInfoUI : BaseUI
{
    public override UIName ID => UIName.EnemyActionInfoUI;

    // UI 레이아웃 관련 변수
    [SerializeField] private GameObject actionInfoPanel; // 메인 패널
    [SerializeField] private GameObject moveablePanel;  // 이동 범위 패널
    [SerializeField] private GameObject attackablePanel; // 공격 범위 패널

    // 포인트들의 부모 객체
    private RectTransform moveablePointsParent;
    private RectTransform attackablePointsParent;

    // 이동/공격 가능한 지점 정보
    private List<RectTransform> moveablePoints = new List<RectTransform>();
    private List<RectTransform> attackablePoints = new List<RectTransform>();

    // 색상 정의
    private Color moveableColor = new Color(0.4f, 0.8f, 1, 1); // 이동 가능한 범위 색상
    private Color attackableColor = new Color(1, 0.4f, 0.4f, 1); // 공격 가능한 범위 색상

    // 정렬 필요 여부
    protected override bool IsSorting => false;

    public override void Init()
    {
        base.Init();

        // 패널과 부모 객체 설정
        actionInfoPanel = new GameObject("EnemyActionInfoPanel");
        moveablePanel = new GameObject("MoveablePanel");
        attackablePanel = new GameObject("AttackablePanel");

        var actionInfoRT = actionInfoPanel.AddComponent<RectTransform>();
        var moveableRT = moveablePanel.AddComponent<RectTransform>();
        var attackableRT = attackablePanel.AddComponent<RectTransform>();

        actionInfoRT.sizeDelta = new Vector2(800, 600); // 메인 패널 크기
        moveableRT.sizeDelta = new Vector2(300, 300); // 이동 패널 크기
        attackableRT.sizeDelta = new Vector2(300, 300); // 공격 패널 크기

        // 패널 계층 구조 설정
        actionInfoPanel.transform.SetParent(this.transform);
        moveablePanel.transform.SetParent(actionInfoPanel.transform);
        attackablePanel.transform.SetParent(actionInfoPanel.transform);

        // 부모 객체 설정
        moveablePointsParent = moveablePanel.GetComponent<RectTransform>();
        attackablePointsParent = attackablePanel.GetComponent<RectTransform>();
    }

    // 이동/공격 가능한 포인트를 초기화
    public void InitializePoints(int moveableCount, int attackableCount)
    {
        ClearPoints();

        // 이동 가능한 포인트 생성
        for (int i = 0; i < moveableCount; i++)
        {
            var point = CreatePoint(moveableColor, moveablePointsParent);
            moveablePoints.Add(point);
        }

        // 공격 가능한 포인트 생성
        for (int i = 0; i < attackableCount; i++)
        {
            var point = CreatePoint(attackableColor, attackablePointsParent);
            attackablePoints.Add(point);
        }
    }

    // 포인트를 생성하는 메서드
    private RectTransform CreatePoint(Color color, Transform parent)
    {
        var point = new GameObject("Point").AddComponent<RectTransform>();
        point.transform.SetParent(parent, false);
        var image = point.gameObject.AddComponent<Image>();
        image.color = color;
        point.sizeDelta = new Vector2(20, 20); // 포인트 크기 설정
        return point;
    }

    // 포인트를 초기화
    private void ClearPoints()
    {
        foreach (var point in moveablePoints)
        {
            Destroy(point.gameObject);
        }
        foreach (var point in attackablePoints)
        {
            Destroy(point.gameObject);
        }
        moveablePoints.Clear();
        attackablePoints.Clear();
    }

    // 패널의 위치를 설정
    public void SetPanelPosition(Vector3 enemyPosition, bool isAbove)
    {
        // 메인 패널 위치 및 피벗 설정
        var actionInfoRT = actionInfoPanel.GetComponent<RectTransform>();
        actionInfoRT.position = Camera.main.WorldToScreenPoint(enemyPosition);

        if (isAbove)
        {
            actionInfoRT.pivot = new Vector2(0.5f, 1); // 상단 배치
        }
        else
        {
            actionInfoRT.pivot = new Vector2(0.5f, 0); // 하단 배치
        }
    }

    // 이동/공격 포인트를 배치
    public void SetPointsPositions(Vector2Int[] moveablePositions, Vector2Int[] attackablePositions, float panelSize)
    {
        // 이동 포인트 배치
        PositionPoints(moveablePoints, moveablePositions, moveablePointsParent, panelSize);

        // 공격 포인트 배치
        PositionPoints(attackablePoints, attackablePositions, attackablePointsParent, panelSize);
    }

    // 포인트를 위치에 배치
    private void PositionPoints(List<RectTransform> points, Vector2Int[] positions, Transform parent, float panelSize)
    {
        float gridSize = panelSize / Mathf.Max(positions.Length, 1); // 그리드 크기 계산

        for (int i = 0; i < points.Count; i++)
        {
            if (i < positions.Length)
            {
                points[i].anchoredPosition = new Vector2(positions[i].x * gridSize, positions[i].y * gridSize);
                points[i].gameObject.SetActive(true);
            }
            else
            {
                points[i].gameObject.SetActive(false);
            }
        }
    }

    // 패널을 활성화/비활성화
    public void SetActive(bool active)
    {
        actionInfoPanel.SetActive(active);
    }
}