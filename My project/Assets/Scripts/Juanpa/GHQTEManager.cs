using UnityEngine;
using System.Collections.Generic;

public class GHQTEManager : MonoBehaviour
{
    [Header("Lane Rects (index = laneID)")]
    public RectTransform[] laneRects;

    [Header("Timing")]
    public float spawnLeadTime = 2f;
    public float perfectMargin = 0.1f;

    private bool managerActive = false;
    private List<GHQTEData> upcomingQTEs = new List<GHQTEData>();
    private List<GHQTEIndicator> activeIndicators = new List<GHQTEIndicator>();

    private void Update()
    {
        if (!managerActive) return;

        float dspNow = (float)AudioSettings.dspTime;

        // Spawnear
        for (int i = 0; i < upcomingQTEs.Count; i++)
        {
            var qte = upcomingQTEs[i];
            float spawnTime = qte.perfectTime - spawnLeadTime;
            if (dspNow >= spawnTime)
            {
                SpawnIndicator(qte);
                upcomingQTEs.RemoveAt(i);
                i--;
            }
        }
    }

    public void StartQTEManager()
    {
        managerActive = true;
        upcomingQTEs.Clear();

        // limpiar QTEIndicators activos
        foreach (var ind in activeIndicators)
        {
            if (ind != null) Destroy(ind.gameObject);
        }
        activeIndicators.Clear();

        Debug.Log("[GHQTEManager] StartQTEManager => Limpieza e inicia.");
    }

    public void StopQTEManager()
    {
        managerActive = false;
        upcomingQTEs.Clear();
        foreach (var ind in activeIndicators)
        {
            if (ind != null) Destroy(ind.gameObject);
        }
        activeIndicators.Clear();

        Debug.Log("[GHQTEManager] StopQTEManager => limpiado");
    }

    public void AddQTEs(List<GHQTEData> newQTEs)
    {
        upcomingQTEs.AddRange(newQTEs);
        upcomingQTEs.Sort((a, b) => a.perfectTime.CompareTo(b.perfectTime));
        Debug.Log($"[GHQTEManager] Agregados {newQTEs.Count} QTEs a la cola.");
    }

    private void SpawnIndicator(GHQTEData qte)
    {
        if (qte.laneID < 0 || qte.laneID >= laneRects.Length)
        {
            Debug.LogWarning($"[GHQTEManager] LaneID={qte.laneID} fuera de rango");
            return;
        }
        RectTransform laneRect = laneRects[qte.laneID];
        if (!laneRect)
        {
            Debug.LogWarning($"[GHQTEManager] laneRects[{qte.laneID}] es null");
            return;
        }

        if (qte.prefab == null)
        {
            Debug.LogWarning("[GHQTEManager] QTE prefab nulo => no spawneamos");
            return;
        }

        GameObject go = Instantiate(qte.prefab, laneRect);
        GHQTEIndicator indicator = go.GetComponent<GHQTEIndicator>();
        if (indicator == null)
        {
            Debug.LogWarning("[GHQTEManager] Prefab sin GHQTEIndicator => destruyendo");
            Destroy(go);
            return;
        }

        indicator.Initialize(qte, this, spawnLeadTime, perfectMargin);
        activeIndicators.Add(indicator);

        Debug.Log($"[GHQTEManager] Spawn => lane={qte.laneID}, dspNow={AudioSettings.dspTime:F2}, perfTime={qte.perfectTime:F2}");
    }

    public void RemoveIndicator(GHQTEIndicator ind)
    {
        activeIndicators.Remove(ind);
    }
}
