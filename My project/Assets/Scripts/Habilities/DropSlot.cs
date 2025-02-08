using UnityEngine;
using UnityEngine.EventSystems;

public class DropSlot : MonoBehaviour, IDropHandler
{
    public GameObject item;
    public int slotIndex; // Índice del slot en la grid

    void Start()
    {
        // Opcional: se puede asignar el índice dinámicamente si se maneja desde otro script.
    }

    public void OnDrop(PointerEventData eventData)
    {
        // Si el slot está vacío, asigna el objeto que se está arrastrando.
        if (!item)
        {
            item = DragHandler.itemDragging;
            item.transform.SetParent(transform);
            item.transform.position = transform.position;

            Debug.Log($"Objeto colocado en la posición: {slotIndex}");
        }
    }

    void Update()
    {
        // Si el objeto ya no está en este slot, se limpia la referencia.
        if (item != null && item.transform.parent != transform)
        {
            item = null;
        }
    }
}