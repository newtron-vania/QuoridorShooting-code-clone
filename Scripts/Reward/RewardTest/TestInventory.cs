using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class TestInventory : MonoBehaviour
{
    [SerializeField] private int _money = 0;

    public event Action<int> OnMoneyChanged;

    private void Start()
    {
        UpdateMoney(_money);
    }

    public void UpdateMoney(int value)
    {
        _money += value;
        OnMoneyChanged?.Invoke(_money);
    }
}
