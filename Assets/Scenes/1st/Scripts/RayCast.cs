using System;
using Unity.VisualScripting;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    public event Action<string> OnRayCast;
    [SerializeField] LayerMask RaylayerMask;
    Transform DragObject;
    const string TagObjectToDrag = "Drag";
    float hitDistance;

    ItemPosition itemPosition;
    [SerializeField] Vector3 dragOffset;
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit Hit, Mathf.Infinity))
        {
            OnRayCast?.Invoke(Hit.collider.name);

            if (Input.GetMouseButtonDown(0))
            {
                if (Hit.collider.CompareTag(TagObjectToDrag))
                {
                    DragObject = Hit.collider.transform;
                    dragOffset = DragObject.position - Hit.point;

                    hitDistance = Hit.distance;
                    if (Hit.collider.TryGetComponent<ItemPosition>(out ItemPosition itemPosition))
                    {
                        itemPosition.IsDragable = true;
                        itemPosition.ActiveFade(true);
                        this.itemPosition = itemPosition;
                    }
                }
            }
        }
        else
        {
            OnRayCast?.Invoke("");
        }

        if (Input.GetMouseButtonUp(0))
        {
            DragObject = null;
            if (itemPosition != null)
            {
               itemPosition.IsDragable = false;
                itemPosition.ActiveFade(false);
                ExperimentManager.Instance.CheckIfItemsInCorrectPosition();
                itemPosition = null;
            }
            
        }

        if (DragObject != null)
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitDistance);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 endPos = worldPos + dragOffset;
            DragObject.position = endPos;
         //   DragObject.position = new Vector3(worldPos.x + dragOffset.x, worldPos.y + dragOffset.y, DragObject.position.z);
        }
    }
}
