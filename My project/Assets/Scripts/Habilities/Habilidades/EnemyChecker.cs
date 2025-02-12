using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyChecker : MonoBehaviour
{
    [Tooltip("Nombre de la escena del Tablero que se reiniciará si no hay enemigos.")]
    public string tableroSceneName = "Tablero";

    [Tooltip("Intervalo (en segundos) para chequear la existencia de enemigos.")]
    public float checkInterval = 1f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckEnemies();
        }
    }

    private void CheckEnemies()
    {
        // Busca todos los objetos con el tag "Enemigo".
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemigo");
        if (enemies.Length == 0)
        {
            Debug.Log("No quedan enemigos en la escena. Reiniciando la escena del Tablero...");
            SceneManager.LoadScene(tableroSceneName);
        }
    }
}