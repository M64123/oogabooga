using UnityEngine;
using System.Collections.Generic;

public class DinoSceneManager : MonoBehaviour
{
    public bool showDeadDinos = false;
    public GameObject movementArea; // Debe tener BoxCollider
    public List<DinoIDPrefab> dinoPrefabs;

    [System.Serializable]
    public class DinoIDPrefab
    {
        public string dinoID;
        public GameObject prefab;
    }

    void Start()
    {
        var gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("No hay GameManager.");
            return;
        }

        Dictionary<string, GameManager.DinoData> allDinos = gm.GetAllPlayerDinos();
        List<GameManager.DinoData> dinosToShow = new List<GameManager.DinoData>();

        // Ajustar esta lógica según tu criterio de vivos/muertos:
        // Ejemplo: Common, Rare = vivos / ShinyCommon, ShinyRare = muertos
        foreach (var kvp in allDinos)
        {
            bool isDinoAlive = (kvp.Value.rarity == Rarity.Common || kvp.Value.rarity == Rarity.Rare);
            if (showDeadDinos && !isDinoAlive) dinosToShow.Add(kvp.Value);
            if (!showDeadDinos && isDinoAlive) dinosToShow.Add(kvp.Value);
        }

        if (movementArea == null)
        {
            Debug.LogError("Asigna 'movementArea' en el Inspector.");
            return;
        }

        BoxCollider areaCollider = movementArea.GetComponent<BoxCollider>();
        if (areaCollider == null)
        {
            Debug.LogError("movementArea no tiene BoxCollider.");
            return;
        }

        Bounds bounds = areaCollider.bounds;

        // Instanciar dinos
        foreach (var dinoData in dinosToShow)
        {
            GameObject prefab = GetDinoPrefab(dinoData.dinoID);
            if (prefab == null)
            {
                Debug.LogWarning($"No se encontró prefab para {dinoData.dinoID}");
                continue;
            }

            float xPos = Random.Range(bounds.min.x, bounds.max.x);
            float zPos = Random.Range(bounds.min.z, bounds.max.z);
            float yPos = movementArea.transform.position.y;

            Vector3 spawnPos = new Vector3(xPos, yPos, zPos);
            GameObject dinoInstance = Instantiate(prefab, spawnPos, Quaternion.identity);

            // Asignar DinoMovement
            DinoMovement dm = dinoInstance.GetComponent<DinoMovement>();
            if (dm == null)
            {
                // Si el prefab no lo tiene, se lo agregamos por código
                dm = dinoInstance.AddComponent<DinoMovement>();
            }
            dm.SetMovementBounds(bounds);
        }
    }

    GameObject GetDinoPrefab(string dinoID)
    {
        foreach (var dp in dinoPrefabs)
        {
            if (dp.dinoID == dinoID)
            {
                return dp.prefab;
            }
        }
        return null;
    }
}
