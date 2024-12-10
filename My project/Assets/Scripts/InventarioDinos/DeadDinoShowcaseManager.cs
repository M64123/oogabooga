using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DeadDinoShowcaseManager : MonoBehaviour
{
    [Header("Area de Spawn (Muertos)")]
    public Collider spawnArea;
    public string boardSceneName = "Tablero";

    void Start()
    {
        SpawnDinos();
    }

    void SpawnDinos()
    {
        var dinos = GameManager.Instance.GetAllPlayerDinos();
        foreach (var kvp in dinos)
        {
            var dinoData = kvp.Value;
            if (!dinoData.isAlive) // Solo los muertos
            {
                GameObject prefab = GameManager.Instance.GetDinoPrefabByID(dinoData.dinoID);
                if (prefab != null)
                {
                    Vector3 randomPos = GetRandomPositionInsideCollider(spawnArea);
                    GameObject dinoObj = Instantiate(prefab, randomPos, Quaternion.identity);

                    // Para dinos muertos, podrías no añadir UnitIdleMovement o añadirlo sin moverse:
                    // Si quieres que no se muevan, no agregues UnitIdleMovement.
                    // dinoObj no se mueve.
                }
            }
        }
    }

    Vector3 GetRandomPositionInsideCollider(Collider col)
    {
        BoxCollider box = col as BoxCollider;
        if (box == null)
        {
            Debug.LogError("spawnArea no es BoxCollider.");
            return col.transform.position;
        }

        Vector3 size = box.size;
        Vector3 center = box.center;
        Vector3 worldCenter = box.transform.TransformPoint(center);
        Vector3 halfSize = size * 0.5f;
        Vector3 randomOffset = new Vector3(
            Random.Range(-halfSize.x, halfSize.x),
            Random.Range(-halfSize.y, halfSize.y),
            Random.Range(-halfSize.z, halfSize.z)
        );
        Vector3 randomPos = worldCenter + box.transform.TransformDirection(randomOffset);
        return randomPos;
    }

    public void OnBackToBoardButton()
    {
        SceneManager.LoadScene(boardSceneName);
    }
}
