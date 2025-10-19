using UnityEngine;
using JohnsProject.CrossPlatformSave;

public class RotationSaveTest : MonoBehaviour
{
    [SerializeField]
    private DataToSave dataToSave;
    [SerializeField]
    private Transform transformToSave;

    // Start is called before the first frame update
    void Start()
    {
        CrossPlatformSaveData saveData = CrossPlatformSaveManager.Instance.SaveData;
        dataToSave = saveData.dataToSave;
        Debug.Log("loaded : " + dataToSave.rotation);
        transformToSave.rotation = dataToSave.rotation;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SaveData()
    {
        dataToSave.rotation = transformToSave.rotation;
        Debug.Log("saving : " + dataToSave.rotation);
        CrossPlatformSaveManager.Instance.Save();
    }
}
