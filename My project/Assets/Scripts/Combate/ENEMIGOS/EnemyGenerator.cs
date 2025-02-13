using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [Header("Global Settings")]
    [Tooltip("Valor global de peligro. Se incrementa cada vez que se abre un huevo en el gacha.")]
    public int globalPeligro = 1;

    [Header("Spawn Settings")]
    [Tooltip("Transform del punto donde se instanciarán los enemigos.")]
    public Transform enemySpawnPoint;

    [Header("Pools de IDs por Peligro")]
    [Tooltip("Lista de IDs para dinos con peligro 1.")]
    public List<string> peligro1DinoIDs;
    [Tooltip("Lista de IDs para dinos con peligro 2.")]
    public List<string> peligro2DinoIDs;

    // Lista para guardar las instancias generadas.
    public List<GameObject> spawnedEnemies = new List<GameObject>();

    void Start()
    {
        // Llama a GenerateEnemies() al iniciar el combate.
        GenerateEnemies();
    }

    /// <summary>
    /// Genera enemigos según el valor global de peligro.
    /// Limpia la lista de enemigos previamente generados para reiniciar la generación.
    /// </summary>
    public void GenerateEnemies()
    {
        // Limpia y destruye las instancias previas (si existen)
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();

        // Genera una combinación de números que sumen globalPeligro.
        List<int> combination = GenerateCombination(globalPeligro);
        Debug.Log("Global peligro: " + globalPeligro + ". Combinación generada: " + string.Join(", ", combination));

        // Por cada valor de la combinación, selecciona aleatoriamente un ID según el peligro.
        foreach (int peligro in combination)
        {
            string selectedID = "";
            if (peligro == 1 && peligro1DinoIDs.Count > 0)
            {
                int index = Random.Range(0, peligro1DinoIDs.Count);
                selectedID = peligro1DinoIDs[index];
            }
            else if (peligro == 2 && peligro2DinoIDs.Count > 0)
            {
                int index = Random.Range(0, peligro2DinoIDs.Count);
                selectedID = peligro2DinoIDs[index];
            }

            if (!string.IsNullOrEmpty(selectedID))
            {
                // Obtiene el prefab del enemigo a partir del GameManager.
                GameObject prefab = GameManager.Instance.GetDinoPrefabByID(selectedID);
                if (prefab != null)
                {
                    // Instancia el enemigo en el enemySpawnPoint.
                    GameObject enemy = Instantiate(prefab, enemySpawnPoint.position, enemySpawnPoint.rotation);

                    // (Opcional) Ajusta escala o rotación si es necesario.
                    // Por ejemplo, puedes modificar la escala o el flip según tu diseño.

                    // Asigna el tag "Enemigo" para que otros sistemas lo detecten.
                    enemy.tag = "Enemigo";

                    // Asegura que tenga el script EnemyDinosaur (si no lo tiene, lo añade).
                    if (enemy.GetComponent<EnemyDinosaur>() == null)
                    {
                        enemy.AddComponent<EnemyDinosaur>();
                    }

                    // Añade el enemigo a la lista.
                    spawnedEnemies.Add(enemy);
                    Debug.Log("Enemy generated with ID: " + selectedID);
                }
                else
                {
                    Debug.LogWarning("EnemyGenerator: No se encontró prefab para el ID: " + selectedID);
                }
            }
        }
    }

    /// <summary>
    /// Genera una lista de enteros (1 y 2) que sumen exactamente el valor total.
    /// </summary>
    /// <param name="total">El valor total de peligro.</param>
    /// <returns>Lista de enteros.</returns>
    List<int> GenerateCombination(int total)
    {
        List<int> combination = new List<int>();
        int remaining = total;
        while (remaining > 0)
        {
            int val = (remaining >= 2) ? ((Random.value < 0.5f) ? 1 : 2) : 1;
            combination.Add(val);
            remaining -= val;
        }
        return combination;
    }
}
