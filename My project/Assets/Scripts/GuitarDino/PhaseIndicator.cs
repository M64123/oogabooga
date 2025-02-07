using UnityEngine;

public class PhaseIndicator : MonoBehaviour
{
    // Sprite que se muestra cuando estamos en fase de ataque (true)
    public Sprite ataqueSprite;
    // Sprite que se muestra cuando estamos en fase de defensa (false)
    public Sprite defensaSprite;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Se asume que este GameObject tiene un componente SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Se actualiza la imagen de forma inmediata al iniciar
        UpdateIndicator();
    }

    void Update()
    {
        // Cada frame se consulta el estado de la fase y se actualiza el sprite
        UpdateIndicator();
    }

    void UpdateIndicator()
    {
        // Se asume que MeasureManager es un singleton con la propiedad IsAtaque
        if (MeasureManager.Instance != null)
        {
            if (MeasureManager.Instance.IsAtaque)
            {
                spriteRenderer.sprite = ataqueSprite;
            }
            else
            {
                spriteRenderer.sprite = defensaSprite;
            }
        }
    }
}