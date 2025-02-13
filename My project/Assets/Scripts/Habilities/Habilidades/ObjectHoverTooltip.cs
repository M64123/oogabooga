using UnityEngine;
using System.Collections;

public class ObjectHoverTooltip : MonoBehaviour
{
    [Header("Configuración del tooltip")]
    [Tooltip("Prefab del tooltip que se instanciará (por ejemplo, una imagen con información).")]
    public GameObject tooltipPrefab;

    [Tooltip("Retraso (en segundos) antes de instanciar el tooltip.")]
    public float delay = 1.0f;

    [Tooltip("Offset en píxeles para posicionar el tooltip junto al objeto.")]
    public Vector2 offset = new Vector2(50f, 0f);

    // Referencia al tooltip instanciado.
    private GameObject tooltipInstance;
    // Referencia a la Coroutine para el retraso.
    private Coroutine hoverCoroutine;

    // Se requiere un Collider en el objeto para que estos métodos se disparen.
    private void OnMouseEnter()
    {
        hoverCoroutine = StartCoroutine(SpawnTooltipAfterDelay());
    }

    private void OnMouseExit()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        HideTooltip();
    }

    IEnumerator SpawnTooltipAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        // Encuentra el Canvas principal en la escena.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("ObjectHoverTooltip: No se encontró un Canvas en la escena.");
            yield break;
        }

        // Instancia el tooltip como hijo del Canvas.
        tooltipInstance = Instantiate(tooltipPrefab, canvas.transform);
        Debug.Log("ObjectHoverTooltip: Tooltip instanciado en el Canvas.");

        // Posiciona el tooltip en función de la posición del mouse.
        RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localPoint);
        tooltipRect.localPosition = localPoint + offset;
    }

    public void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }
}