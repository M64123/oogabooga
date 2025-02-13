using UnityEngine;
using UnityEngine.UI;  // Necesario para trabajar con Canvas

public class PopupTrigger : MonoBehaviour
{
    // Objeto UI que funcionará como popup (por ejemplo, un Panel)
    public GameObject popupUI;
    // Canvas en el que se instanciará el popup. Si no se asigna, se usará el que tenga por defecto el popupUI.
    public Canvas targetCanvas;
    // Desfase en píxeles para posicionar el popup respecto al objeto (puedes ajustarlo en el inspector)
    public Vector2 offset = new Vector2(50, 50);

    private Camera mainCamera;
    private RectTransform popupRectTransform;

    void Start()
    {
        // Obtenemos la cámara principal
        mainCamera = Camera.main;

        if (popupUI != null)
        {
            // Obtenemos el RectTransform del popup
            popupRectTransform = popupUI.GetComponent<RectTransform>();

            // Si se ha asignado un canvas específico, reparentamos el popupUI a ese canvas
            if (targetCanvas != null)
            {
                popupUI.transform.SetParent(targetCanvas.transform, false);
            }

            // Nos aseguramos de que el popup esté oculto al inicio
            popupUI.SetActive(false);
        }
        else
        {
            Debug.LogError("No se ha asignado el objeto popupUI en " + gameObject.name);
        }
    }

    // Se ejecuta cuando el ratón entra en el collider del objeto
    void OnMouseEnter()
    {
        if (popupUI != null)
        {
            // Convertimos la posición del objeto en coordenadas de pantalla
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            // Añadimos un desfase para que el popup aparezca al lado
            screenPos += new Vector3(offset.x, offset.y, 0);
            // Asignamos la posición al popup
            popupRectTransform.position = screenPos;
            // Mostramos el popup
            popupUI.SetActive(true);
        }
    }

    // Se ejecuta cuando el ratón sale del collider del objeto
    void OnMouseExit()
    {
        if (popupUI != null)
        {
            // Ocultamos el popup
            popupUI.SetActive(false);
        }
    }
}