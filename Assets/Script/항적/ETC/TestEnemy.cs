using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class TestEnemy : PooledObject, ILifecycleBindable
{
    bool _active = false;

    //어우 어지러워
    public float speed = 5f;
    public float LifeTime;
    Action<ILifecycleBindable> action_Life;
    Rigidbody2D rb;
    float currentTimer;
    

    //웨이포인트 테스트.. 아마 인덱스를 몇번째인덱스로 향하는진 자기가알아야겠지?
    [SerializeField] DotPath WayMap;
    [SerializeField] int WayIndex;

    public void OnLifeBind(Action<ILifecycleBindable> onRelease)=> action_Life += onRelease;
    public void Init()
    {
        currentTimer = 0f;
        WayIndex = 0;
        
        _active = true;
        Debug.Log("객체 테스트중 : 생성됨");
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; 

    }

    void Update() 
    {   if(!ActiveObject || !_active) return;

        currentTimer += G_Time.Dtime;
        if (currentTimer >= LifeTime) Kill();
    }
    void FixedUpdate() // 일부러 분리 공사중임
    {
        if (WayMap == null || WayMap.points.Count == 0 || WayIndex >= WayMap.points.Count)
         return;// 정지시킬려면 백터 제로를 걸던가
        
        Vector2 WayPos = WayMap.points[WayIndex];
        Vector2 MyPos = transform.position;

        // 현재 위치에서 목적지를 향하는 순수 방향 벡터 추출
        Vector2 direction = (WayPos - MyPos).normalized;

        rb.linearVelocity = direction * speed;

      
        float distanceSqr = (WayPos - MyPos).sqrMagnitude;
        float arrivalThreshold = 0.04f; // 거리 0.2f 미만일 때 도달 판정 (0.2 * 0.2)

        if (distanceSqr < arrivalThreshold)
        {
            WayIndex++;
            
            // 나중에 환형(Loop) 제어를 넣거나 풀링 반환 처리를 할 분기 구역
            // 예: if (WayIndex >= WayMap.points.Count && isLoop) WayIndex = 0;
            // 근데일단 패스먼저 패스끝나고 이제 라운드so슬슬 구조화해야함
        }
    }
    protected override void Kill()
    {
        
        action_Life?.Invoke(this);
        action_Life = null;
        _active     = false;

        // 이시점부터 참조가 끊김
        
        Debug.Log("객체 테스트중 : 초기화됨");
        base.Kill();

    }

    public void FlushOrder()
    {
        throw new NotImplementedException();
    }
}

