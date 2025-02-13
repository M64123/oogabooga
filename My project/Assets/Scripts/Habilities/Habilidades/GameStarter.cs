using UnityEngine;
using UnityEngine.UI;

public class GameStarter : MonoBehaviour
{
    // Asigna estos campos desde el Inspector
    public GameObject pauseCanvas;
    public Button resumeButton;

    void Start()
    {
        // Pausar el juego al inicio
        Time.timeScale = 0f;

        // Agregar la función ResumeGame al evento OnClick del botón
        resumeButton.onClick.AddListener(ResumeGame);
    }

    void ResumeGame()
    {
        // Reanudar el juego
        Time.timeScale = 1f;
        // Desactivar el Canvas para que desaparezca
        pauseCanvas.SetActive(false);
    }
}