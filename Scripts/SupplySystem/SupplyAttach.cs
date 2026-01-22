using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 보급품 상자 또는 적으로 부터 보급품이 떨어졌을 때 등급과 확률에 따른 계산
// 미리 이 보급품에 도달 했을 때 어떤 보급품을 줄지 정해둘 예정
public class SupplyAttach : MonoBehaviour
{
    // TODO : 만약에 맵 그래프 형태로 보급품을 저장 후 사용한다면 해당 함수 수정 필요
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Player")
        {
            UIManager.Instance.ShowPopupUI<SupplyShowPanelUI>();
            Destroy(gameObject);
        }
    }
}
