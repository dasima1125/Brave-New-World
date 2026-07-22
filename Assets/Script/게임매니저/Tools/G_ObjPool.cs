using UnityEngine;
using System.Collections.Generic;

public class G_ObjPool : G_Tools<G_ObjPool>
{
    private Dictionary<GameObject, Queue<GameObject>> _pools;
    // K: 생성원본 , Value: 오브젝트들(생성물)
    private Dictionary<GameObject, GameObject> _instanceToPrefab;
    // K: 오브젝트(생성물), Value: 프리펩(원본주소)
    private HashSet<GameObject> _poolStoreObjects;
    // 반환된 오브젝트 색인용
    private Transform _poolRoot;
    protected override void OnInit() //인스턴스 생성 로직
    {
        _poolRoot = new GameObject("Object Pool").transform;
        _pools = new();
        _instanceToPrefab = new();
        _poolStoreObjects = new();
    }
    protected override void OnRelease() //인스턴스철수로직
    {
        _poolRoot = null;

        _pools.Clear();
        _instanceToPrefab.Clear();
        _poolStoreObjects.Clear();
    }
    public static GameObject Get(GameObject prefab) => Ready ? Get(prefab, _i._poolRoot) : null;
    public static GameObject Get(GameObject prefab, Transform parent)
    {
        if (!Ready || prefab == null)
        {
            Debug.LogAssertion("[오브젝트 풀] 생성실패");
            return null;
        }

        Queue<GameObject> queue = _i.PrepareQueue(prefab);
        GameObject obj = queue.Count > 0 ? _i.UseInstance(queue) : _i.CreateInstance(prefab);
        _i._poolStoreObjects.Remove(obj);

        obj.SetActive(true);
        obj.transform.SetParent(parent);

        return obj;
    }
    public static void Prewarm(GameObject prefab, int count)
    {
        if (count <= 0) return;
        // 여분 오브젝트를 만드는게 관건. 즉 반복을하되 만들자마자 릴리즈가아닌 
        // 다만들고 한번에 릴리즈해야함 부모는.. 뭐 알빠아님 애당초 이거안쓸지도

        GameObject[] obj = new GameObject[count];
        for (int i = 0; i < count; i++) obj[i] = Get(prefab);
        for (int i = 0; i < count; i++) Release(obj[i]);
    }
    public static void Release(GameObject obj)
    {
        if (!Ready || obj == null || !_i._poolStoreObjects.Add(obj))
        {
            Debug.LogAssertion("[오브젝트 풀] 회수실패");
            return;
        }
        obj.SetActive(false);
        obj.transform.SetParent(_i._poolRoot);

        if (_i._instanceToPrefab.TryGetValue(obj, out var prefab))
            _i._pools[prefab].Enqueue(obj);
        else
            Object.Destroy(obj); // 추적 안 된 오브젝트는 그냥 파괴
    }
    Queue<GameObject> PrepareQueue(GameObject prefab)
    {
        if (!_pools.TryGetValue(prefab, out var queue)) // 없을경우 큐 생성
        {
            queue = new Queue<GameObject>();
            _pools[prefab] = queue;
        }
        return queue;
    }
    GameObject UseInstance(Queue<GameObject> queue)
    {
        GameObject obj = queue.Dequeue();
        obj.SetActive(true);
        return obj;
    }
    GameObject CreateInstance(GameObject prefab)
    {
        GameObject obj = Object.Instantiate(prefab);
        _instanceToPrefab[obj] = prefab;
        return obj;
    }

    // TODO : 나아아아중에 HideFlags.HideInHierarchy; 를이용해서 하이리키창 테레당하는거도 막는걸 만들어볼려나?근데필요가있나?>
}
public interface IPoolable //풀러 등록용
{
    void Init(System.Action releaseAction);
}

public interface ILifecycleBindable // 객체추적용..근데 오브젝트 풀클래스에둘만한건아닌거같은데 
// 그렇다고 어디두기도그럼
// 추가 : 부모를 공통으로 선언할꺼임 이거쓰는건 기본적으로 아마 오브젝트풀이랑 공통접근부모가있을예정
// 아마도? // 철수자를 굳이 인터페이스화시켜야할까?
{
    void OnLifeBind(System.Action<ILifecycleBindable> onRelease);

}

