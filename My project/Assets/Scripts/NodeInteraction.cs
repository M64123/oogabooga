// NodeInteraction.cs
using UnityEngine;

public class NodeInteraction : MonoBehaviour
{
    public MapNode node;

    // Materiales para cada tipo de nodo
    public Material inicioMaterial;
    public Material combateMaterial;
    public Material gamblingMaterial;
    public Material manaTamañoMaterial;
    public Material revivirMaterial;
    public Material randomMaterial;
    public Material coinFlipMaterial;
    public Material bossMaterial;

    private Renderer nodeRenderer;
    private Material originalMaterial; // Material original del nodo
    private Material instanceMaterial; // Instancia del material para modificaciones

    void Awake()
    {
        nodeRenderer = GetComponentInChildren<Renderer>();

        if (nodeRenderer == null)
        {
            Debug.LogError("nodeRenderer es null en NodeInteraction. Asegúrate de que el objeto tiene un componente Renderer.");
        }
    }

    // Este método será llamado después de asignar 'node' desde MapGenerator
    public void InitializeNodeVisual()
    {
        if (node == null)
        {
            Debug.LogError("El MapNode 'node' es null en NodeInteraction. Asegúrate de que se asigna correctamente en MapGenerator.");
            return;
        }

        SetNodeVisual(node.nodeType);
    }

    public void SetNodeVisual(NodeType nodeType)
    {
        switch (nodeType)
        {
            case NodeType.Inicio:
                originalMaterial = inicioMaterial;
                break;
            case NodeType.Combate:
                originalMaterial = combateMaterial;
                break;
            case NodeType.GAMBLING:
                originalMaterial = gamblingMaterial;
                break;
            case NodeType.Mana_Tamaño:
                originalMaterial = manaTamañoMaterial;
                break;
            case NodeType.Revivir:
                originalMaterial = revivirMaterial;
                break;
            case NodeType.Random:
                originalMaterial = randomMaterial;
                break;
            case NodeType.CoinFlip:
                originalMaterial = coinFlipMaterial;
                break;
            case NodeType.Boss:
                originalMaterial = bossMaterial;
                break;
            default:
                Debug.LogError($"Tipo de nodo no reconocido: {nodeType}");
                return;
        }

        if (originalMaterial == null)
        {
            Debug.LogError($"El material para el nodo {nodeType} no está asignado en el prefab del nodo. Por favor, asigna el material correspondiente en el Inspector.");
            return;
        }

        // Crear una instancia del material para modificarlo sin afectar a otros nodos
        instanceMaterial = new Material(originalMaterial);
        nodeRenderer.material = instanceMaterial;

        Debug.Log($"Nodo {node.nodeType} en posición {node.position} ha sido visualizado.");
    }

    // Método para marcar el nodo como visitado (reduce opacidad al 60%)
    public void SetVisited()
    {
        if (instanceMaterial == null)
        {
            Debug.LogError("instanceMaterial es null en SetVisited.");
            return;
        }

        Color color = instanceMaterial.color;
        color.a = 0.6f; // 60% de opacidad
        instanceMaterial.color = color;

        // Asegurarse de que la emisión esté desactivada
        instanceMaterial.DisableKeyword("_EMISSION");
        instanceMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        instanceMaterial.SetColor("_EmissionColor", Color.black);
    }

    // Método para resetear el material al estado original
    public void ResetColor()
    {
        if (instanceMaterial == null || originalMaterial == null)
        {
            Debug.LogError("instanceMaterial u originalMaterial es null en ResetColor.");
            return;
        }

        instanceMaterial.CopyPropertiesFromMaterial(originalMaterial);
    }

    // Método para marcar el nodo como disponible (se ilumina conservando el color)
    public void SetAvailable()
    {
        if (instanceMaterial == null)
        {
            Debug.LogError("instanceMaterial es null en SetAvailable.");
            return;
        }

        // Activar emisión
        instanceMaterial.EnableKeyword("_EMISSION");
        instanceMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

        // Ajustar el color de emisión al color base multiplicado para aumentar el brillo
        Color emissionColor = instanceMaterial.color * 2f; // Puedes ajustar el multiplicador
        instanceMaterial.SetColor("_EmissionColor", emissionColor);
    }

    // Método para marcar el nodo como actual
    public void SetCurrent()
    {
        // Por ejemplo, puedes aumentar un poco el tamaño del nodo
        transform.localScale = Vector3.one * 1.2f;
    }

    // Método para manejar el clic en el nodo
    void OnMouseDown()
    {
        if (node == null)
        {
            Debug.LogError("El nodo es null en NodeInteraction.");
            return;
        }

        Debug.Log($"Nodo {node.nodeType} en posición {node.position} ha sido clicado.");

        // Obtener el jugador y moverlo a este nodo
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.MoveToNode(node);
        }
        else
        {
            Debug.LogError("No se encontró el script PlayerMovement en la escena.");
        }
    }
}
