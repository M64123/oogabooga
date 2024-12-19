using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableDinoPlatform : MonoBehaviour
{
    private Vector3 initialPosition;      // Posición inicial del dino
    private Transform initialParent;      // Parent inicial del dino
    private Transform closestSlot;        // Slot más cercano
    private bool isDragging = false;      // Estado del drag

    public float snapThreshold = 2.0f;    // Distancia para snap al slot
    public Combatgrid combatGrid;         // Referencia a la grid
    public float returnSpeed = 5.0f;      // Velocidad de retorno
    public float maxParabolaHeight = 2.0f; // Altura máxima de la parábola

    private PlatformMover platformMover;   // Referencia al PlatformMover

    private void Start()
    {
        initialPosition = transform.position;
        initialParent = transform.parent;

        if (combatGrid == null)
        {
            combatGrid = FindObjectOfType<Combatgrid>();
            if (combatGrid == null)
            {
                Debug.LogError("Combatgrid no encontrado en la escena.");
            }
        }

        platformMover = FindObjectOfType<PlatformMover>();
        if (platformMover == null)
        {
            Debug.LogError("PlatformMover no encontrado en la escena.");
        }
    }

    private void OnMouseDown()
    {
        if (platformMover != null && platformMover.CombatHasStarted())
        {
            return; // No permitir arrastrar si el combate ha comenzado
        }

        isDragging = true;

        if (combatGrid != null)
        {
            combatGrid.EnsureSlotAvailability();
        }
        else
        {
            Debug.LogWarning("Combatgrid no asignado. No se pueden generar nuevos slots.");
        }

        transform.SetParent(null);
    }

    private void OnMouseDrag()
    {
        if (platformMover != null && platformMover.CombatHasStarted())
        {
            return; // No permitir arrastrar si el combate ha comenzado
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        // Limitar el valor de Z
        float clampedZ = Mathf.Min(mouseWorldPosition.z, 2.58f);

        // Actualizar la posición, limitando Z
        transform.position = new Vector3(mouseWorldPosition.x, transform.position.y, clampedZ);

        closestSlot = FindClosestSlot();
        HighlightSlot(closestSlot);
    }

    private void OnMouseUp()
    {
        if (platformMover != null && platformMover.CombatHasStarted())
        {
            return; // No permitir arrastrar si el combate ha comenzado
        }

        isDragging = false;

        if (closestSlot != null && closestSlot.childCount == 0)
        {
            SnapToSlot(closestSlot);
        }
        else
        {
            ReturnToOriginWithParabola();
        }

        RemoveSlotHighlight(closestSlot);

        if (combatGrid != null)
        {
            combatGrid.RemoveTemporarySlots();
        }
    }

    private void SnapToSlot(Transform slot)
    {
        transform.SetParent(slot);
        transform.localPosition = Vector3.zero;
    }

    private void ReturnToOriginWithParabola()
    {
        StopAllCoroutines(); // Detener cualquier movimiento anterior
        StartCoroutine(MoveToOriginWithParabola());
    }

    private System.Collections.IEnumerator MoveToOriginWithParabola()
    {
        Vector3 startPoint = transform.position;
        Vector3 endPoint = initialPosition;

        float distance = Vector3.Distance(startPoint, endPoint);
        float height = Mathf.Clamp(distance / 2, 0.5f, maxParabolaHeight);
        float speed = Mathf.Clamp(returnSpeed / distance, 1.0f, 10.0f);

        float t = 0;
        while (t < 1.0f)
        {
            t += Time.deltaTime * speed;
            Vector3 currentPosition = Vector3.Lerp(startPoint, endPoint, t);

            // Aplicar altura parabólica
            currentPosition.y += height * (1 - t) * t * 4; // Fórmula para parábola
            transform.position = currentPosition;

            yield return null;
        }

        transform.position = endPoint;
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
