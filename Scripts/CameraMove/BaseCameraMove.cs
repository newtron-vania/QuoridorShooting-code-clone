using UnityEngine;

//ToDo: 터치 유틸이 변경전 코드입니다.  
public class BaseCameraMove
{
    private float _zoomSpeed = 1000f;
    private float _maxCameraSize;
    public Camera MovingCamera;


    private Legacy.TouchUtil.ETouchState _touchState = Legacy.TouchUtil.ETouchState.None;
    private Vector2 _touchPosition = Vector2.zero;

    public Legacy.TouchUtil.ETouchState TouchState => _touchState;
    public Vector2 TouchPosition => _touchPosition;

    private Vector2 _touchBeforePosition = Vector2.zero;


    public void CameraUpdate()
    {
        _touchBeforePosition = _touchPosition;

        Legacy.TouchUtil.TouchSetUp(ref _touchState, ref _touchPosition); //이건 리턴이 훨씬 낫지않나요???

        Zoom();
        MoveCamera();

    }

    void Zoom()
    {
        // #if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            MovingCamera.orthographicSize += deltaMagnitudeDiff * 0.1f;
            MovingCamera.orthographicSize = Mathf.Clamp(MovingCamera.orthographicSize, 10f, _maxCameraSize);
        }
        // #else
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            MovingCamera.orthographicSize -= scroll * _zoomSpeed * Time.deltaTime;
            MovingCamera.orthographicSize = Mathf.Clamp(MovingCamera.orthographicSize, 10f, _maxCameraSize);
        }
        // #endif
    }


    void MoveCamera()
    {
        if (_touchState == Legacy.TouchUtil.ETouchState.Moved)
        {
            Vector2 touchDeltaPosition = (Vector2)Camera.main.ScreenToWorldPoint(_touchBeforePosition) - (Vector2)Camera.main.ScreenToWorldPoint(_touchPosition);
            Camera.main.transform.Translate(touchDeltaPosition);
        }
    }

    public void SetCamera(Vector2 startPos, float maxCameraSize)
    {
        MovingCamera = Camera.main;

        MovingCamera.transform.position = new Vector3(startPos.x, startPos.y, -10);
        _maxCameraSize = maxCameraSize;

    }



}
