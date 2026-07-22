using System;
using System.Collections.Generic;
using UnityEngine;
public class G_Excutor : G_Tools<G_Excutor>
{
    private Dictionary<string, Action<object>> _EventBook;

    protected override void OnInit()
    {
        _EventBook = new();
    }
    protected override void OnRelease()
    {
        _EventBook.Clear();
        _EventBook = null;
    }
    public static void Subscribe(string key, Action handler)
    {
        if (!Ready) return;
        if (_i._EventBook.ContainsKey(key))
        {
            Debug.Log("이미 등록된 구독입니다.");
            return;
        }
        void wrapper(object _) => handler?.Invoke();
        _i._EventBook[key] = null;
        _i._EventBook[key] = wrapper;
    }
    /*
    // 1. 구독: T(클래스 규격, 액션의 파라미터)를 고정하고, object 규격의 wrapper 대리인을 장부에 예약 대입.
    // 2. 호출: Call이 wrapper를 찾아 Invoke(dto)로 실행, 데이터가 obj 파라미터에 넣은채 액션 실행.
    // 3. 검증: obj is T actualData 이게그거지 콜에서 들어온 파라미터의 클래스 타입검사 래퍼에 등록된거랑 같은 클래스타입인지 검증.
    // 4. 실행: pass handler에 넣고 실행 .
    public static void Subscribe(string key, Action<object> handler)
    {
        if (!Ready) return;
        if (_i._EventBook.ContainsKey(key))
        {
            Debug.Log("이미 등록된 구독입니다.");
            return;
        }
        _i._EventBook[key] = null;
        _i._EventBook[key] = handler;
    }
    */
    public static void Subscribe<T>(string key, Action<T> handler)
    {
        if (!Ready) return;
        if (_i._EventBook.ContainsKey(key))
        {
            Debug.Log("이미 등록된 구독입니다.");
            return;
        }
        void wrapper(object obj) // 파라미터 래핑 
        {
            if (obj is T pass)  handler?.Invoke(pass);
            else
            {
                Debug.LogError($"[G_Excutor] 파라미터 불일치 Key: {key}, Expected: {typeof(T).Name}");
            }
        }
        _i._EventBook[key] = null;
        _i._EventBook[key] = wrapper; 
    }
    public static void Unsubscribe(string key)
    {
        if (!Ready) return;
        if (_i._EventBook.ContainsKey(key)) _i._EventBook.Remove(key);
    }
    public static void Call(string key)
    {
        if (!Ready) return;
        if (_i._EventBook.TryGetValue(key, out var handler) && handler != null) handler?.Invoke(null);
        
        else Debug.LogWarning($"[G_Excutor] 구독되지 않은 실행을 호출했습니다: {key}");
    }
    /*
    public static void Call(string key, object dto)
    {
        if (!Ready) return;
        if (_i._EventBook.TryGetValue(key, out var handler) && handler != null)
            handler?.Invoke(dto);
        
        else Debug.LogWarning($"[G_Excutor] 미등록 키 호출: {key}");
    }
    */
    public static void Call<T>(string key, T dto)
    {
        if (!Ready) return;
        if (_i._EventBook.TryGetValue(key, out var handler) && handler != null)
        {
            handler?.Invoke(dto);
        }
        else Debug.LogWarning($"[G_Excutor] 구독되지 않은 실행을 호출했습니다. Key: {key}");
        
    }
}
