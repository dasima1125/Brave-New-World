using UnityEngine;
using TMPro;


public class DataLinkAddOn_MTT : MonoBehaviour
{
    [Header("TGT Info")]
    [SerializeField] private TextMeshProUGUI IndexText;
    [SerializeField] private TextMeshProUGUI AZText;
    [SerializeField] private TextMeshProUGUI SPDText;
    [Header("DL_AddOn Info")]
    [SerializeField] private TextMeshProUGUI TTIText;
    [SerializeField] private TextMeshProUGUI RNGText;
    [Header("Missile Status")]
    [SerializeField] private GameObject DLObject;
    [SerializeField] private GameObject NonDLObject;
    [SerializeField] private TextMeshProUGUI MissileNameText;
    [SerializeField] private TextMeshProUGUI MissileGuidanceText;
    public void Render(bool DL ,Ping ping ,Guided guided)
    {
        int displayIndex = transform.GetSiblingIndex() + 1;
        IndexText.text = $"{displayIndex:D2}";
        AZText.text = ping.GetTNS.RAngle.ToString("F1") + "°";
        SPDText.text = ping.GetTNS.velocity.magnitude.ToString("F0") + " m/s";
        if (!DL)
        {
            DLObject.SetActive(false);
            NonDLObject.SetActive(true);
            TTIText.text = "---";
            RNGText.text = "---";
            return;
        }
        {
            DLObject.SetActive(true);
            NonDLObject.SetActive(false);
            var RNG = Vector2.Distance(ping.GetTNS.Position, guided.GetINS().position);
            var SPD = guided.GetINS().Speed;
            var TTI = RNG / SPD;
            TTIText.text = TTI.ToString("F1");
            RNGText.text = RNG.ToString("F1");

            MissileNameText.text = "WIP progress";
            MissileGuidanceText.text = "WIP progress";
        }
        
    }
}
