using UnityEngine;

public class NodeInteraction : MonoBehaviour
{
    public MapNode node;

    private Renderer nodeRenderer;
    private Color originalColor;

    // Materiales para cada tipo de nodo
    public Material battleMaterial;
    public Material shopMaterial;
    public Material upgradeMaterial;
    public Material restMaterial;
    public Material treasureMaterial;
    public Material bossMaterial;

    private void Start()
    {
        nodeRenderer = GetComponent<Renderer>();
        if (nodeRenderer != null)
        {
            originalColor = nodeRenderer.material.color;
        }
    }

    private void OnMouseDown()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.MoveToNode(node);
        }
    }

    public void SetVisited()
    {
        // Reducir la opacidad al 50%
        if (nodeRenderer != null)
        {
            Color color = nodeRenderer.material.color;
            color.a = 0.5f;
            nodeRenderer.material.color = color;
        }
    }

    public void SetAvailable()
    {
        // Iluminar el nodo en verde
        if (nodeRenderer != null)
        {
            nodeRenderer.material.color = Color.green;
        }
    }

    public void SetCurrent()
    {
        // Iluminar el nodo en azul
        if (nodeRenderer != null)
        {
            nodeRenderer.material.color = Color.blue;
        }
    }

    public void ResetColor()
    {
        // Restablecer el color original
        if (nodeRenderer != null)
        {
            nodeRenderer.material.color = originalColor;
        }
    }

    public void SetNodeVisual(NodeType type)
    {
        // Cambiar el material del nodo según su tipo
        if (nodeRenderer != null)
        {
            switch (type)
            {
                case NodeType.Battle:
                    nodeRenderer.material = battleMaterial;
                    break;
                case NodeType.Shop:
                    nodeRenderer.material = shopMaterial;
                    break;
                case NodeType.Upgrade:
                    nodeRenderer.material = upgradeMaterial;
                    break;
                case NodeType.Rest:
                    nodeRenderer.material = restMaterial;
                    break;
                case NodeType.Treasure:
                    nodeRenderer.material = treasureMaterial;
                    break;
                case NodeType.Boss:
                    nodeRenderer.material = bossMaterial;
                    break;
            }

            // Guardar el color original
            originalColor = nodeRenderer.material.color;
        }
    }
}
