using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectWallPosPopUpUI : BaseUI
{
    protected override bool IsSorting => true;
    public override UIName ID => UIName.SelectWallPosPanelUI;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
    }
}
