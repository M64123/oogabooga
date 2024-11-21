using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NodeSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class NodeTypeScenes
    {
        public NodeType nodeType;
        public List<string> sceneNames;
    }

    public List<NodeTypeScenes> nodeTypeScenesList = new List<NodeTypeScenes>();

    // Singleton instance
    public static NodeSceneManager Instance;




    void Awake()
    {
        // Configurar el patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional si deseas que persista entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para obtener una escena aleatoria basada en el NodeType
    public string GetRandomSceneName(NodeType nodeType)
    {
        foreach (var nts in nodeTypeScenesList)
        {
            if (nts.nodeType == nodeType)
            {
                if (nts.sceneNames.Count > 0)
                {
                    int index = Random.Range(0, nts.sceneNames.Count);
                    return nts.sceneNames[index];
                }
                else
                {
                    Debug.LogWarning($"No hay escenas asignadas para el tipo de nodo {nodeType}.");
                    return null;
                }
            }
        }
        Debug.LogWarning($"No se encontró una entrada para el tipo de nodo {nodeType}.");
        return null;
    }
}
