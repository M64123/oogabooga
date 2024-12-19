using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    private void Awake()
    {
        // Obtener todos los objetos en la escena actual
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // Verificar si el objeto pertenece a una escena diferente (DontDestroyOnLoad)
            if (obj.scene.name == null || obj.scene.name == "DontDestroyOnLoad")
            {
                Debug.Log($"Destruyendo objeto persistente: {obj.name}");
                Destroy(obj);
            }
        }
    }
}