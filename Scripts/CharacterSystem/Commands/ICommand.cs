using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    public bool Execute();
    
    public bool Undo();
}
