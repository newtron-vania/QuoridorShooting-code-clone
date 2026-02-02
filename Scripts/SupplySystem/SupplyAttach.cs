using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 보급품 상자 또는 적으로 부터 보급품이 떨어졌을 때 등급과 확률에 따른 계산
// 미리 이 보급품에 도달 했을 때 어떤 보급품을 줄지 정해둘 예정
public class SupplyAttach : MonoBehaviour
{
    public float StayTime = 0.5f; // 머무를 시간
    private float timer = 0f;
    private bool _isPlayerInside = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInside = true;
            timer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            _isPlayerInside = false;
            timer = 0f;
        }
    }

    private void Update()
    {
        if (_isPlayerInside)
        {
            timer += Time.deltaTime;
            if (timer >= StayTime)
            {
                UIManager.Instance.ShowPopupUI<SupplyShowPanelUI>();
                Destroy(gameObject);
            }
        }
    }
}
