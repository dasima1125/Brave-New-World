using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineLenderTest : MonoBehaviour
{
   
    [SerializeField] private LineLenderCore line;

    public void Cupuling(Transform target)
    {
        line.SetUpLine(transform,target);
    }
    public void Decupuling()
    {
        line.TeardownLine();
    }

}
