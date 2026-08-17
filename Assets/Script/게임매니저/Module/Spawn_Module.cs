using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Module : MonoBehaviour
{
    GameManager_Scene core;
    public void Init(GameManager_Scene gameManager)
    {
        core = gameManager;
        G_Excutor.Subscribe<SpawnDTO_Basic>("DebugSpawn", Spawn);

    }
    public void Spawn(SpawnDTO_Basic DTO)
    {
        if (DTO.prefebObject == null) return;

        GameObject instance = G_ObjPool.Get(DTO.prefebObject);
        if (instance == null) return;
        if (!instance.TryGetComponent<IPoolable>(out var poolable))
        {
            Debug.LogError($"[Spawn_Module] 스폰 실패. '{DTO.prefebObject.name}' 허가되지 않은 컴포넌트.");
            G_ObjPool.Release(instance);
            return;
        }
        poolable.Init(() => G_ObjPool.Release(instance));
        AfterActionBoot(DTO.AfterAction, instance);// 경고를 하는게맞나 없으면 근데사실..없어도되는애가있을수도잇잔아?
        return;
    }
    //일부러 오브젝트로보냄 뭐 나중에 애프터액션을하는데 다른개념이나올수도있는데 인터페이스째 보내는건아님
    void AfterActionBoot(RoundActionSO actionSO, GameObject instance)
    {
        if (actionSO == null) return;
        if (actionSO is IAction_Receiver receiver)
        {
            receiver.Action(instance);
            return;
        }
        actionSO.Action();
    }
}
[Serializable]
public class SpawnDTO_Basic
{
    public GameObject prefebObject;
    public RoundActionSO AfterAction;
}