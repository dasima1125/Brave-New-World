using UnityEngine;


public abstract class G_Tools<T> where T : G_Tools<T>
{
    protected static T _i;
    protected static bool Ready
    {
        get
        {
            if (_i == null)
            {
                Debug.LogWarning($"<color=red>[{typeof(T).Name}]</color> 인스턴스 없음.");
                return false;
            }
            return true;
        }
    }

    protected G_Tools()
    {
        _i = (T)this;
        OnInit();
    } 

    public void Release()
    {
        if (_i == this as T)
        {
            OnRelease(); 
            _i = null;  
        }
    }
    protected virtual void OnInit() { }
    protected virtual void OnRelease() { }
}
