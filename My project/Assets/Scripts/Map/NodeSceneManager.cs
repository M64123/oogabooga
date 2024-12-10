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

    [Header("Main Board Scene")]
    [Tooltip("Nombre de la escena principal del tablero.")]
    public string boardSceneName = "Tablero";

    [Header("Dino Scenes")]
    [Tooltip("Nombre de la escena donde se muestran los dinos vivos.")]
    public string dinoShowcaseSceneName = "DinoShowcase";
    [Tooltip("Nombre de la escena donde se muestran los dinos muertos.")]
    public string deadDinoShowcaseSceneName = "DeadDinoShowcase";

    // Singleton instance
    public static NodeSceneManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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

    public void LoadBoardScene()
    {
        if (!string.IsNullOrEmpty(boardSceneName))
        {
            SceneManager.LoadScene(boardSceneName);
        }
        else
        {
            Debug.LogError("boardSceneName no está asignado.");
        }
    }

    public void LoadDinoShowcaseScene()
    {
        if (!string.IsNullOrEmpty(dinoShowcaseSceneName))
        {
            SceneManager.LoadScene(dinoShowcaseSceneName);
        }
        else
        {
            Debug.LogError("dinoShowcaseSceneName no está asignado.");
        }
    }

    public void LoadDeadDinoShowcaseScene()
    {
        if (!string.IsNullOrEmpty(deadDinoShowcaseSceneName))
        {
            SceneManager.LoadScene(deadDinoShowcaseSceneName);
        }
        else
        {
            Debug.LogError("deadDinoShowcaseSceneName no está asignado.");
        }
    }
}
