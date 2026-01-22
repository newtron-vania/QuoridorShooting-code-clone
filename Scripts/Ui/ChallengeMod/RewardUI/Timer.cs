using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class Timer : MonoBehaviour
{
    [Header("----- Timer -----")]
    [SerializeField] private Image    _timerImage;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private float    _time;

    private Tween _tween;
    public event Action OnTimerEvent;

    public void SetTime(float time)
    {
        _time = time;
        SetTimeText(Mathf.CeilToInt(_time));
    }

    private void SetTimeText(int time)
    {
        _timerText.text = string.Format("{0:D}", time);
    }

    private void SetTimerBar(float value)
    {
        _timerImage.fillAmount = value;
    }

    private void OnEnable()
    {
        SetTimerBar(1f);
        SetTime(_time);

        StartTimer();
    }

    private void OnDisable()
    {
        _tween.Kill();
    }

    private void StartTimer()
    {
        int prevValue = -1;

        _tween = DOVirtual.Float(_time, 0f, _time, (value) =>
        {
            int currentInt = Mathf.CeilToInt(value);

            SetTimerBar(value / _time);

            if (currentInt != prevValue)
            {
                SetTimeText(currentInt);
                prevValue = currentInt;
            }
        })
        .SetEase(Ease.Linear)
        .OnComplete(() => OnTimerEvent?.Invoke());
    }
}
