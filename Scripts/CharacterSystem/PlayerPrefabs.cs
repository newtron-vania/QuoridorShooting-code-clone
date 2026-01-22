using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefabs : MonoBehaviour
{
    public GameObject PlayerPreview; // 플레이어 위치 미리보기
    public GameObject AttackPreview;
    public GameObject AttackHighlight;
    public GameObject SkillPreview;
    public GameObject WallPreview; // 플레이어 설치벽 위치 미리보기
    public GameObject Wall; // 플레이어 설치벽
    public GameObject ActionUI;
    public GameObject RangeFrame; // 기물 이동 범위 및 공격 범위 담아두는 오브젝트
}
