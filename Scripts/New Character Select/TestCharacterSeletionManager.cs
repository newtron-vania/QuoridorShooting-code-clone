using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TestCharacterSeletionManager : MonoBehaviour
{
    // 프리팹
    public GameObject CharacterSlotPrefab;
    public GameObject SelectedCharacterSlotPrefab;

    // 에디터에서 넣어줄 변수(패널/스크롤뷰)
    public Transform InfoPanel;                   // 캐릭터 정보 패널
    public GameObject SelectedCharacterPanel;      // 선택된 캐릭터 패널
    public GameObject CharacterList;              // 캐럭터 리스트 스크롤 뷰의 content
    public GameObject PartyMaxCautionPanel;       // 파티원 초과 경고 패널

    // 에디터에서 넣어줄 변수(캐릭터 정보창)
    public TMP_Text CharacterNameText;
    public Button BackButton;                      // 뒤로가기 버튼(캐릭터 정보창 닫기)
    public Button JoinOrLeaveButton;               // 파티 참가/해제 버튼
    public Button OkButton;                        // 파티 초과 경고창 - 확인 버튼

    public Button SaveButton;                      // 파티 구성 저장 버튼

    // 파티 참가/해제 버튼 텍스트에 들어갈 문자열
    private const string JOIN_TEXT = "파티 참가";
    private const string LEAVE_TEXT = "파티 해제";

    // 캐릭터 슬롯 활성화/비활성화 색상
    private Color _activeSlotColor = Color.white;
    private Color _deactiveSlotColor = Color.gray;

    private int _selectedCharacterCount;           // 캐릭터 몇명 선택했는지
    private CharacterData _currentCharacter;            // 현재 선택한 캐릭터의 정보

    private List<int> _partyList = new List<int>();

    private void Start()
    {
        //데이터 불러오기
        InfoPanel.gameObject.SetActive(false);
        //_partyList = DataManager.Instance.GetPartyMembers();
        //_selectedCharacterCount = DataManager.Instance.GetPartyMembers().Count;

        CreateSlots();

        // 저장되어있는 파티 구성 반영
        if (_partyList.Count != 0)
        {
            ChangeSelectSlot();
            foreach (int id in _partyList)
            {
                ChangeColorCharacterSlot(id, false);
            }
        }

        // 버튼 클릭 이벤트 할당
        OkButton.onClick.AddListener(() => ClosePartyMaxCaution());
        BackButton.onClick.AddListener(() => CloseCharacterInfo());
        JoinOrLeaveButton.onClick.AddListener(() => JoinOrLeaveParty());
        SaveButton.onClick.AddListener(SaveParty);
    }

    // 캐릭터/파티 슬롯 생성
    private void CreateSlots()
    {
        // 캐릭터 슬롯 생성
        for (int i = 0; i < DataManager.Instance.PlayableCharacterCount; i++)
        {
            GameObject slot = Instantiate(CharacterSlotPrefab);
            slot.transform.SetParent(CharacterList.transform);
            slot.GetComponent<RectTransform>().localScale = Vector3.one;

            CharacterData character = DataManager.Instance.GetPlayableCharacterInfo(i);
            slot.GetComponentInChildren<TMP_Text>().text = character.Name;
            slot.GetComponent<Button>().onClick.AddListener(() => ActiveCharacterInfo(character));
        }

        // 파티 슬롯 생성
        for (int i = 0; i < DataManager.Instance.PartyMaxComposiitionNum; i++)
        {
            GameObject slot = Instantiate(SelectedCharacterSlotPrefab);
            slot.transform.SetParent(SelectedCharacterPanel.transform);
            slot.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    // 캐릭터 정보창 활성화 (선택창)
    private void ActiveCharacterInfo(CharacterData character)
    {
        InfoPanel.gameObject.SetActive(true);

        CharacterNameText.text = character.Name;

        JoinOrLeaveButton.GetComponentInChildren<TMP_Text>().text = (_partyList.Contains(character.Index) ? LEAVE_TEXT : JOIN_TEXT);

        _currentCharacter = character;
    }

    // 캐릭터 정보창 닫음
    private void CloseCharacterInfo()
    {
        InfoPanel.gameObject.SetActive(false);
    }

    // 파티 초과 경고창 닫음
    private void ClosePartyMaxCaution()
    {
        PartyMaxCautionPanel.SetActive(false);
    }

    // 캐릭터 파티 추가/추방
    private void JoinOrLeaveParty()
    {
        // 파티 추방
        if (_partyList.Contains(_currentCharacter.Index))
        {
            _partyList.Remove(_currentCharacter.Index);
            ChangeColorCharacterSlot(_currentCharacter.Index, true);
            _selectedCharacterCount--;
        }

        // 파티 인원 초과 경고
        else if (_selectedCharacterCount == DataManager.Instance.PartyMaxComposiitionNum)
        {
            PartyMaxCautionPanel.SetActive(true);
            return;
        }

        // 파티 참가
        else
        {
            _partyList.Add(_currentCharacter.Index);
            ChangeColorCharacterSlot(_currentCharacter.Index, false);
            _selectedCharacterCount++;
        }

        CloseCharacterInfo();
        ChangeSelectSlot();
    }

    // 파티원 변경에 따른 파티원 슬롯 변경
    private void ChangeSelectSlot()
    {
        for (int i = 0; i < DataManager.Instance.PartyMaxComposiitionNum; i++)
        {
            if (i < _partyList.Count)
            {
                SelectedCharacterPanel.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text = DataManager.Instance.GetPlayableCharacterInfo(_partyList[i]).Name;
            }
            else
            {
                SelectedCharacterPanel.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text = "";
            }
        }
    }

    // 캐릭터 슬롯 이미지 색상 교체
    private void ChangeColorCharacterSlot(int characterNum, bool isActive)
    {
        CharacterList.transform.GetChild(characterNum).GetComponent<Image>().color = isActive ? _activeSlotColor : _deactiveSlotColor;
    }

    // 현재 설정된 파티 저장
    private void SaveParty()
    {
        //DataManager.Instance.SavePartyMembers(_partyList);
    }
}
