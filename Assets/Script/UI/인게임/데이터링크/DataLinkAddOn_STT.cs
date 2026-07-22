using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class DataLinkAddOn_STT : MonoBehaviour
{
    [SerializeField] GameObject OnlinePanel;
    [SerializeField] GameObject OffLinePanel;
    [Header("TGT_UI_Zone")]
    [SerializeField] private TextMeshProUGUI TGT_AZ;
    [SerializeField] private TextMeshProUGUI TGT_RNG;
    [SerializeField] private TextMeshProUGUI TGT_ALT;
    [SerializeField] private TextMeshProUGUI TGT_HDG;
    [SerializeField] private TextMeshProUGUI TGT_SPD;


    [Header("Missile_UI_Zone")]
    [SerializeField] private GameObject DLPanel;
    [SerializeField] private GameObject NonDLPanel;

    [SerializeField] private TextMeshProUGUI Missile_GuidanceType;
    [SerializeField] private TextMeshProUGUI Missile_AMMName;
    [SerializeField] private TextMeshProUGUI Missile_TTI;
    [SerializeField] private TextMeshProUGUI Missile_RNG;
    [SerializeField] private TextMeshProUGUI Missile_SPD;



    public void Online(bool OnOff)
    {
        OnlinePanel.SetActive(OnOff);
        OffLinePanel.SetActive(!OnOff);
    }
    public void UpdateTGT(STT_TargetData data)
    {
        TGT_AZ.text = $"{data.TargetAngle:F1}°";
        TGT_RNG.text = $"{data.TargetDistance:F0} km";
        TGT_HDG.text = $"{(Mathf.Atan2(data.Velocity.x, data.Velocity.y) * Mathf.Rad2Deg + 360f) % 360f:F0}°";
        TGT_SPD.text = $"{data.Velocity.magnitude:F0} m/s";
        TGT_ALT.text = "--- m";

    }

    public void UpdateMissile(bool DL, INS2DData INS, TNS2DData TNS)
    {

        DLPanel.SetActive(DL);
        NonDLPanel.SetActive(!DL);

        if (DL)
        {
            Missile_GuidanceType.text = "WIP progress";
            Missile_AMMName.text = "WIP progress";
            var RNG = Vector2.Distance(INS.position, TNS.position);
            var SPD = INS.Speed;
            var TTI = RNG / SPD;
            Missile_TTI.text = $"{TTI:F1} sec";
            Missile_RNG.text = $"{RNG:F1} km";
            Missile_SPD.text = $"{SPD:F0} m/s";
        }
    }
}
