using UnityEngine.EventSystems;
using UnityEngine;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public GameObject item;
    public int slotIndex; // Índice del slot en la grid

    void Start()
    {
        // Se puede asignar el índice dinámicamente si se maneja desde otro script
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!item)
        {
            item = DragHandler.itemDragging;
            item.transform.SetParent(transform);
            item.transform.position = transform.position;

            // Notificar la posición del objeto
            Debug.Log($"Objeto colocado en la posición: {slotIndex}");
        }
    }

    void Update()
    {
        if (item != null && item.transform.parent != transform)
        {
            item = null;
        }
    }
}
