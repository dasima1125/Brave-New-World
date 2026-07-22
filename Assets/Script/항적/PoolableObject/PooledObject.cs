using System;
using UnityEngine;

public abstract class PooledObject : MonoBehaviour, IPoolable
{
    protected bool ActiveObject { get; private set; }
    Action _poolRelease;
    public void Init(Action releaseAction)
    {
        _poolRelease = releaseAction;
        ActiveObject = true;

        InitChain();
    } 
    protected virtual void InitChain() { }
    
    protected virtual void Kill()
    {
        if(!ActiveObject) return; 

        _poolRelease?.Invoke();
        _poolRelease = null;
        
        ActiveObject = false;
    }
}
