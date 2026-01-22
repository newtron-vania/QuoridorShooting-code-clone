using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageComponent : MonoBehaviour
{
    private Stage _stage;

    private bool _canEnter;

    private SpriteRenderer _image;

    private GameObject _clearedMark;

    public void Init(Stage stage, Sprite sprite, bool canEnter)
    {

        _stage = stage;

        _image = transform.Find("Image").GetComponent<SpriteRenderer>();

        _clearedMark = transform.Find("ClearedMark").gameObject;
        _clearedMark.SetActive(stage.IsCleared);

        _canEnter = canEnter;
        if (!_canEnter && !_stage.IsCleared)
        {
            _image.color = new Color(1f, 1f, 1f, 0.3f);

        }

        _image.sprite = sprite;
    }

    
    ///<summary>
    ///스테이지 선택 시 실행된 이벤트를 실행합니다.
    ///</summary>   
    //ToDo: 씬이동은 여기서 진행합니다.
    public void OnTouchEvent()
    {
        Debug.Log("Stage Cliced Id: " + _stage.Id);
        if (!_canEnter)
            return;

    }
}
