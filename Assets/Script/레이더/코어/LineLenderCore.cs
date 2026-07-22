using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineLenderCore : MonoBehaviour
{
    private LineRenderer lr;
    private Transform Startpoints;
    private Transform Endpoints;
    private void Awake() {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }
    public void SetUpLine(Transform start , Transform end) 
    {
        Startpoints = start;
        Endpoints   = end;  
        lr.enabled  = true;    
    }
    public void TeardownLine() 
    {
        Startpoints = null;
        Endpoints   = null; 
        lr.enabled  = false;  
    }
    private void Update() 
    {
        if(Startpoints != null && Endpoints != null)
        {
            lr.SetPosition(0,Startpoints.position);
            lr.SetPosition(1,Endpoints.position);
        }
            
    }
}
