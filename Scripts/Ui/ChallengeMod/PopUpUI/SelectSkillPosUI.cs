using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectSkillPosUI : BaseUI
{
    protected override bool IsSorting => true;
    public override UIName ID => UIName.SelectSkillPosUI;

    public override void Init()
    {
        base.Init();
    }

    private void Start()
    {
        Init();
    }
}
