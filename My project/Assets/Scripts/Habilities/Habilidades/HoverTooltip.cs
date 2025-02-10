using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración del tooltip")]
    [Tooltip("Prefab del tooltip (imagen) que se mostrará en la UI.")]
    public GameObject tooltipPrefab;

    [Tooltip("Retraso (en segundos) para mostrar el tooltip.")]
    public float delay = 1.0f;

    [Tooltip("Offset en píxeles para posicionar el tooltip junto al objeto.")]
    public Vector2 offset = new Vector2(50f, 0f);

    // Referencia al tooltip instanciado.
    private GameObject tooltipInstance;
    // Referencia al Coroutine para el retraso.
    private Coroutine hoverCoroutine;

    // En este script no mantenemos directamente la habilidad en un campo,
    // sino que la capturamos en el momento en que el puntero entra.

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Capturamos la habilidad antes de iniciar el delay.
        DinoAbility capturedAbility = GetAbilityFromThisObject();
        hoverCoroutine = StartCoroutine(ShowTooltipAfterDelay(capturedAbility));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        HideTooltip();
    }

    /// <summary>
    /// Intenta obtener la habilidad asociada a este objeto a partir del componente DinoImage.
    /// Se asume que el objeto tiene un DinoImage con un dinoID y que, mediante el GameManager,
    /// se puede obtener el prefab del dino, y a partir de él, la habilidad (por ejemplo, la primera).
    /// </summary>
    private DinoAbility GetAbilityFromThisObject()
    {
        DinoAbility result = null;
        // Obtén el componente que almacena el ID.
        DinoImage dinoImage = GetComponent<DinoImage>();
        if (dinoImage != null)
        {
            string dinoID = dinoImage.dinoID;
            Debug.Log("UIHoverTooltip: dinoID obtenido: " + dinoID);
            // Usa el GameManager para obtener el prefab del dino.
            GameObject dinoPrefab = GameManager.Instance.GetDinoPrefabByID(dinoID);
            if (dinoPrefab != null)
            {
                Dinosaurio dino = dinoPrefab.GetComponent<Dinosaurio>();
                if (dino != null && dino.habilities != null && dino.habilities.Length > 0)
                {
                    result = dino.habilities[0]; // Por ejemplo, usamos la primera habilidad.
                    Debug.Log("UIHoverTooltip: Habilidad capturada: " + result.abilityName);
                }
                else
                {
                    Debug.LogWarning("UIHoverTooltip: El prefab no tiene un Dinosaurio o no tiene habilidades asignadas.");
                }
            }
            else
            {
                Debug.LogWarning("UIHoverTooltip: No se encontró prefab para el dinoID " + dinoID);
            }
        }
        else
        {
            Debug.LogWarning("UIHoverTooltip: No se encontró el componente DinoImage en este objeto.");
        }
        return result;
    }

    IEnumerator ShowTooltipAfterDelay(DinoAbility abilityToShow)
    {
        yield return new WaitForSeconds(delay);

        // Encuentra el Canvas principal para instanciar el tooltip allí.
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("UIHoverTooltip: No se encontró un Canvas en la escena.");
            yield break;
        }

        // Instancia el tooltip como hijo del Canvas.
        tooltipInstance = Instantiate(tooltipPrefab, canvas.transform);
        Debug.Log("UIHoverTooltip: Tooltip instanciado como hijo del Canvas: " + canvas.name);

        // Posicionar el tooltip basado en la posición del ratón.
        RectTransform tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out localPoint);
        tooltipRect.localPosition = localPoint + offset;

        // Asigna la información de la habilidad al tooltip.
        AbilityTooltipUI tooltipUI = tooltipInstance.GetComponent<AbilityTooltipUI>();
        if (tooltipUI != null)
        {
            tooltipUI.SetAbility(abilityToShow);
        }
        else
        {
            Debug.LogWarning("UIHoverTooltip: No se encontró el componente AbilityTooltipUI en el tooltip.");
        }
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