using System;
using System.Collections.Generic;
using System.Collections;
using CharacterDefinition;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class PlayerActionUI : BaseUI
{
    enum GameObjects
    {
        Move,
        Attack,
        Ap,
        Build,
        Skill,
        CharacterPopUpPoint, // 캐릭터 피격, 회피, 회복 효과 적용 포인트 1
    }

    enum Buttons
    {
        MoveButton,
        AttackButton,
        ApButton
    }

    enum Texts
    {
        WallCount,
        
    }

    protected override bool IsSorting => false;
    public override UIName ID => UIName.PlayerActionUI;

    public GameObject GetToken;
    private CharacterController _characterController;
    public PlayerCharacter Character;

    private void Start()
    {
        Bind<GameObject>(typeof(GameObjects));
        Bind<TMP_Text>(typeof(GameObjects));
        Character.OnDamage += OnCharacterTakeDamage;
    }

    // 필수 정의
    public void SetCharacter(CharacterController controller, PlayerCharacter character)
    {
        _characterController = controller;
        Character = character;
    }

    public void OnCharacterTakeDamage(int damage)
    {
        int childCount = GetObject((int)GameObjects.CharacterPopUpPoint).transform.childCount;
        if (childCount < 2)
        {
            Transform parent = GetObject((int)GameObjects.CharacterPopUpPoint).transform;
            int actionType = damage == 0 ? 2 : (damage > 0 ? 1 : 0);
            int value = damage;
            CharacterPopUpTextUI item = UIManager.Instance.MakeSubItem<CharacterPopUpTextUI>(parent.position - new Vector3(0, (childCount <= 1 ? 0 : 0.5f), 0), parent.rotation, parent);
            item.Value = value;
            item.PopUpType = actionType;
        }
    }

    public void DestroyPlayerActionUI()
    {
        foreach (Transform child in GetObject((int)GameObjects.CharacterPopUpPoint).transform)
        {
            child.GetComponent<CharacterPopUpTextUI>().DeleteItem();
        }
        //DestroyImmediate(gameObject);
    }
}
