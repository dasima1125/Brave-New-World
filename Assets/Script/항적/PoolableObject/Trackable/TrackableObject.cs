using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class TrackableObject : PooledObject, ILifecycleBindable, IContactable
{
    protected Action<ILifecycleBindable> action_Life;
    protected Action<IContactable> action_Contect;
    protected Rigidbody2D rb;
    [SerializeField] //테스트끝나면지우셈
    protected float altitude; // 애당초 개발당시 고도를 고려안하다 추가한거
                              // 2d환경의 임의 축이라 사실상 식별용이걸로 어려운연산은 안함

    public PosData CurrentPosData => new(
        rb != null ? rb.position : Vector2.zero,
        rb != null ? rb.linearVelocity : Vector2.zero,
        altitude
    );


    //게임라운드 시스템이 참조할시스템 또한 처리할시스템 
    public void OnLifeBind(Action<ILifecycleBindable> onRelease) => action_Life = onRelease;
    public void OnContacted(Action<IContactable> action) => action_Contect = action;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
    }

    protected override void Kill()
    {
        if (!ActiveObject) return;

        action_Life?.Invoke(this);
        action_Life = null;

        action_Contect?.Invoke(this);
        action_Contect = null;


        base.Kill();
    }

    public void FlushOrder()
    {
        Kill();
    }
}
[Serializable]
public struct PosData
{
    public Vector2 position; // 좌표위치
    public Vector2 velocity; // 가속 및 방향
    public float altitude;  // 고도(색인용)

    public PosData(Vector2 pos, Vector2 vel, float alt)
    {
        position = pos;
        velocity = vel;
        altitude = alt;
    }
}

public interface IContactable
{
    PosData CurrentPosData { get; }
    void OnContacted(Action<IContactable> action);
}
/*
// 원래는 원자화를 할려했음
// 근데 생각해보면 포착을 할수있어야 선택을하든 할꺼안가?
// 파사드 패턴쓰지뭐

public interface IPathInput
{
    void SetPath(){}
}
*/
public interface ISelectable : IContactable
{
    void SetPath(DotPathVer2 path);
}
public interface IDamageable
{
    bool IsDestroyed { get; }
    void TakeDamage(float damage);
}

// 생각해보니 이둘은.. 쓸필요가없을지도?
/*
public interface IAAExplosive 
{ 
    float ProximityRange { get; } 
    float ExplosionRadius { get; } 
    float ExplosionDamage { get; } 
    void TriggerProximity(); 
}
public interface IAGExplosive 
{ 
    float ImpactRadius { get; } 
    float ImpactDamage { get; } 
    void TriggerImpact();
}
*/