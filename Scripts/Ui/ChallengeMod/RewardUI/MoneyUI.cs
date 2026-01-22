using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MoneyUI : MonoBehaviour
{
    [Header("------ UI ------")]
    [SerializeField] private TMP_Text _moneyText;

    [Header("----- Model ------")]
    [SerializeField] private TestInventory _inventory;

    private void Awake()
    {
        _inventory.OnMoneyChanged += UpdateMoney;
    }

    private void OnDestroy()
    {
        _inventory.OnMoneyChanged -= UpdateMoney;
    }

    public void UpdateMoney(int money)
    {
        _moneyText.text = string.Format("Money : {0}", money);
    }
}
