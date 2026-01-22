using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestRangeData : MonoBehaviour
{
    public Transform OriginPoint;  // 기준점
    public Transform ButtonsParent;
    public GameObject RangeImage;
    public GameObject RangeButton;
    public Toggle ForPlayerToggle;

    private DataManager _dataManager;

    void Start()
    {
        _dataManager = DataManager.Instance;
        MakeButtons();
    }

    private void MakeButtons()
    {
        for (int i = 0; i < _dataManager.RangesCount; i++)
        {
            Button btn = Instantiate(RangeButton, ButtonsParent).GetComponent<Button>();
            int rangeId = i;
            btn.onClick.AddListener(() => ShowRange(rangeId, true));
            btn.GetComponentInChildren<TMP_Text>().text = "Range ID : " + i;
        }
    }

    private void ShowRange(int rangeId, bool isMoveRange)
    {
        foreach(Transform item in OriginPoint)
        {
            Destroy(item.gameObject);
        }

        List<Vector2Int> range = _dataManager.GetRangeData(rangeId, ForPlayerToggle.isOn);

        foreach (var item in range)
        {
            GameObject obj = Instantiate(RangeImage, OriginPoint);
            obj.transform.localPosition = new Vector3(item.x, item.y, 0) * obj.GetComponent<RectTransform>().rect.width;
        }
    }
}
