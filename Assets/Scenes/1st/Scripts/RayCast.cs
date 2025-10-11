using System;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    public event Action<string> OnRayCast;
    [SerializeField] LayerMask RaylayerMask;
    Transform DragObject;
    const string TagObjectToDrag = "Drag";
    float hitDistance;

    ItemPosition itemPosition;
    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit Hit, 500, RaylayerMask))
        {
            OnRayCast?.Invoke(Hit.collider.name);
            if (Input.GetMouseButtonDown(0))
            {
                if (Hit.collider.CompareTag(TagObjectToDrag))
                {
                    DragObject = Hit.collider.transform;
                    hitDistance = Hit.distance;
                    if (Hit.collider.TryGetComponent<ItemPosition>(out ItemPosition itemPosition))
                    {
                        itemPosition.IsDragable = true;
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
            if (itemPosition != null) itemPosition.IsDragable = false;
            ExperimentManager.Instance.CheckIfItemsInCorrectPosition();
        }

        if (DragObject != null)
        {
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitDistance);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            DragObject.position = new Vector3(worldPos.x, worldPos.y, DragObject.position.z);
        }
    }
}
