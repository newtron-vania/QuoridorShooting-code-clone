using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region RequireComponents
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
#endregion


[DisallowMultipleComponent]
public class QuoridorCharacterObject : MonoBehaviour
{
    private BaseCharacter _connectedCharacter;
    
    private Rigidbody2D _rigidbody;
    private Animator _animator;

    public Rigidbody2D Rigidbody => _rigidbody;
    public Animator Animator => _animator;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    public void Init(BaseCharacter character)
    {
        _connectedCharacter = character;
    }
}
