using System;
using UnityEngine;

public interface IMovable
{
    public Vector2Int Position { get; }
    public Vector2Int PrevPosition { get; }

    public Action<IMovable> OnPositionChanged { get; }
}

public enum MovableEvent
{
    OnTileEnter,
    OnTileExit,
}

public interface IMovableEventEffectLogic
{
    public void EffectByMovableEvent(MovableEvent movableEvent, EffectInstance effectInstance, EffectData effectData, IEffectableProvider target);
}
