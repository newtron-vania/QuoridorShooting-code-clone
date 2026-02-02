using CharacterDefinition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WallSetUI : BaseUI
{
    enum Buttons
    {
        YesButton,  // 벽건설 승인
        NoButton    // 벽건설 거부
    }
    protected override bool IsSorting => true;
    public override UIName ID => UIName.SetWallUI;

    public PlayerCharacter WallPlayerChacter;

    public void Start()
    {
        Init();
    }

    public override void Init()
    {
        Bind<Button>(typeof(Buttons));

        // 버튼 이벤트 연결
        GetButton((int)Buttons.YesButton).gameObject.BindEvent(OnClickYesButton);
        GetButton((int)Buttons.NoButton).gameObject.BindEvent(OnClickNoButton);
    }

    public void OnClickYesButton(PointerEventData data)
    {
        var controller = GameManager.Instance.BattleSystem;
        if (controller.PreviewWall.GetComponent<PreviewWall>().CanBuild && controller.PreviewWall.activeInHierarchy) //갇혀있거나 겹쳐있거나 비활성화 되어있지않다면
        {
            controller.SetWall(); // 벽 설치
            WallPlayerChacter.CharacterStat.Ap -= controller.NeededBuildPoint; // 행동력 소모
            GameManager.Instance.playerWallCount++; // 설치한 벽 개수 +1
            controller.PlayerControlStatus = PlayerControlStatus.None;
            WallPlayerChacter.ResetPreview();
        }
    }
    public void OnClickNoButton(PointerEventData data)
    {
        GameManager.Instance.BattleSystem.PlayerControlStatus = PlayerControlStatus.None;
        WallPlayerChacter.ResetPreview();
    }
}
