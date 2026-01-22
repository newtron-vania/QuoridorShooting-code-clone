using System.Collections;
using System.Collections.Generic;
using UI.Define;
using UnityEngine;
using HM;

public class ToastPopUpQueue
{
    private Queue<ToastItemUI> _queue = new Queue<ToastItemUI>();

    public int Count
    {
        get { return _queue.Count; }
    }

    // 사이즈가 2가 넘어가면 false 아닐 시 true 토스트 팝업 갯수 조절
    public void Enqueue(ToastItemUI value)
    {
        _queue.Enqueue(value);
        if (_queue.Count > 2)
        {
            ToastItemUI item = _queue.Dequeue();
            if (item)
            {
                item.DeleteToast();
            }
            else
            {
                Debug.LogWarning("[WARN]ChallengeBattleUI.ToastPopUp - 이미 해당 토스트 팝업은 제거되었습니다 (접근 경고)");
            }
        }
    }

    public ToastItemUI Dequeue()
    {
        return _queue.Dequeue();
    }
}

public partial class ChallengeBattleUI : BaseUI
{
    private ToastPopUpQueue _toastPopUps = new ToastPopUpQueue();
    private GameObject _toastParent;
    // 이벤트 조건 확인
    private List<HM.EventType> _eventCondition = new List<HM.EventType>()
    {
        HM.EventType.OnSupplyGet,   
    };

    private void OnEventToastPopUp(HM.EventType eventType)
    {
        // 해당 이벤트가 토스트 팝업과 일치하는지 확인
        if (!_eventCondition.Contains(eventType)) return;

        ToastItemUI item = UIManager.Instance.MakeSubItem<ToastItemUI>(_toastParent.transform);
        item.ToastData = CreateToastData(eventType);
        _toastPopUps.Enqueue(item);
    }

    private ToastData CreateToastData(HM.EventType eventType)
    {
        ToastData data = new ToastData();
        switch(eventType)
        {
            case HM.EventType.OnSupplyGet:
                SupplymentData sup = DataManager.Instance.GetSupplyData(SupplyManager.Instance.SelectedSupplyItem);
                //data.Icon = sup.IconId;
                data.Description = sup.Name + "을 획득했습니다!";
                // 초기화
                SupplyManager.Instance.SelectedSupplyItem = 0;
                break;
        }
        return data;
    }

    private void ToastInit()
    {
        _toastParent = GetObject((int)GameObjects.ToastPopUpBoundary);
    }

    private void ToastUpdate()
    {
        // 이벤트 발생시 넣어줄 예정 (임시코드)
        if (Input.GetKeyDown(KeyCode.L))
        {
            _toastPopUps.Enqueue(UIManager.Instance.MakeSubItem<ToastItemUI>(GetObject((int)GameObjects.ToastPopUpBoundary).transform));
        }
    }
}
