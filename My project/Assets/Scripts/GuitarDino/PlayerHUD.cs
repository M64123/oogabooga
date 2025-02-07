using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referencia al Slider de vida del jugador")]
    public Slider playerHealthSlider;

    // Referencia al dinosaurio del jugador (se asume que es el que proviene del primer slot)
    private Dinosaurio playerDino;

    void Start()
    {
        // Buscamos el dinosaurio en la escena.
        playerDino = FindObjectOfType<Dinosaurio>();

        if (playerDino != null)
        {
            // Configuramos el slider con la salud máxima.
            playerHealthSlider.maxValue = playerDino.MaxHealth;
            playerHealthSlider.value = playerDino.CurrentHealth;
        }
        else
        {
            Debug.LogError("No se encontró el dinosaurio del jugador en la escena.");
        }
    }

    void Update()
    {
        if (playerDino != null)
        {
            // Actualizamos el slider para que refleje la salud actual del jugador.
            playerHealthSlider.value = playerDino.CurrentHealth;
        }
    }
}