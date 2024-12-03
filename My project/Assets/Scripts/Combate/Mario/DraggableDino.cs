using UnityEngine;

public class DraggableDino3D : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Transform closestSlot;

    private void OnMouseDown()
    {
        // Guarda la posici�n original
        originalParent = transform.parent;
        originalPosition = transform.position;

        // Activa el modo de arrastre
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;

        // Aseg�rate de que el dinosaurio sea visible durante el arrastre
        transform.position += Vector3.up * 0.5f; // Elevarlo ligeramente
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // Mueve el dinosaurio seg�n el cursor del mouse
            transform.position = GetMouseWorldPosition() + offset;

            // Busca el slot m�s cercano mientras arrastras
            closestSlot = FindClosestSlot();
            if (closestSlot != null)
            {
                // Previsualiza el snap al slot m�s cercano
                transform.position = closestSlot.position + Vector3.up * 0.5f; // Elevado para indicar previsualizaci�n
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (closestSlot != null && closestSlot.childCount == 0)
        {
            // Coloca el dinosaurio en el slot m�s cercano
            transform.SetParent(closestSlot);
            transform.localPosition = Vector3.zero;

            // Notifica a la grid para asegurarse de que haya suficientes slots
            Combatgrid grid = closestSlot.GetComponentInParent<Combatgrid>();
            grid.EnsureEnoughSlots();
        }
        else
        {
            // Si no est� sobre un slot, regresa a la posici�n original
            transform.position = originalPosition;
            transform.SetParent(originalParent);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        // Convierte la posici�n del cursor en coordenadas del mundo
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    private Transform FindClosestSlot()
    {
        float closestDistance = Mathf.Infinity;
        Transform bestSlot = null;

        GameObject[] slots = GameObject.FindGameObjectsWithTag("Slot");
        foreach (GameObject slot in slots)
        {
            float distance = Vector3.Distance(transform.position, slot.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestSlot = slot.transform;
            }

            // Cambiar el material del slot m�s cercano
            slot.GetComponent<Renderer>().material.color = Color.white; // Resetear color
        }

        if (bestSlot != null)
        {
            bestSlot.GetComponent<Renderer>().material.color = Color.green; // Resaltar el m�s cercano
        }

        return bestSlot;
    }
}
