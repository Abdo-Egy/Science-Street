using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ForceRigUpdate : MonoBehaviour
{
    [SerializeField] RigBuilder rigBuilder;

    private void Update()
    {
        rigBuilder.Build();
    }
    void LateUpdate()
    {
       
    }
}
