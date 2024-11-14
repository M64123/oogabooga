using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using System.Collections;

public class NodeInteraction : MonoBehaviour
{
    public MapNode node;

    private Renderer nodeRenderer;
    private Material originalMaterial; // Material original del nodo
    private Material instanceMaterial; // Instancia del material para modificaciones

    void Awake()
    {
        nodeRenderer = GetComponentInChildren<Renderer>();

        if (nodeRenderer == null)
        {
            Debug.LogError("NodeInteraction: nodeRenderer es null. Aseg�rate de que el objeto tiene un componente Renderer.");
        }
        else
        {
            // Crear una instancia del material para modificarlo sin afectar a otros nodos
            originalMaterial = nodeRenderer.material;
            instanceMaterial = new Material(originalMaterial);
            nodeRenderer.material = instanceMaterial;
        }
    }

    // Este m�todo ser� llamado despu�s de asignar 'node' desde MapGenerator
    public void InitializeNodeVisual()
    {
        if (node == null)
        {
            Debug.LogError("NodeInteraction: El MapNode 'node' es null. Aseg�rate de que se asigna correctamente en MapGenerator.");
            return;
        }

        // No es necesario asignar materiales aqu� ya que cada nodo utiliza su propio prefab con su material correspondiente
    }

    // M�todo para marcar el nodo como visitado (p. ej., oscurecerlo)
    public void SetVisited()
    {
        if (instanceMaterial == null)
        {
            Debug.LogError("NodeInteraction: instanceMaterial es null en SetVisited.");
            return;
        }

        // Ajustar el color para indicar que ha sido visitado (reducir el brillo)
        Color color = instanceMaterial.color;
        color *= 0.6f; // Reducir el brillo en un 40%
        instanceMaterial.color = color;
    }

    // M�todo para resetear el material al estado original
    public void ResetColor()
    {
        if (instanceMaterial == null || originalMaterial == null)
        {
            Debug.LogError("NodeInteraction: instanceMaterial u originalMaterial es null en ResetColor.");
            return;
        }

        instanceMaterial.CopyPropertiesFromMaterial(originalMaterial);
    }

    // M�todo para marcar el nodo como disponible (p. ej., aumentar el brillo o a�adir emisi�n)
    public void SetAvailable()
    {
        if (instanceMaterial == null)
        {
            Debug.LogError("NodeInteraction: instanceMaterial es null en SetAvailable.");
            return;
        }

        // Verificar si el material soporta emisi�n
        if (instanceMaterial.HasProperty("_EmissionColor"))
        {
            // Activar emisi�n
            instanceMaterial.EnableKeyword("_EMISSION");
            instanceMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            // Ajustar el color de emisi�n para aumentar el brillo
            Color emissionColor = instanceMaterial.color * 2f; // Puedes ajustar el multiplicador
            instanceMaterial.SetColor("_EmissionColor", emissionColor);
        }
        else
        {
            // Si el material no soporta emisi�n, aumentar el brillo del color base
            Color color = instanceMaterial.color;
            color *= 1.2f; // Aumentar el brillo en un 20%
            instanceMaterial.color = color;
        }
    }

    // M�todo para marcar el nodo como actual (p. ej., aumentar su tama�o)
    public void SetCurrent()
    {
        // Aumentar ligeramente el tama�o del nodo
        transform.localScale = Vector3.one * 1.2f;
    }

    // M�todo para manejar el clic en el nodo
    void OnMouseDown()
    {
        if (node == null)
        {
            Debug.LogError("NodeInteraction: El nodo es null en OnMouseDown.");
            return;
        }

        // Obtener el jugador y moverlo a este nodo
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            // Suscribirse al evento OnMovementFinished
            playerMovement.OnMovementFinished += OnPlayerMovementFinished;

            playerMovement.MoveToNode(node);
        }
        else
        {
            Debug.LogError("NodeInteraction: No se encontr� el script PlayerMovement en la escena.");
        }
    }

    private void OnPlayerMovementFinished()
    {
        // Desuscribirse del evento para evitar m�ltiples llamadas
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.OnMovementFinished -= OnPlayerMovementFinished;
        }

        // Iniciar la carga de la escena despu�s de una peque�a espera
        StartCoroutine(LoadSceneAfterDelay(0.1f));
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Cargar la escena correspondiente al tipo de nodo
        if (NodeSceneManager.Instance != null)
        {
            string sceneName = NodeSceneManager.Instance.GetRandomSceneName(node.nodeType);
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning($"No se pudo obtener una escena para el tipo de nodo {node.nodeType}.");
            }
        }
        else
        {
            Debug.LogError("NodeInteraction: No se encontr� el NodeSceneManager en la escena.");
        }
    }
}
