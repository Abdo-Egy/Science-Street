using ALArcade.ArabicTMP;
using TMPro;
using UnityEngine;

public class DisplayNameItem : MonoBehaviour
{
    [SerializeField] RayCast rayCastScript;
    [SerializeField] ArabicTextMeshProUGUI Text;
    void Start()
    {
        rayCastScript.OnRayCast += DisplayName;
    }
    private void DisplayName(string ItemName)
    {
        Text.arabicText = ItemName;
    }
    private void OnDestroy()
    {
        rayCastScript.OnRayCast -= DisplayName;
    }

}
