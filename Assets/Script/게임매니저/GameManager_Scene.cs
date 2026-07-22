using System;
using UnityEngine;

public class GameManager_Scene : MonoBehaviour
{
    private static GameManager_Scene _instance;

    [Header("모듈"), SerializeField]
    public GameModules Modules;
    private G_Time time;
    private G_ObjPool pool;
    private G_Excutor excutor;


    void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }
        _instance = this;
        ToolUpdate();
    }
    void Start()
    {
        Modules?._Phase.Init(this);
        Modules?._Round.Init(this);
        Modules?._Spawn.Init(this);
    }
    void OnDestroy() => Kill();

    #region 라운드 모듈 접근부
        
    #endregion

    #region 페이즈 모듈 접근부
    public bool SetPhase(Phase_Main phase)
    {
        return phase switch
        {
            Phase_Main.READY => Modules._Phase.RunPhase_Ready(),
            Phase_Main.PLAY => Modules._Phase.RunPhase_Play(),
            Phase_Main.END => Modules._Phase.RunPhase_End(),
            _ => false,
        };
    }
    public void SetProgress() => Modules._Phase.RunPhase_Pause();
   
    public Phase_Main GetPhase() => Modules._Phase.CurrentPhase;
    public Progression GetProgress() => Modules._Phase.CurrentProgress;

    public void Request_StartRound(float GameTime, Action onEnd, Action<float> onProgress = null) 
        => Modules._Phase.Run_Routine(GameTime, onEnd, onProgress);
    
    #endregion

    #region 스폰 모듈 접근부
    //public void RequestSpawn(GameObject prefeb)
    //    => Modules._Spawn.Spawn(prefeb, Vector3.zero, Quaternion.identity);
    
    #endregion

    void ToolUpdate()
    {
        time    = new();
        pool    = new();
        excutor = new();
    }
    void Kill()
    {
        if (_instance == this) _instance = null;

        time?.Release();
        pool?.Release();
        excutor?.Release();
    }
}

[Serializable]
public class GameModules
{
    public Phase_Module _Phase;
    public Round_Module _Round;
    public Spawn_Module _Spawn;
}