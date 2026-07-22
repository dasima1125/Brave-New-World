using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;

public class DropDown_Custom : MonoBehaviour
{
    [Header("커스텀 드롭다운")]
    [SerializeField] Transform ItemListPosition;
    [SerializeField] GameObject DropdownItem;
    [SerializeField] GameObject BlockerPanel;
    [Header("아이템")]
    [SerializeField] List<GameObject> Items = new();
    bool IsOpen = false;
    private GameObject activeBlocker;// 런타임 관제
    public List<GameObject> GetItemObject => Items;
    public List<T> GetItemComponents<T>() where T : Component => Items.Select(item => item.GetComponent<T>()).Where(item => item != null).ToList();
    public bool OpenDropdown(int count)
    {
        if(IsOpen) return false;

        IsOpen = true;
        BlockerCreate();
       
        if (ItemListPosition.TryGetComponent<LayoutGroup>(out var layoutGroup))
            layoutGroup.CalculateLayoutInputHorizontal();
        
        for (int i = 0; i < count; i++)
        {
            var item = Instantiate(DropdownItem, ItemListPosition);
            Items.Add(item);
        }
        return true;
    }
    
    public bool CLoseDropdown()
    {
        if(!IsOpen) return false;
        
        foreach (var item in Items)
        {
            item.transform.SetParent(null);
            Destroy(item);
        }
        Items.Clear();
        if (activeBlocker != null) Destroy(activeBlocker);
        return true;
    }
    void BlockerCreate()
    {
        var rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        activeBlocker = Instantiate(BlockerPanel, rootCanvas.rootCanvas.transform);
        activeBlocker.transform.SetAsFirstSibling();

        if (activeBlocker.TryGetComponent<Button>(out var btn)) btn.onClick.AddListener(() => { CLoseDropdown(); IsOpen = false; });
    }

}
