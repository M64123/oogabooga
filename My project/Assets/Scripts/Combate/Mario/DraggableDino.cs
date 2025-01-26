using UnityEngine;

public class DraggableDino3D : MonoBehaviour
{
    private Vector3 initialPosition;      // Posición inicial del dino
    private Transform initialParent;     // Parent inicial del dino
    private Transform closestSlot;       // Slot más cercano
    private bool isDragging = false;     // Estado del drag

    public float snapThreshold = 2.0f;   // Distancia para snap al slot
    public Combatgrid combatGrid;        // Referencia a la grid

    private void Start()
    {
        initialPosition = transform.position;
        initialParent = transform.parent;

        if (combatGrid == null)
        {
            combatGrid = FindObjectOfType<Combatgrid>();
        }
    }

    private void OnMouseDown()
    {
        isDragging = true;

        // Asegúrate de que hay suficiente espacio en la grid
        combatGrid.EnsureSlotAvailability();

        // Desvincular temporalmente
        transform.SetParent(null);
    }

    private void OnMouseDrag()
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        transform.position = new Vector3(mouseWorldPosition.x, transform.position.y, mouseWorldPosition.z);

        closestSlot = FindClosestSlot();
        HighlightSlot(closestSlot);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (closestSlot != null && closestSlot.childCount == 0)
        {
            SnapToSlot(closestSlot);
        }
        else
        {
            ReturnToOrigin();
        }

        RemoveSlotHighlight(closestSlot);

        // Verifica y elimina slots temporales no ocupados
        combatGrid.RemoveTemporarySlots();
    }

    private void SnapToSlot(Transform slot)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
    }

    private void ReturnToOrigin()
    {
        transform.position = initialPosition;
        transform.SetParent(initialParent);
    }

    private Transform FindClosestSlot()
    {
        GameObject[] slots = GameObject.FindGameObjectsWithTag("Slot");
        Transform bestSlot = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject slot in slots)
        {
            float distance = Vector3.Distance(transform.position, slot.transform.position);
            if (distance < closestDistance && distance <= snapThreshold)
            {
                closestDistance = distance;
                bestSlot = slot.transform;
            }
        }

        return bestSlot;
    }

    private void HighlightSlot(Transform slot)
    {
        if (slot != null)
        {
            Renderer renderer = slot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green; // Resalta el slot
            }
        }
    }

    private void RemoveSlotHighlight(Transform slot)
    {
        if (slot != null)
        {
            Renderer renderer = slot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.white; // Quita el resaltado
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Plane plane = new Plane(Vector3.up, 0);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}