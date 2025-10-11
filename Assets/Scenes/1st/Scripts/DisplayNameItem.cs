using TMPro;
using UnityEngine;

public class DisplayNameItem : MonoBehaviour
{
    [SerializeField] RayCast rayCastScript;
    [SerializeField] TextMeshProUGUI Text;
    void Start()
    {
        rayCastScript.OnRayCast += DisplayName;
    }
    private void DisplayName(string ItemName)
    {
        Text.text = ItemName;
    }
    private void OnDestroy()
    {
        rayCastScript.OnRayCast -= DisplayName;
    }

}
