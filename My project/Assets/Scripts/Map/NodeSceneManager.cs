using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NodeSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class NodeTypeScenes
    {
        public NodeType nodeType;
        public List<string> sceneNames;
    }

    public List<NodeTypeScenes> nodeTypeScenesList = new List<NodeTypeScenes>();

    [Header("Dino Scenes")]
    public string dinoShowcaseSceneName = "DinoShowcase";
    public string deadDinoShowcaseSceneName = "DeadDinoShowcase";

    [Header("Board Scene")]
    public string boardSceneName = "Tablero";

    private Button buttonLoadDinoShowcase; // Botón para cargar DinoShowcase
    private Button buttonLoadBoardScene;  // Botón para cargar Tablero

    public static NodeSceneManager Instance;

    private void Awake()
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == boardSceneName)
        {
            Debug.Log("Escena del tablero cargada. Configurando botones...");
            SetupButtons(); // Configurar los botones dinámicamente
        }
    }

    private void SetupButtons()
    {
        // Buscar botones en la escena actual
        buttonLoadDinoShowcase = GameObject.Find("ButtonLoadDinoShowcase")?.GetComponent<Button>();
        buttonLoadBoardScene = GameObject.Find("ButtonLoadBoardScene")?.GetComponent<Button>();

        if (buttonLoadDinoShowcase != null)
        {
            buttonLoadDinoShowcase.onClick.RemoveAllListeners();
            buttonLoadDinoShowcase.onClick.AddListener(LoadDinoShowcaseScene);
            Debug.Log("Botón DinoShowcase configurado correctamente.");
        }
        else
        {
            Debug.LogWarning("Botón DinoShowcase no encontrado en la escena del tablero.");
        }

        if (buttonLoadBoardScene != null)
        {
            buttonLoadBoardScene.onClick.RemoveAllListeners();
            buttonLoadBoardScene.onClick.AddListener(LoadBoardScene);
            Debug.Log("Botón Tablero configurado correctamente.");
        }
        else
        {
            Debug.LogWarning("Botón Tablero no encontrado en la escena del tablero.");
        }
    }

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