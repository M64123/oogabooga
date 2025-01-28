using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public TextMeshProUGUI scoreText; // Asigna un TextMeshPro para mostrar la puntuación
    public AudioSource hitSFX; // Sonido para aciertos
    public AudioSource missSFX; // Sonido para fallos

    private int score = 0;

    private void Awake()
    {
        Instance = this;
    }

    public static void Hit()
    {
        if (Instance != null)
        {
            Instance.score += 100; // Incrementa la puntuación
            Instance.UpdateScoreText();
            if (Instance.hitSFX != null)
            {
                Instance.hitSFX.Play(); // Reproduce el sonido de acierto
            }
        }
    }

    public static void Miss()
    {
        if (Instance != null)
        {
            Instance.score -= 50; // Reduce la puntuación (opcional)
            Instance.UpdateScoreText();
            if (Instance.missSFX != null)
            {
                Instance.missSFX.Play(); // Reproduce el sonido de fallo
            }
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }
}