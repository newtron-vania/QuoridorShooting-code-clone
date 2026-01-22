using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using CharacterDefinition;

// 기물 정보 보여주는 UI 관리 (CharacterInfo)
public partial class ChallengeBattleUI : BaseUI
{
    private List<CharInfoItemUI> _charItems = new List<CharInfoItemUI>();

    private bool _isPlayerCharacterPanel = true;

    private void CharacterInfoInit()
    {
        GetButton((int)Buttons.SelectEnemyButton).gameObject.BindEvent(OnClickSelectEnemyInfoButton);
        GetButton((int)Buttons.SelectPlayerButton).gameObject.BindEvent(OnClickSelectPlayerInfoButton);
        ConvertCharacterInfoPanel(true);
        _isPlayerCharacterPanel = true;
    }

    // 캐릭터 selectButton 클릭
    public void OnClickSelectPlayerInfoButton(PointerEventData data)
    {
        ConvertCharacterInfoPanel(true);
        _isPlayerCharacterPanel = true;
    }
    public void OnClickSelectEnemyInfoButton(PointerEventData data)
    {
        ConvertCharacterInfoPanel(false);
        _isPlayerCharacterPanel = false;
    }

    public void CharInfoSelectHighLighting()
    {
        foreach(CharInfoItemUI item in _charItems)
        {
            if(item.ItemBaseCharacter == _currentPlayerCharacter)
            {
                item.HighLighting();
            }
        }
    }

    public void CharInfoActionHighlighting()
    {
        foreach (CharInfoItemUI item in _charItems)
        {
            if (item.ItemBaseCharacter.CanBuild)
            {
                item.FullActionHighlighting();
            }
        }
    }

    #region OnClickSelectInfoButtonFunc
    // 캐릭터 info panel 오브젝트 변경
    public void ConvertCharacterInfoPanel(bool isPlayerTeam)
    {
        UIManager.Instance.CloseUI(UIName.CharDetailFrameUI);
        UIManager.Instance.CloseAllGroupUI(UIName.CharInfoHighLighting);
        GameObject parent = GetObject((int)GameObjects.CharInfoBackGround);
        List<BaseCharacter> charData = new List<BaseCharacter>();
        _charItems.Clear();
        // 초기화
        foreach (Transform child in parent.transform)
        {
            ResourceManager.Instance.Destroy(child.gameObject);
        }

        // 생성 flag : true - 아군 / :false - 적군
        if (isPlayerTeam)
        {
            charData = GameManager.Instance.CharacterController.StageCharacter[CharacterIdentification.Player];
            foreach (BaseCharacter child in charData)
            {
                GameObject item = UIManager.Instance.MakeSubItem<CharInfoItemUI>(parent.transform).gameObject;
                // Frame에 데이터 넣기
                CharInfoItemUI infoItem = item.GetOrAddComponent<CharInfoItemUI>();
                _charItems.Add(infoItem);
                infoItem.ItemBaseCharacter = child;
                infoItem.IsPlayer = true;
                infoItem.DetailParent = GetObject((int)GameObjects.CharDetailBoundary).transform;
            }
        }
        else
        {
            charData = GameManager.Instance.CharacterController.StageCharacter[CharacterIdentification.Enemy];
            foreach (BaseCharacter child in charData)
            {
                GameObject item = UIManager.Instance.MakeSubItem<CharInfoItemUI>(parent.transform).gameObject;
                CharInfoItemUI infoItem = item.GetOrAddComponent<CharInfoItemUI>();
                _charItems.Add(infoItem);
                infoItem.ItemBaseCharacter = child;
                infoItem.IsPlayer = false;
                infoItem.DetailParent = GetObject((int)GameObjects.CharDetailBoundary).transform;
            }
        }
    }
    #endregion
}