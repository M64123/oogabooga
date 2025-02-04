using UnityEngine;
using System.Collections.Generic;

public class GHQTEManager : MonoBehaviour
{
    public static GHQTEManager Instance { get; private set; }

    [Header("LeadTime")]
    public float leadTime = 2f;

    [Header("Margen Perfect")]
    public float perfectMargin = 0.1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Ejemplo de spawn QTE con GHQTEData
    /// </summary>
    public void SpawnQTE(GHQTEData qteData, RectTransform parentRect)
    {
        if (!qteData.qteType)
        {
            Debug.LogWarning("[GHQTEManager] qteData.qteType es null => no spawn");
            return;
        }
        if (!qteData.qteType.prefab)
        {
            Debug.LogWarning("[GHQTEManager] QTEType sin prefab => no spawn");
            return;
        }
        if (!parentRect)
        {
            Debug.LogWarning("[GHQTEManager] parentRect es null => no spawn");
            return;
        }

        // Instanciamos el prefab
        GameObject go = Instantiate(qteData.qteType.prefab, parentRect);
        GHQTEIndicator indicator = go.GetComponent<GHQTEIndicator>();
        if (!indicator)
        {
            Debug.LogWarning("[GHQTEManager] Prefab sin GHQTEIndicator => destruyendo");
            Destroy(go);
            return;
        }

        // Llamada con la firma: (GHQTEData, GHQTEManager, float, float)
        indicator.Initialize(qteData, this, leadTime, perfectMargin);
    }

    public void RemoveIndicator(GHQTEIndicator ind)
    {
        Debug.Log($"[GHQTEManager] RemoveIndicator => {ind.gameObject.name}");
    }
}
