using System;
using System.Collections.Generic;
using UnityEngine;

public class Rule_Module : MonoBehaviour
{
    //테스트존 이따구로 구현할일은 없겠지?
    GameManager_Scene core;
    public int cityDestroyThreshold;
    public int citiesDestroyed;
    public bool isGameOver;
    public void Init(GameManager_Scene gameManager)
    {
        core = gameManager;
        G_Excutor.Subscribe("CityDestroyed", OnCityDestroyed);
        G_Excutor.Subscribe<GameObject>("DebugStore2", ListStore_LifeCycle);
    }



    public void ResetState()
    {
        citiesDestroyed = 0;
        isGameOver = false;
    }


    void OnCityDestroyed()
    {
        if (isGameOver) return;

        citiesDestroyed++;
        if (citiesDestroyed >= cityDestroyThreshold)
        {
            isGameOver = true;
            core.Request_RoundLose();
        }
    }
    //
    //경로생성자//
    [SerializeField] private List<Transform> startTransforms;
    [SerializeField] private List<Transform> MiddleTransforms;
    [SerializeField] private List<Transform> endTransforms;
    // 게임내 npc 관리
    public List<ILifecycleBindable> Test_LifeCycle = new(); // 풀링 대상인가
    public List<IContactable> Test_Contactable = new(); // 추적해야할 대상인가
    public List<GameObject> Test_ContactableView = new();
    public List<GameObject> Test_LifeCycleView = new();

    void ListStore_LifeCycle(GameObject obj)
    {
        if (obj.TryGetComponent<ILifecycleBindable>(out var lifecycleBindable))
        {
            Test_LifeCycle.Add(lifecycleBindable);
            Test_LifeCycleView.Add(obj);

            lifecycleBindable.OnLifeBind(ListRemove_LifeCycle);
            //경로및 추가데이터 주입..아마 이부분은 상당히바뀔지도.
            // 경고 위치는 반드시 시작점과 갈점 두개이상은되어야함 이점 중요
            int randomStartIdx = UnityEngine.Random.Range(0, startTransforms.Count);
            int randomMiddleIdx = UnityEngine.Random.Range(0, MiddleTransforms.Count);
            int randomEndIdx = UnityEngine.Random.Range(0, endTransforms.Count);

            Vector2 randomStart = startTransforms[randomStartIdx].position;
            Vector2 randomMiddle = MiddleTransforms[randomMiddleIdx].position;
            Vector2 randomEnd = endTransforms[randomEndIdx].position;

            DotPathVer2 path = new()
            {
                Waypoints = new List<Vector2> { randomStart, randomMiddle, randomEnd }
            };
            // 첫번째랑 두번째 위치랑 첫번째랑 두번째 상대각 즉 첫번째에서두번째 바라보는각도 추출
            Vector2 FirstPos = path.Waypoints[0];
            Vector2 SecondPos = path.Waypoints[1];
            //아 물론 고찰이 하나있음 유니티자체각을 안쓴다는거임.. 뭐 근데생각해보니 안써도될거같기도하고?
            //이놈은 레이더가쓰는게아니라 항적자체라 굳이,,간할거같은데? 
            //시그널 클래스 참고하고 필요하면수정해야지뭐,
            Vector2 dir = FirstPos - SecondPos;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // 여기서 생성 위치랑 방위 잡도록함 물론 월드좌표 기준.
            obj.SetActive(false);
            obj.transform.SetPositionAndRotation(FirstPos, Quaternion.Euler(0f, 0f, targetAngle));
            obj.SetActive(true);


            if (lifecycleBindable is ISelectable Sel)
            {
                Sel.SetPath(path);
            }

            if (lifecycleBindable is IContactable Con) // 컨텍에이블.이놈은 사실은.. 여기있으면안됨 다만 일단두기로함
            {
                ListStore_Contactable(Con);
            }

        }
    }
    void ListRemove_LifeCycle(ILifecycleBindable obj)
    {
        if (Test_LifeCycle.Contains(obj))
        {
            Test_LifeCycle.Remove(obj);
            if (obj is MonoBehaviour mono) Test_LifeCycleView.Remove(mono.gameObject);

        }

    }
    void ListStore_Contactable(IContactable selectable)
    {
        Test_Contactable.Add(selectable);
        if (selectable is MonoBehaviour mono) Test_ContactableView.Add(mono.gameObject);

        selectable.OnContacted(ListRemove_Contactable);
    }
    void ListRemove_Contactable(IContactable selectable)
    {
        if (Test_Contactable.Contains(selectable))
        {
            Test_Contactable.Remove(selectable);
            if (selectable is MonoBehaviour mono) Test_ContactableView.Remove(mono.gameObject);
        }
    }
    public void ListFlush_LifeCycle()
    {
        for (int i = Test_LifeCycle.Count - 1; i >= 0; i--)  
        Test_LifeCycle[i].FlushOrder();
    }
}
