using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropMoneyUI : MonoBehaviour, IPointerDownHandler
{
    [Header("------ UI -------")]
    [SerializeField] private GameObject  _panel;
    [SerializeField] private int         _money;
    [SerializeField] private TMP_Text    _moneyText;

    [Header("----- Model ------")]
    [SerializeField] private TestInventory _inventory;
    [SerializeField] private RewardPool    _rewardPool;

    [Header("----- Timer ------")]
    [SerializeField] private Timer _timer;
    [SerializeField] private float _time;

    private void Awake()
    {
        _timer.OnTimerEvent += ClickEvent;
    }

    private void OnDestroy()
    {
        _timer.OnTimerEvent -= ClickEvent;
    }

    public void OpenUI()
    {
        _rewardPool.GetDropMoneyInstance(ref _money, ref _time);
        SetMoney(_money);
        _timer.SetTime(_time);

        _panel.SetActive(true);
    }

    public void CloseUI()
    {
        _panel.SetActive(false);
    }

    public void SetMoney(int money)
    {
        _money = money;
        _moneyText.text = money.ToString();
    }

    private void ClickEvent()
    {
        _inventory.UpdateMoney(_money);
        CloseUI();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ClickEvent();
    }
}
