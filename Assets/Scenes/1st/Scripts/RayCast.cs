using System;
using Unity.VisualScripting;
using UnityEngine;

public class RayCast : MonoBehaviour
{
    public event Action<string> OnRayCast;
    [Header("Mouse Sprite")]
    [SerializeField] Texture2D DefultMouse;
    [SerializeField] Texture2D DragMouse;
    [SerializeField] Texture2D DropMouse;
    [SerializeField] string RaylayerMask = "ray";
    const string TagObjectToDrag = "Drag";
    Transform DragObject;
    float hitDistance;

    ItemPosition itemPosition;
    [SerializeField] Vector3 dragOffset;
    private void Update()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit Hit, 500))
        {
            if (LayerMask.LayerToName(Hit.collider.gameObject.layer) == RaylayerMask)
            {
                OnRayCast?.Invoke(Hit.collider.name);

            }
            if (Input.GetMouseButtonDown(0))
            {
                if (Hit.collider.CompareTag(TagObjectToDrag))
                {
                    SetCursor(DropMouse);
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
            else
            {
                if (Hit.collider.CompareTag(TagObjectToDrag))
                    SetCursor(DragMouse);
            }
        }
        else
        {
            SetCursor(DefultMouse);
            OnRayCast?.Invoke("");
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            DragObject = null;
            if (itemPosition != null)
            {
                itemPosition.IsDragable = false;
                itemPosition.ActiveFade(false);
                ExperimentManager.Instance.CheckIfItemsInCorrectPosition(itemPosition);
                itemPosition = null;
            }
        }

        if (DragObject != null)
        {
            SetCursor(DropMouse);
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, hitDistance);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 endPos = worldPos + dragOffset;
            DragObject.position = endPos;
            //   DragObject.position = new Vector3(worldPos.x + dragOffset.x, worldPos.y + dragOffset.y, DragObject.position.z);
        }
    }
    void SetCursor(Texture2D texture2D)
    {
        Cursor.SetCursor(texture2D, Vector2.zero, CursorMode.Auto);
    }
}

