using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EnemyStateBoxUI : BaseUI
{
    public override UIName ID => UIName.EnemyStateBoxUI;


    // 상태 UI 요소
    [SerializeField] private Image _enemyIcon;     // 적 아이콘
    [SerializeField] private Text _actionText;    // 행동력 텍스트
    [SerializeField] private Text _healthText;    // 체력 텍스트
    [SerializeField] private Image _highlightImage; // 하이라이트 이미지

    // 적 상태 정보
    public int CurrentAction { get; private set; }
    public int MaxAction { get; private set; } = 10; // 기본 최대 행동력
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; } = 100; // 기본 최대 체력
    
    protected override bool IsSorting => true;
    
    public override void Init()
    {
        base.Init();
        ResetState();
    }

    // 적 상태를 설정합니다.
    public void SetEnemyState(Sprite icon, int currentAction, int maxAction, int currentHealth, int maxHealth)
    {
        CurrentAction = currentAction;
        MaxAction = maxAction;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;

        UpdateUI(icon);
    }
    
    // 행동력을 업데이트합니다.
    public void UpdateAction(int newAction)
    {
        CurrentAction = Mathf.Clamp(newAction, 0, MaxAction);
        UpdateActionText();
    }

    // 체력을 업데이트합니다.
    public void UpdateHealth(int newHealth)
    {
        CurrentHealth = Mathf.Clamp(newHealth, 0, MaxHealth);
        UpdateHealthText();

        if (CurrentHealth <= 0)
        {
            OnEnemyDefeated();
        }
    }

    // 적 상태를 강조(하이라이트)합니다.
    public void HighlightState()
    {
        if (_highlightImage != null)
        {
            _highlightImage.gameObject.SetActive(true);
            _highlightImage.color = Color.yellow;
        }
    }

    // 하이라이트를 초기화합니다.
    public void ResetHighlight()
    {
        if (_highlightImage != null)
        {
            _highlightImage.gameObject.SetActive(false);
        }
    }

    // 적 상태를 초기화합니다.
    public void ResetState()
    {
        CurrentAction = 0;
        CurrentHealth = 0;
        UpdateUI(null);
    }

    // 적 사망 시 처리.
    private void OnEnemyDefeated()
    {
        _healthText.color = Color.red;
        // 추가적인 사망 처리 효과는 여기에 구현
    }

    /// UI 업데이트: 모든 텍스트 및 아이콘
    private void UpdateUI(Sprite icon)
    {
        _enemyIcon.sprite = icon;
        UpdateActionText();
        UpdateHealthText();
    }

    // 행동력 텍스트 업데이트.
    private void UpdateActionText()
    {
        _actionText.text = $"행동력 {CurrentAction} / {MaxAction}";
    }

    // 체력 텍스트 업데이트.
    private void UpdateHealthText()
    {
        _healthText.text = $"체력 {CurrentHealth} / {MaxHealth}";
        _healthText.color = Color.white; // 초기 상태로 리셋
    }
}
