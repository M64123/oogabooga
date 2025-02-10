using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipUI : MonoBehaviour, IPointerExitHandler
{
    [Tooltip("Referencia al script del objeto que generó el tooltip.")]
    public UIHoverTooltip hoverTooltipScript;

    // Este método se llama cuando el puntero sale del área de este elemento de UI.
    public void OnPointerExit(PointerEventData eventData)
    {
        // Se oculta el tooltip llamando al método público del script del objeto.
        if (hoverTooltipScript != null)
        {
            hoverTooltipScript.HideTooltip();
        }
        else
        {
            // Si no se asignó la referencia, simplemente se destruye el tooltip.
            Destroy(gameObject);
        }
    }
}