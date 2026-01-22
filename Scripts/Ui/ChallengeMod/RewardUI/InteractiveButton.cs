using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractiveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _hoverSize    = 1.1f;
    [SerializeField] private float _clickSize    = 0.9f;
    [SerializeField] private float _durationTime = 0.1f;
    [SerializeField] private Ease  _ease         = Ease.Linear;

    private RectTransform _rect;
    private Vector3       _defaultSize;

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();

        _defaultSize = _rect.localScale;
    }

    //마우스 올릴때
    public void OnPointerEnter(PointerEventData eventData)
    {
        _rect.DOScale(_defaultSize * _hoverSize, _durationTime).SetEase(_ease);
    }

    //마우스 내릴때
    public void OnPointerExit(PointerEventData eventData)
    {
        _rect.DOScale(_defaultSize, _durationTime).SetEase(_ease);
    }

    //마우스 클릭 시
    public void OnPointerDown(PointerEventData eventData)
    {
        _rect.DOScale(_defaultSize * _clickSize, _durationTime).SetEase(_ease);
    }

    //마우스 클릭 종료
    public void OnPointerUp(PointerEventData eventData)
    {
        _rect.DOScale(_defaultSize, _durationTime).SetEase(_ease);
    }
}
