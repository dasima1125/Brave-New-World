using UnityEngine;

public class Trackable_Noise : TrackableObject 
{
    protected override void InitChain()
    {
        base.InitChain(); 
        Debug.Log("[최하자식]개선구조 테스트중");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
