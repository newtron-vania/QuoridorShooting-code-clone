using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class MapPlayer : MonoBehaviour
{
    private Vector3 _targetPos;
    public bool IsMoving => _isMoving;
    private bool _isMoving = false;
    private bool _isSceneLoading = false;
    // 미구현된 스테이지 이동 후에도 스테이지와 간선 색상 변경을 위한 이벤트입니다.
    public event Action Temp_StageProcessCompleted;
    // 미구현된 스테이지 문제가 전부 해소되면 변수와 함께 system도 제거해주세요.(StageSelect.cs)
    [SerializeField] private float _moveSpeed = 5.0f;


    public void Init(Vector3 startPos)
    {
        transform.position = startPos;
        _targetPos = startPos;
    }

    public void MovePlayerToNextStage(Vector3 targetPos)
    {
        _targetPos = targetPos;
        _isMoving = true;
    }

    public void EnterStage(Stage stage)
    {
        switch (stage.Type)
        {
            case Stage.StageType.Normal:
                Debug.Log("[INFO] MapPlayer::EnterStage - Entered Normal Battle Stage " + stage.Id);
                StageManager.Instance.CurrentPlayerPos = transform.position;
                StageManager.Instance.CompleteStage(stage.Id);
                
                //SceneManager.LoadScene("BattleScene");
                StartCoroutine(TempLoadSceneCoroutine("BattleScene"));
                break;

            case Stage.StageType.Elite:
                Debug.Log("[INFO] MapPlayer::EnterStage - Entered Elite Battle Stage " + stage.Id);
                StageManager.Instance.CurrentPlayerPos = transform.position;
                StageManager.Instance.CompleteStage(stage.Id);

                //SceneManager.LoadScene("BattleScene");
                StartCoroutine(TempLoadSceneCoroutine("BattleScene"));
                break;

            /*Myestery에 진입했으니 기획에 따라 랜덤으로 stage 타입을 가져옵니다!*/
            case Stage.StageType.Mystery:
                Debug.Log("[INFO] MapPlayer::EnterStage - Entered Mystery Stage " + stage.Id);
                Stage.StageType newType = StageManager.Instance.GetMysteryEventType();
                Debug.Log("[INFO] MapPlayer::EnterStage - Mystery Revealed: " + newType);
                stage.MysteryChangeType(newType);
                //바뀐 stage에 다시 진입합니다.
                EnterStage(stage);
                break;

            /*Myestery에서 Ambush가 나왔으니 normal+기습전투로 설정합니다.(CharacterController.CharacterInstantiator)*/
            /*ChallengeStageProbability에서 테스트를 위해 Ambush 확률을 100%로 설정해두었으니 잊지 마세요!!!*/
            case Stage.StageType.Ambush:
                Debug.Log("[INFO] MapPlayer::EnterStage - Entered Ambush Stage " + stage.Id);
                StageManager.Instance.CurrentPlayerPos = transform.position;
                StageManager.Instance.CompleteStage(stage.Id);

                //SceneManager.LoadScene("BattleScene");
                StartCoroutine(TempLoadSceneCoroutine("BattleScene"));
                break;

            default:
                Debug.LogWarning("[WARN] MapPlayer::EnterStage - 미구현된 스테이지에 진입하려 시도함 " + stage.Id);
                StageManager.Instance.CurrentPlayerPos = transform.position;
                StageManager.Instance.CompleteStage(stage.Id);
                // 미구현된 스테이지 이동 후에도 스테이지와 간선 색상 변경을 위한 이벤트입니다.
                Temp_StageProcessCompleted?.Invoke();
                // 미구현된 스테이지 문제가 전부 해소되면 변수와 함께 system도 제거해주세요.(StageSelect.cs)
                break;
        }
    }

    private void Update()
    {
        if (!_isMoving) return;

        // 현재 위치에서 목표 위치까지 부드럽게 이동하기 위해 사용 Lerp
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * _moveSpeed);

        if (Vector3.Distance(transform.position, _targetPos) < 0.01f)
        {
            transform.position = _targetPos;
            _isMoving = false;
            EnterStage(StageManager.Instance.StageDic[StageManager.Instance.CurStageId]);
        }
    }

    IEnumerator TempLoadSceneCoroutine(string sceneName)
    {
        _isSceneLoading = true;
        _isMoving = false;
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        op.allowSceneActivation = false;
        float timer = 0.0f;
        while (!op.isDone)
        {
            timer += Time.deltaTime;
            if (timer >= 1.5f)
            {
                op.allowSceneActivation = true;//씬 전환
            }
            yield return null;
        }
    }

    private void OnGUI()
    {
        if (!_isSceneLoading) return;
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        var style = new GUIStyle(GUI.skin.label) { fontSize = 60, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "로딩텍스트\n로딩바", style);
    }
}