using TMPro;
using UnityEngine;
using UnityEngine.UI;

//절차적으로 만들어진 맵에서 스테이지 이동을 관리하는 코드입니다
public class MapTempController : MonoBehaviour
{
    public StageSelect StageSelect;
    public TextMeshProUGUI ChapterText;

    private void Start()
    {
        //GenerateMap();
        StageSelect.InitStageSelect();
        if (ChapterText != null)
            ChapterText.text = $"Current Chapter: {StageManager.Instance.CurrentChapterLevel}";
    }

    public void OnClickRegenerate()
    {
        //Regenerate버튼이 시드를 수정합니다!!! 디버깅 시 조심하셔야 합니다!!!
        GameManager.Instance.RandSeed = Random.Range(int.MinValue, int.MaxValue);
        Debug.LogWarning("[Warning]디버그 주의MapTempController: Seed Updated - " + GameManager.Instance.RandSeed);
        //
        StageManager.Instance.CurrentPlayerPos = Vector3.zero;
        StageManager.Instance.CurStageId = StageManager.Instance.CurrentChapterLevel * 1000;
        GenerateMap();
    }

    public void OnClickNextChapter()
    {
        StageManager.Instance.CurrentChapterLevel++;
        if (StageManager.Instance.CurrentChapterLevel > 7) StageManager.Instance.CurrentChapterLevel = 1;

        GenerateMap();
    }

    public void OnClickClear()
    {
        StageSelect.ClearStage();
    }

    private void GenerateMap()
    {
        StageSelect.ClearStage();
        StageManager.Instance.InitChapterStart();
        StageSelect.InitStageSelect();
        if (ChapterText != null)
            ChapterText.text = $"Current Chapter: {StageManager.Instance.CurrentChapterLevel}";
    }
}