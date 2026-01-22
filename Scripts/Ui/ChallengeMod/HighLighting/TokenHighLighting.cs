using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TokenHighLighting : BaseUI
{
    protected override bool IsSorting => false;
    public override UIName ID => UIName.CharacterHighLighting;

    private void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().DOFade(0.0f, 1).SetLoops(-1, LoopType.Yoyo);
    }
}