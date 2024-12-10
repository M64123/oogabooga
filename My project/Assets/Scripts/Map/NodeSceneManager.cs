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

    // Estas dos escenas son las que muestran los dinos vivos y muertos
    [Header("Dino Scenes")]
    public string dinoShowcaseSceneName = "DinoShowcase";       // Asignar el nombre real de la escena en el Inspector
    public string deadDinoShowcaseSceneName = "DeadDinoShowcase"; // Asignar el nombre real de la escena en el Inspector

    // Escena principal (tablero)
    [Header("Board Scene")]
    public string boardSceneName = "Tablero"; // Asignar el nombre real de la escena del tablero en el Inspector

    // Singleton instance
    public static NodeSceneManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persistir entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para obtener una escena aleatoria basada en el NodeType (como antes)
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

    // Función para cargar la escena del tablero
    public void LoadBoardScene()
    {
        if (!string.IsNullOrEmpty(boardSceneName))
        {
            SceneManager.LoadScene(boardSceneName);
        }
        else
        {
            Debug.LogError("No se ha asignado la escena del tablero en el Inspector.");
        }
    }

    // Función para cargar la escena DinoShowcase (dinos vivos)
    public void LoadDinoShowcaseScene()
    {
        if (!string.IsNullOrEmpty(dinoShowcaseSceneName))
        {
            SceneManager.LoadScene(dinoShowcaseSceneName);
        }
        else
        {
            Debug.LogError("No se ha asignado la escena DinoShowcase en el Inspector.");
        }
    }

    // Función para cargar la escena DeadDinoShowcase (dinos muertos)
    public void LoadDeadDinoShowcaseScene()
    {
        if (!string.IsNullOrEmpty(deadDinoShowcaseSceneName))
        {
            SceneManager.LoadScene(deadDinoShowcaseSceneName);
        }
        else
        {
            Debug.LogError("No se ha asignado la escena DeadDinoShowcase en el Inspector.");
        }
    }
}
