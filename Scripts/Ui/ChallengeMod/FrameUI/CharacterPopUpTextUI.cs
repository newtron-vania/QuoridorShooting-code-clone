using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterPopUpTextUI : BaseUI
{
    protected override bool IsSorting => false;
    public override UIName ID => UIName.CharacterPopUpTextUI;

    private TMP_Text _myText;
    private InTokenTextMaterialData _myTokenData;
    private Coroutine _myCoroutine;

    public int PopUpType = 0;
    public int Value = 0;

    void Start()
    {
        Init();
    }

    public override void Init()
    {
        _myText = gameObject.GetComponent<TMP_Text>();
        _myTokenData = gameObject.GetComponent<InTokenTextMaterialData>();
        _myCoroutine = StartCoroutine(OnCharacterDamageAction(PopUpType, Value));
    }

    public void DeleteItem()
    {
        if (_myCoroutine != null)
        {
            StopCoroutine(_myCoroutine);
        }
        DestroyImmediate(gameObject);
    }

    // 0 : 회복 / 1 : 감소 / 2 : 회피
    public IEnumerator OnCharacterDamageAction(int actionNumber, int value)
    {
        _myText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        switch (actionNumber)
        {
            case 0:
                _myText.text = "+" + value;
                _myText.color = new Color(1, 1, 1);
                Debug.Log("[INFO]PlayerActionUI(OnCharacterDamageAction) - 캐릭터 회복");
                _myText.fontMaterial = _myTokenData.mats[0];
                break;
            case 1:
                _myText.text = "" + value;
                _myText.color = new Color(0.282353f, 0.282353f, 0.282353f);
                _myText.fontMaterial = _myTokenData.mats[1];
                Debug.Log("[INFO]PlayerActionUI(OnCharacterDamageAction) - 캐릭터 피해");
                break;
            case 2:
                _myText.text = "회피";
                _myText.color = new Color(0, 0, 0);
                _myText.fontMaterial = _myTokenData.mats[0];
                Debug.Log("[INFO]PlayerActionUI(OnCharacterDamageAction) - 캐릭터 회피");
                break;
            default:
                Debug.LogWarning("[WARN]PlayerActionUI(OnCharacterDamageAction) - 올바르지 않은 actionNumber가 할당되었습니다.");
                break;
        }
        _myText.GetComponent<RectTransform>().DOAnchorPosY(0.5f, 0.8f).SetEase(Ease.Linear);
        _myText.DOFade(1, 0.3f).SetEase(Ease.Linear);
        yield return new WaitForSeconds(0.5f);
        _myText.DOFade(0, 0.3f).SetEase(Ease.Linear);
        DestroyImmediate(gameObject);
    }
}
