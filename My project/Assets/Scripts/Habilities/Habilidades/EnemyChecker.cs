using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyChecker : MonoBehaviour
{
    [Tooltip("Nombre de la escena del Tablero que se reiniciará si no hay enemigos.")]
    public string tableroSceneName = "Tablero";

    [Tooltip("Intervalo (en segundos) para chequear la existencia de enemigos.")]
    public float checkInterval = 1f;

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckEnemies();
        }
    }

    void CheckEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemigo");
        if (enemies.Length == 0)
        {
            Debug.Log("EnemyChecker: No enemies left. Resetting Tablero scene from scratch.");
            // Reinicia los datos del GameManager antes de reiniciar la escena.
            GameManager.Instance.ResetGameData();
            SceneManager.LoadScene(tableroSceneName, LoadSceneMode.Single);
        }
    }
}
