using UnityEngine;
using TMPro;
public class PingPanel_AddOn : MonoBehaviour
{
    [Header("Pure TMP Components")]
    [SerializeField] private TextMeshProUGUI IndexText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI angleText;
    [SerializeField] private TextMeshProUGUI altitudeText;
    [SerializeField] private TextMeshProUGUI HDGText;
    [SerializeField] private TextMeshProUGUI VelocityText;
    [Header("Selection Visual")]
    [SerializeField] private GameObject SelectAbleObject;
    [SerializeField] private GameObject HighlightObject;
    public void Render(PingTNS tns, PingState state, bool isSelectAble, bool isSelected)
    {
        int displayIndex = transform.GetSiblingIndex() + 1;
        IndexText.text = $"{displayIndex:D2}";

        SelectAbleObject.SetActive(isSelectAble);
        HighlightObject.SetActive(isSelected);
        
        if (!tns.IsValid)
        {
            statusText.text = "DATA INV";
            return;
        } 

        statusText.text = $"{state}";
        distanceText.text = $"{tns.RDistance:F0} km";
        angleText.text = $"{tns.RAngle:F1}°";
        altitudeText.text = "---";//아직 고도시스템은 안만듬
        if (state != PingState.SRC_Nomarl)
        {
            float speed = tns.velocity.magnitude;
            float headingAngle = Mathf.Atan2(tns.velocity.x, tns.velocity.z) * Mathf.Rad2Deg;
            if (headingAngle < 0) headingAngle += 360f;

            HDGText.text      = $"{headingAngle:F0}°";  
            VelocityText.text = $"{speed:F0} m/s";
        }
        else
        {
            HDGText.text      = "---"; 
            VelocityText.text = "---";

        }
    }
}
