using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CambioSpriteUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Sprite que se asigna en el Inspector para el estado "seleccionado"
    public Sprite spriteSeleccionado;

    // Referencias a componentes de UI
    private Image imageComponent;
    private RawImage rawImageComponent;

    // Variables para guardar el estado original
    private Sprite spriteOriginal;
    private Texture textureOriginal;

    void Start()
    {
        // Intentamos obtener ambos componentes
        imageComponent = GetComponent<Image>();
        rawImageComponent = GetComponent<RawImage>();

        if (imageComponent == null && rawImageComponent == null)
        {
            Debug.LogError("No se encontró ningún componente Image o RawImage en " + gameObject.name);
            return;
        }

        // Guardamos el sprite o textura original según el componente encontrado
        if (imageComponent != null)
        {
            spriteOriginal = imageComponent.sprite;
        }
        else if (rawImageComponent != null)
        {
            textureOriginal = rawImageComponent.texture;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SceneManager.LoadScene("Tablero");
        }
    }
    // Cuando el cursor entra en el área del elemento UI
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spriteSeleccionado == null)
        {
            Debug.LogWarning("No has asignado un sprite para 'spriteSeleccionado'.");
            return;
        }

        if (imageComponent != null)
        {
            imageComponent.sprite = spriteSeleccionado;
        }
        else if (rawImageComponent != null)
        {
            // Para RawImage se asigna la textura del sprite
            rawImageComponent.texture = spriteSeleccionado.texture;
        }
        
    }

    // Cuando el cursor sale del área del elemento UI
    public void OnPointerExit(PointerEventData eventData)
    {
        if (imageComponent != null)
        {
            imageComponent.sprite = spriteOriginal;
        }
        else if (rawImageComponent != null)
        {
            rawImageComponent.texture = textureOriginal;
        }
    }
    
}