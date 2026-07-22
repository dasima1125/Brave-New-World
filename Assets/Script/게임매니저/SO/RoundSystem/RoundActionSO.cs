using UnityEngine;

public abstract class RoundActionSO : ScriptableObject
{
    public string RequestKey;
    public abstract void Action();
    
}
public interface IAction // 근데 굳이?

{
    void Action();
}
public interface IAction_Receiver
{
    void Action(GameObject instance);
}