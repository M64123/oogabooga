using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUnit : MonoBehaviour
{
    private Vector3 startPosition;
    private Transform originalParent;
    private Canvas canvas;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPosition = transform.position;
        originalParent = transform.parent;
        transform.SetParent(canvas.transform, true);
        GetComponent<UnitMovement>().enabled = false; // Detener movimiento
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 newPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            Camera.main,
            out newPos);
        transform.position = new Vector3(newPos.x, transform.position.y, newPos.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Verificar si está sobre la arena
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.name == "Arena" && UnitManager.instance.CanPlaceUnit())
            {
                // Colocar la unidad en la arena
                transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                transform.SetParent(hit.collider.transform);
                UnitManager.instance.AddUnit();
                // Mantenerla congelada hasta que comience la pelea
            }
            else
            {
                // Devolver la unidad a su posición inicial
                transform.position = startPosition;
                transform.SetParent(originalParent);
                GetComponent<UnitMovement>().enabled = true; // Reanudar movimiento
            }
        }
        else
        {
            // Devolver la unidad a su posición inicial
            transform.position = startPosition;
            transform.SetParent(originalParent);
            GetComponent<UnitMovement>().enabled = true; // Reanudar movimiento
        }
    }
}
