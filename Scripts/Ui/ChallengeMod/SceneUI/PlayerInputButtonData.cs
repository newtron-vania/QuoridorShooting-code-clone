using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInputButtonData : MonoBehaviour
{
    public GameObject ButtonObject;
    public bool IsActive
    {
        get
        {
            return _isActive;
        }
        set
        {
            // 활성화 상태
            if (value)
            {
                gameObject.GetComponent<Image>().color = new Color(1, 1, 1);
            }
            // 비활성화 상태
            else
            {
                gameObject.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            }
            _isActive = value;
        }
    }
    private bool _isActive = true;

    private void Start()
    {
        ButtonObject = gameObject;
    }

    private void Update()
    {
        // 확정 버튼 처리
        if (gameObject.CompareTag("PlayerUIConfirm"))
        {
            IsActive = GameManager.Instance.CharacterController.IsConfirm;
        }
    }
}
