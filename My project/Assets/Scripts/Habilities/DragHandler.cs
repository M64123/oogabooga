using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragHandler : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public static GameObject itemDragging;
    private Transform startParent;
    private Transform dragParent;
    private Vector3 startPosition;
    private CanvasGroup canvasGroup;
    private Canvas rootCanvas; // Referencia al canvas principal

    void Start()
    {
        dragParent = GameObject.FindGameObjectWithTag("DragParent")?.transform;
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Buscar el Canvas raíz más cercano para asegurarse de que el objeto esté en el nivel más alto
        rootCanvas = GetComponentInParent<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError("No se encontró un Canvas en los padres de " + gameObject.name);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        itemDragging = gameObject;
        startPosition = transform.position;
        startParent = transform.parent;

        // Asegurar que el objeto se mueve al frente del Canvas
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling(); // Lo mueve al frente de la UI

        // Hacer que no bloquee eventos debajo
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f; // Opción visual de transparencia
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        itemDragging = null;

        // Si no se suelta en un DropSlot, vuelve a la posición original
        if (transform.parent == rootCanvas.transform)
        {
            transform.position = startPosition;
            transform.SetParent(startParent);
        }

        // Restaurar visibilidad y reactivar eventos
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }
}