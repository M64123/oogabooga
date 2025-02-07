using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    [Header("Referencia al Slider de vida del enemigo")]
    public Slider enemyHealthSlider;

    // Referencia al enemigo; se asume que solo hay un enemigo en la escena o que
    // se actualizará este valor mediante otro método (por ejemplo, al iniciar el combate).
    private EnemyDinosaur enemy;

    void Start()
    {
        // Buscar el enemigo en la escena (o asignarlo de otra forma)
        enemy = FindObjectOfType<EnemyDinosaur>();

        if (enemy != null)
        {
            // Configuramos el valor máximo del slider con la salud máxima del enemigo
            enemyHealthSlider.maxValue = enemy.MaxHealth;
            enemyHealthSlider.value = enemy.CurrentHealth;
        }
        else
        {
            Debug.LogError("No se encontró un EnemyDinosaur en la escena.");
        }
    }

    void Update()
    {
        if (enemy != null)
        {
            // Actualiza el valor del slider para que refleje la vida actual del enemigo
            enemyHealthSlider.value = enemy.CurrentHealth;
        }
    }
}