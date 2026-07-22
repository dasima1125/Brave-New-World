using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class RadarPanel_AddOn : MonoBehaviour
{
    [Header("SRC_UI_Zone")]
    [SerializeField] private TextMeshProUGUI SRC_RadarState;
    [SerializeField] private TextMeshProUGUI SRC_RadarWidth;
    [SerializeField] private TextMeshProUGUI SRC_RadarRange;
    [Header("TRK_UI_Zone")]
    [SerializeField] private TextMeshProUGUI TRK_RadarState;
    public void Render_SRC(string type, string width, string range)
    {
        SRC_RadarState.text = type;
        SRC_RadarWidth.text = width;
        SRC_RadarRange.text = range;
    }
    public void Render_TRK(string type)
    {
        TRK_RadarState.text = type;
    }
}
