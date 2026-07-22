using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class AAMPanel_AddOn : MonoBehaviour
{
    [Header("Weapon Identity TMP")]
    [SerializeField] private Image AMMImage;
    [SerializeField] private TextMeshProUGUI AMMNameText;
    [SerializeField] private TextMeshProUGUI SeekerText;
    [SerializeField] private TextMeshProUGUI DLText;

    [Header("Weapon Specs TMP (Trio)")]
    [SerializeField] private TextMeshProUGUI SPDText;
    [SerializeField] private TextMeshProUGUI MaxAngleText;
    [SerializeField] private TextMeshProUGUI DuritonText;

    [Header("Ammo Indicator UI")]
    [SerializeField] private TextMeshProUGUI AMMCountText;
    [SerializeField] private GameObject[] AMMSegments;

    [Header("Panel State Visual")]
    [SerializeField] private GameObject SelectAbleObject;
    [SerializeField] private CanvasGroup panelCanvasGroup;
    /// <summary>
    /// 무장 매니저가 매 프레임 혹은 무장 변경 시 이 함수를 호출해 데이터를 주입.
    /// </summary>
    public void Render(MissileStock stock) // 선택됬는지 안됬는지도전잘해줘야함
    {
        var data = stock.Spec;

        AMMNameText.text = data.MissileModel.ToString();
        AMMImage.sprite = data.MissileSprite;
        SeekerText.text = data.SeekerType.ToString();
        DLText.text = data.DLSystem.ToString();

        SPDText.text = $"SPD {data.ProjectileSpeed:F0} Km/s";
        MaxAngleText.text = $"AV {data.MaxAngularVelocity:F0}G";
        DuritonText.text = $"DUR {data.LifeTime:F0}S";

        AMMCountText.text = $"{stock.NowCount}/{stock.PeekCount}";

        foreach (var Seg in AMMSegments) Seg.SetActive(false);
        for (int i = 0; i < stock.PeekCount; i++)
        {
            AMMSegments[i].SetActive(true);
            var image = AMMSegments[i].GetComponent<Image>();
            Color color = image.color;
            if (i < stock.NowCount)
                color.a = 1.0f;
            else
                color.a = 0.3f;
            image.color = color;
        }

        if (panelCanvasGroup != null)
        {
            if (stock.NowCount <= 0) panelCanvasGroup.alpha = 0.2f;
            else panelCanvasGroup.alpha = 1.0f;

        }
    }
    public void Render(MissileStock stock, bool isSelected) //확장
    {
        Render(stock);
        SelectAbleObject.SetActive(isSelected);
    }
   
}
