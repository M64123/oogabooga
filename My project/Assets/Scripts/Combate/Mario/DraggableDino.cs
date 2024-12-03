using UnityEngine;

public class DraggableDino3D : MonoBehaviour
{
    private Vector3 initialPosition;
    private Transform initialParent;
    private Vector3 offset;
    private bool isDragging = false;
    private Transform closestSlot;
    private Combatgrid combatGrid;

    public float elevation = 0.5f;
    public float smoothSpeed = 10f;

    private void Start()
    {
        // Guarda la posición y el parent iniciales
        initialPosition = transform.position;
        initialParent = transform.parent;

        // Encuentra el script CombatGrid en la escena
        combatGrid = FindObjectOfType<Combatgrid>();
    }

    private void OnMouseDown()
    {
        // Desvincula el objeto temporalmente
        transform.SetParent(null);

        // Calcula el offset
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;

        // Eleva el objeto para feedback visual
        transform.position += Vector3.up * elevation;
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // Calcula la nueva posición basada en el mouse
            Vector3 targetPosition = GetMouseWorldPosition() + offset;

            // Suaviza el movimiento
            transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, elevation, targetPosition.z), Time.deltaTime * smoothSpeed);

            // Encuentra el slot más cercano
            closestSlot = FindClosestSlot();

            // Asegúrate de que haya suficientes slots
            combatGrid.EnsureEnoughSlots();
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;

        if (closestSlot != null && closestSlot.childCount == 0)
        {
            // Coloca el objeto en el slot más cercano
            transform.SetParent(closestSlot);
            transform.localPosition = Vector3.zero;
        }
        else
        {
            // Regresa el objeto a su posición inicial
            ReturnToInitialPosition();
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Plane gridPlane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (gridPlane.Raycast(ray, out float enter))
        {
            return ray.GetPoint(enter);
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

            // Cambiar el color de los slots
            Renderer renderer = slot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.white;
            }
        }

        if (bestSlot != null)
        {
            Renderer renderer = bestSlot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.green; // Resalta el slot más cercano
            }
        }

        return bestSlot;
    }

    private void ReturnToInitialPosition()
    {
        // Devuelve el objeto a su posición inicial
        transform.position = initialPosition;
        transform.SetParent(initialParent);
    }
}

