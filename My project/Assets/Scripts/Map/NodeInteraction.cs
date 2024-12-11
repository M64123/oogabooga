using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class NodeInteraction : MonoBehaviour
{
    public MapNode node;

    private Renderer nodeRenderer;
    private Material originalMaterial;
    private Material instanceMaterial;

    void Awake()
    {
        nodeRenderer = GetComponentInChildren<Renderer>();

        if (nodeRenderer == null)
        {
            Debug.LogError("NodeInteraction: nodeRenderer es null. Asegúrate de que el objeto tiene un componente Renderer.");
        }
        else
        {
            originalMaterial = nodeRenderer.material;
            instanceMaterial = new Material(originalMaterial);
            nodeRenderer.material = instanceMaterial;
        }
    }

    public void InitializeNodeVisual()
    {
        if (node == null)
        {
            Debug.LogError("NodeInteraction: El MapNode 'node' es null. Asegúrate de que se asigna correctamente en MapGenerator.");
            return;
        }
    }

    public void SetVisited()
    {
        if (instanceMaterial == null) return;

        Color color = instanceMaterial.color;
        color *= 0.6f;
        instanceMaterial.color = color;
    }

    public void ResetColor()
    {
        if (instanceMaterial == null || originalMaterial == null) return;

        instanceMaterial.CopyPropertiesFromMaterial(originalMaterial);
    }

    public void SetAvailable()
    {
        if (instanceMaterial == null) return;

        if (instanceMaterial.HasProperty("_EmissionColor"))
        {
            instanceMaterial.EnableKeyword("_EMISSION");
            instanceMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

            Color emissionColor = instanceMaterial.color * 2f;
            instanceMaterial.SetColor("_EmissionColor", emissionColor);
        }
        else
        {
            Color color = instanceMaterial.color;
            color *= 1.2f;
            instanceMaterial.color = color;
        }
    }

    public void SetCurrent()
    {
        transform.localScale = Vector3.one * 1.2f;
    }

    void OnMouseDown()
    {
        if (node == null) return;

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.OnMovementFinished -= OnPlayerMovementFinished;
            playerMovement.OnMovementFinished += OnPlayerMovementFinished;

            playerMovement.MoveToNode(node);
        }
    }

    private void OnPlayerMovementFinished()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.OnMovementFinished -= OnPlayerMovementFinished;
        }

        StartCoroutine(LoadSceneAfterDelay(0.1f));
    }

    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (NodeSceneManager.Instance != null)
        {
            string sceneName = NodeSceneManager.Instance.GetRandomSceneName(node.nodeType);
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}
