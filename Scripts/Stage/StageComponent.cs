using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageComponent : MonoBehaviour
{
    private Stage _stage;

    private bool _canEnter;
    public bool CanEnter => _canEnter;

    private SpriteRenderer _image;

    private GameObject _clearedMark;


    //StageTest prefab 구조
    //--Image(SpriteRenderer)
    //--Mask(SpriteMask)
    //--ClearedMark(SpriteRenderer)

    //검은색 스프라이트라 사실 맨 뒤의 알파값만 사용해요.
    private readonly Color ColorLocked = new Color(0.3f, 0.3f, 0.3f, 0.3f); //흐릿하게
    private readonly Color ColorCleared = new Color(0.7f, 0.7f, 0.7f, 0.7f); // 약간 흐릿하게
    private readonly Color ColorNext = new Color(1f, 1f, 1f, 1f); // 원본 스프라이트 색상
    private Vector3 _beforeHoverOriginalScale;
    private static readonly Vector3 _hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    private void Awake()
    {
        _beforeHoverOriginalScale = transform.localScale;
    }

    public void Init(Stage stage, Sprite sprite, bool canEnter, bool isCurrent)
    {
        _stage = stage;
        _image = transform.Find("Image").GetComponent<SpriteRenderer>();
        _clearedMark = transform.Find("ClearedMark").gameObject;
        _canEnter = canEnter;
        //_clearedMark.SetActive(stage.IsCleared);
        if (isCurrent) // 1. 현재 위치
        {
            _image.color = ColorCleared;
            _clearedMark.SetActive(true);
        }
        else if (_canEnter && !_stage.IsCleared) // 2. 다음에 갈 수 있는 스테이지 (아직 안 깸)
        {
            _image.color = ColorNext;
            _clearedMark.SetActive(false);
        }
        else if (_stage.IsCleared) // 3. 깬 스테이지 (지나온 길)
        {
            _image.color = ColorCleared;
            _clearedMark.SetActive(true);
        }
        else // 4. 갈 수 없는 스테이지 (먼 미래 or 다른 루트)
        {
            _image.color = ColorLocked;
            _clearedMark.SetActive(false);
        }

        _image.sprite = sprite;
    }


    ///<summary>
    ///스테이지 선택 시 실행된 이벤트를 실행합니다.
    ///</summary>
    //ToDo: 씬이동은 여기서 진행합니다.
    public void OnTouchEvent()
    {
        //Debug.Log("[INFO] StageComponent.cs::OnTouchEvent - Clicked Stage Id: " + _stage.Id + ", Tile type is " + _stage.Type);
        if (!_canEnter)
            return;

    }

    public Stage GetStage()
    {
        return _stage;
    }

    public void OnHoverEnter()
    {
        if (_canEnter)
        {
            transform.localScale = _hoverScale;
        }
    }

    public void OnHoverExit()
    {
        transform.localScale = _beforeHoverOriginalScale;
    }
}
