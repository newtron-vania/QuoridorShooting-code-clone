using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct RewardCardData
{
    private int        _id;
    private int        _amount;
    private GradeType  _grade;
    private RewardType _category;
    private string     _name;
    private Sprite     _image;
    private string     _description;

    public int        Id          => _id;
    public int        Amount      => _amount;
    public GradeType  Grade       => _grade;
    public RewardType Category    => _category;
    public string     Name        => _name;
    public Sprite     Image       => _image;
    public string     Description => _description;


    public RewardCardData(int id, int amount, GradeType grade, RewardType category, string name, Sprite image, string description)
    {
        _id          = id;
        _amount      = amount;
        _grade       = grade;
        _category    = category;
        _name        = name;
        _image       = image;
        _description = description;
    }
}

public class RewardCard : MonoBehaviour
{
    [Header("------ Reward Info ------")]
    [SerializeField] private Image    _rewardBackground;
    [SerializeField] private TMP_Text _rewardCatergoryText;
    [SerializeField] private TMP_Text _rewardNameText;
    [SerializeField] private Image    _rewardImage;
    [SerializeField] private TMP_Text _rewardDescriptionText;

    [Header("------ Disable Panel ------")]
    [SerializeField] private GameObject _disablePanel;

    private int        _index;
    private int        _id;
    private int        _amount;
    private RewardType _category;

    public int        Id       => _id;
    public RewardType Category => _category;
    public int        Amount   => _amount;

    public event Action<int> OnCardClicked;

    public void ClickEvent()
    {
        OnCardClicked.Invoke(_index);
    }

    public void SetIndex(int index)
    {
        _index = index;
    }

    public void ToggleDisablePanel(bool isActive)
    {
        _disablePanel.SetActive(isActive);
    }

    public void SetRewardCard(RewardCardData rewardCardData)
    {
        _id     = rewardCardData.Id;
        _amount = rewardCardData.Amount;

        SetCardColor(rewardCardData.Grade);
        SetRewardCategory(rewardCardData.Category);

        _rewardNameText.text        = rewardCardData.Name;
        _rewardDescriptionText.text = rewardCardData.Description;
        _rewardImage.sprite         = rewardCardData.Image;
    }

    public void SetRewardCategory(RewardType rewardType)
    {
        string text = "";
        _category = rewardType;

        switch (rewardType)
        {
            case RewardType.Artifact:
                text = "유물";
                break;
            case RewardType.Supplyment:
                text = "보급품";
                break;
            case RewardType.Money:
                text = "재화";
                break;
        }

        _rewardCatergoryText.text = text;
    }

    private void SetCardColor(GradeType tier)
    {
        Color color = Color.black;

        switch(tier)
        {
            case GradeType.Normal:
                color = Color.black;
                break;
            case GradeType.Rare:
                color = Color.white;
                break;
            case GradeType.Unique:
                color = Color.yellow;
                break;
        }

        _rewardBackground.color = color;
    }
}
