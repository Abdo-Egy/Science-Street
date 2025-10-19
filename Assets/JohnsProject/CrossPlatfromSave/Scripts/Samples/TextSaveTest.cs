using UnityEngine;
using UnityEngine.UI;
using JohnsProject.CrossPlatformSave;

public class TextSaveTest : MonoBehaviour
{

    [SerializeField]
    private DataToSave dataToSave;
    [SerializeField]
    private InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        CrossPlatformSaveData saveData = CrossPlatformSaveManager.Instance.SaveData;
        dataToSave = saveData.dataToSave;
        Debug.Log("loaded : " + dataToSave.text);
        inputField.text = dataToSave.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveData() {
        dataToSave.text = inputField.text;
        Debug.Log("saving : " + dataToSave.text);
        CrossPlatformSaveManager.Instance.Save();
    }
}
