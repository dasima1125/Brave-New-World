using System;
using System.Collections;

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private float ProjectileSpeed;
    private float LifeTime;
    private Rigidbody2D rb;
    protected bool isOverride;
    Action KillCode;


    public void Init(float ProjectileSpeed, float LifeTime)
    {
        this.ProjectileSpeed = ProjectileSpeed;
        this.LifeTime = LifeTime;
    }
    public void UploadLink(Action DataLink)
    {
        KillCode = DataLink;  
    } 

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Destroy(gameObject, LifeTime);
        if (!isOverride)
            ThrustRotation(0);
    }
    
    protected void ThrustRotation(float clamped)
    {
        rb.rotation += clamped;

        Vector2 dir = new(Mathf.Cos(rb.rotation * Mathf.Deg2Rad), Mathf.Sin(rb.rotation * Mathf.Deg2Rad));
        rb.linearVelocity = dir * ProjectileSpeed;
    }
    protected void ThrustAngular(float ? clamped)
    {
        if (clamped == null)
        {
            ThrustRotation(0);
            return;
        }
            
        rb.angularVelocity = clamped.Value * Mathf.Rad2Deg;

        Vector2 dir = new(Mathf.Cos(rb.rotation * Mathf.Deg2Rad), Mathf.Sin(rb.rotation * Mathf.Deg2Rad));
        rb.linearVelocity = dir * ProjectileSpeed;
    }
    
    protected INS2DData GetINS2D()
    {
        return new INS2DData
        {
            position = rb.position,
            velocity = rb.linearVelocity,
            rotation = rb.rotation,
            Speed = ProjectileSpeed
        };
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            //collision.gameObject.GetComponent<Target_Test>().Hit();
            Destroy(gameObject);
        }
    }
    void OnDestroy()
    {
        KillCode?.Invoke();
    }

}

public class Guided : Projectile
{
    /// Debug
    bool DebugPath = true;
    Vector2 leadPoint;
    // Projectile Settings
    SeekerType SeekerType;
    TrackLogicType Logic;
    DLMode DLEnable;
    float maxAngularVelocity;
    float navigationConstant;

    // Runtime Memory
    
    INS2DData INS = new();
    TNS2DData TNS = new();
    Vector2 targetMem;
    // 신호 소실시 예상지금으로 tns둬야함
    public void Init(WeaponData data)
    {
        SeekerType = data.SeekerType;
        DLEnable = data.DLSystem;
        Logic = data.LogicType;
        maxAngularVelocity= data.MaxAngularVelocity;
        navigationConstant = data.NavigationConstant;
        

        base.Init(data.ProjectileSpeed, data.LifeTime);
    }
    public void ReceiveCommand(TNS2DData data) => TNS = data;
    public SeekerType GetSeekerType() => SeekerType;
    public DLMode GetDLMode() => DLEnable;
    public INS2DData GetINS() => INS;
    public TNS2DData GetTNS() => TNS;
    protected override void Start() => base.Start();
    private void FixedUpdate()
    {
        INS = GetINS2D();
        LogicSelector();
    }
    void LogicSelector()
    {
        if(!TNS.InVaild)return;
        switch (Logic)
        {
            case TrackLogicType.Pure:
                ThrustRotation(TLogic2D.PureLeadClamped(TNS, INS, maxAngularVelocity, out leadPoint));
                break;

            case TrackLogicType.Lead:
                ThrustRotation(TLogic2D.LeadLineClamped(TNS, INS, maxAngularVelocity, out leadPoint));
                break;

            case TrackLogicType.Pn:
                ThrustAngular(TLogic2D.PN(TNS, INS, ref targetMem, maxAngularVelocity, navigationConstant , out leadPoint));
                break;
        }
    }
    void OnDrawGizmos()
    {
        if (leadPoint == Vector2.zero || !DebugPath) return;
        
        switch (Logic)
        {
            case TrackLogicType.Pure:
            Gizmos.color = Color.black;  
            break;
            
            case TrackLogicType.Lead:
            Gizmos.color = Color.yellow;    
            break;
            
            case TrackLogicType.Pn:
            Gizmos.color = Color.red;       
            break;
        }
        Gizmos.DrawLine(transform.position, leadPoint);
        Gizmos.DrawWireSphere(leadPoint, 1f);
        Gizmos.DrawWireCube(TNS.position, Vector3.one * 2);
        Gizmos.DrawLine(TNS.position, leadPoint);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Type : " + Logic.ToString());
            base.OnTriggerEnter2D(collision);
        }
    }

}


public static class TLogic2D
{
    public static float PureLeadClamped(TNS2DData TNS, INS2DData INS, float maxAngularVelocity, out Vector2 leadPoint)
    {
        Vector2 direction = (TNS.position - INS.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        float angleDiff = Mathf.DeltaAngle(INS.rotation, targetAngle);
        float maxDelta = maxAngularVelocity * Time.fixedDeltaTime;
        float clampedPure = Mathf.Clamp(angleDiff, -maxDelta, maxDelta);

        leadPoint = TNS.position;
        return clampedPure;
    }

    public static float LeadLineClamped(TNS2DData TNS, INS2DData INS, float maxAngularVelocity, out Vector2 leadTarget)
    {
        Vector2 displacement = TNS.position - INS.position;

        float a = Vector2.Dot(TNS.velocity, TNS.velocity) - (INS.Speed * INS.Speed);
        float b = Vector2.Dot(displacement, TNS.velocity) * 2;
        float c = Vector2.Dot(displacement, displacement);
        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0 || Mathf.Abs(a) < 0.0001f || TNS.velocity.magnitude >= INS.Speed)
        {
            float over = TNS.velocity.magnitude + 1;
            Vector2 correctionbackPos = TNS.position + TNS.velocity.normalized * over;
            Vector2 correctionfallDir = (correctionbackPos - INS.position).normalized;

            leadTarget = correctionbackPos;

            float fixtargetAngle = Mathf.Atan2(correctionfallDir.y, correctionfallDir.x) * Mathf.Rad2Deg;
            float fixangleDiff = Mathf.DeltaAngle(INS.rotation, fixtargetAngle);
            float fixmaxDelta = maxAngularVelocity * Time.fixedDeltaTime;
            
            return Mathf.Clamp(fixangleDiff, -fixmaxDelta, fixmaxDelta);
        }
        

        float rootP = (-b + Mathf.Sqrt(discriminant)) / (2 * a);
        float rootM = (-b - Mathf.Sqrt(discriminant)) / (2 * a);
        float t = Mathf.Max(rootP, rootM);

        Vector2 lead = TNS.position + TNS.velocity * t;
        Vector2 dir = (lead - INS.position).normalized;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float angleDiff = Mathf.DeltaAngle(INS.rotation, targetAngle);
        float maxDelta = maxAngularVelocity * Time.fixedDeltaTime;
        float clampedLead = Mathf.Clamp(angleDiff, -maxDelta, maxDelta);
        leadTarget = lead;

        return clampedLead;
    }
    
    public static float ? PN(TNS2DData TNS, INS2DData INS, ref Vector2 LastPos, float maxAngularVelocity, float navigationConstant, out Vector2 leadTarget)
    {
        // Although we could directly use TNS.velocity,
        // here velocity is manually calculated by dividing the difference between the previous position
        // and the current position by the elapsed time .

        Vector2 calcTargetVel = (TNS.position - LastPos) / Time.fixedDeltaTime;

        Vector2 LOS = TNS.position - INS.position;
        float LOSsq = LOS.sqrMagnitude;

        if (LOSsq < 0.0001f)
        {
            leadTarget = Vector2.zero;
            return null;
        }

        Vector2 LOSdir = LOS.normalized;
        Vector2 relVel = calcTargetVel - INS.velocity;
        float LDot = ((LOSdir.x * relVel.y) - (LOSdir.y * relVel.x)) / LOSsq;

        float Omega = navigationConstant * INS.Speed * LDot;
        float clampedOmega = Mathf.Clamp(Omega, -maxAngularVelocity * Mathf.Deg2Rad, maxAngularVelocity * Mathf.Deg2Rad);

        LastPos = TNS.position;

        float leadTime = Mathf.Clamp(Vector2.Distance(TNS.position, INS.position) / (INS.Speed + calcTargetVel.magnitude), 0.1f, 50f);
        leadTarget = TNS.position + calcTargetVel * leadTime;

        return clampedOmega;
    }

}
[Serializable] 
public struct INS2DData
{
    public Vector2 position;
    public Vector2 velocity;
    public float rotation;
    public float Speed;

}
[Serializable] 
public struct TNS2DData 
{
    public bool InVaild;
    public Vector2 position;
    public Vector2 velocity;
    public TNS2DData(Rigidbody2D rb)
    {
        position = rb.position;
        velocity = rb.linearVelocity;
        InVaild = true; 
    }
    public TNS2DData(Vector2 pos, Vector2 vel)
    {
        position = pos;
        velocity = vel;
        InVaild = true;
    }
    
}

