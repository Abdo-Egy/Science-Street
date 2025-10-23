using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{
    [SerializeField] Color Color;

    [ContextMenu("Change")]
    void Start()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("color"))
        {
            if(item.TryGetComponent<Image>(out var image))
            {
                image.color = Color;
            }
        }    
    }
}
