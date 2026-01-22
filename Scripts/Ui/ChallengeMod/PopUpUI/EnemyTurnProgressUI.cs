using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyTurnProgressUI : BaseUI
{
    enum GameObjects
    {
        LoadingImage
    }

    protected override bool IsSorting => true;
    public override UIName ID => UIName.EnemyTurnProgessUI;

    private void Start()
    {
        Init();
    }

    public override void Init()
    {
        base.Init();
        Bind<GameObject>(typeof(GameObjects));

        GetObject((int)GameObjects.LoadingImage).transform.DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
    }
}
