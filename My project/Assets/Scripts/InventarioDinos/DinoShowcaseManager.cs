using UnityEngine;
using System.Collections.Generic;

public class DinoShowcaseManager : MonoBehaviour
{
    public Transform dinosParent; // Para organizar los dinosaurios en la jerarquía
    public float areaSize = 10f;  // Tamaño del área donde se generarán los dinosaurios

    void Start()
    {
        GenerateDinosaurs();
    }

    void GenerateDinosaurs()
    {
        List<Dinosaur> playerDinos = GameManager.Instance.playerDinosaurs;

        foreach (Dinosaur dino in playerDinos)
        {
            // Generar una posición aleatoria dentro del área
            Vector3 spawnPosition = GetRandomPosition();

            // Instanciar el prefab del dinosaurio
            GameObject dinoObj = Instantiate(dino.prefab, spawnPosition, Quaternion.identity, dinosParent);

            // Añadir el script de movimiento (asegúrate de que el prefab tiene Animator)
            /*if (dinoObj.GetComponent<UnitIdleMovement>() == null)
            {
                dinoObj.AddComponent<UnitIdleMovement>();
            }
            */
            // Mostrar el nombre del dinosaurio encima
            GameObject nameTag = CreateNameTag(dino.name);
            nameTag.transform.SetParent(dinoObj.transform);
            nameTag.transform.localPosition = new Vector3(0, 2f, 0); // Ajusta la posición según sea necesario
        }
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-areaSize / 2f, areaSize / 2f);
        float z = Random.Range(-areaSize / 2f, areaSize / 2f);
        float y = 0f; // Asumiendo que el terreno está en y = 0

        return new Vector3(x, y, z);
    }

    GameObject CreateNameTag(string name)
    {
        GameObject nameTag = new GameObject("NameTag");
        TextMesh textMesh = nameTag.AddComponent<TextMesh>();
        textMesh.text = name;
        textMesh.fontSize = 24;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.characterSize = 0.1f;
        textMesh.color = Color.white;

        return nameTag;
    }
}
