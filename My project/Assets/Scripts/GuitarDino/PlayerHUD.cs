using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referencia al Slider de vida")]
    public Slider healthSlider; // Asigna este Slider en el Inspector

    // Referencia local al dino que se está monitoreando.
    private Dinosaurio currentDino;

    void Start()
    {
        // Verifica que el healthSlider esté asignado.
        if (healthSlider == null)
        {
            Debug.LogError("PlayerHUD: El healthSlider no está asignado en el Inspector.");
            return;
        }

        // Comprueba que el TeamManager exista.
        if (TeamManager.Instance == null)
        {
            Debug.LogError("PlayerHUD: TeamManager.Instance es null.");
            return;
        }

        // Si ya existe un dino activo, actualiza la referencia.
        if (TeamManager.Instance.activeDino != null)
        {
            currentDino = TeamManager.Instance.activeDino;
            healthSlider.maxValue = currentDino.MaxHealth;
            healthSlider.value = currentDino.CurrentHealth;
            Debug.Log("PlayerHUD: Se ha asignado el dino activo: " + currentDino.name);
        }
        else
        {
            Debug.LogWarning("PlayerHUD: No hay dino activo asignado en TeamManager al iniciar.");
        }
    }

    void Update()
    {
        // Si el Slider o el TeamManager son nulos, no hacemos nada.
        if (healthSlider == null || TeamManager.Instance == null)
            return;

        // Si el dino activo ha cambiado, actualizamos la referencia y el valor máximo.
        if (TeamManager.Instance.activeDino != currentDino)
        {
            currentDino = TeamManager.Instance.activeDino;
            if (currentDino != null)
            {
                healthSlider.maxValue = currentDino.MaxHealth;
                Debug.Log("PlayerHUD: Nuevo dino activo asignado: " + currentDino.name);
            }
            else
            {
                Debug.LogWarning("PlayerHUD: TeamManager.activeDino es null en Update.");
            }
        }

        // Si hay un dino activo, actualiza el valor del slider.
        if (currentDino != null)
        {
            healthSlider.value = currentDino.CurrentHealth;
        }
    }
}