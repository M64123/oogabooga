using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GHQTEManager : MonoBehaviour
{
    [Header("Lane Rects (en orden de laneID)")]
    public RectTransform[] laneRects; // laneRects[0] => lane 0, laneRects[1] => lane 1, etc.

    [Header("Prefabs")]
    public GameObject qteIndicatorPrefab; // un prefab con GHQTEIndicator

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

        // spawnear QTE
        for (int i = 0; i < upcomingQTEs.Count; i++)
        {
            GHQTEData qte = upcomingQTEs[i];
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

        // Limpiar QTEIndicators activos
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

        Debug.Log("[GHQTEManager] StopQTEManager => todo limpiado.");
    }

    public void AddQTEs(List<GHQTEData> newQTEs)
    {
        upcomingQTEs.AddRange(newQTEs);
        upcomingQTEs.Sort((a, b) => a.perfectTime.CompareTo(b.perfectTime));
        Debug.Log($"[GHQTEManager] Agregados {newQTEs.Count} QTEs a la cola.");
    }

    private void SpawnIndicator(GHQTEData qte)
    {
        // LaneRect
        if (qte.laneID < 0 || qte.laneID >= laneRects.Length)
        {
            Debug.LogWarning($"[GHQTEManager] QTE laneID={qte.laneID} fuera de rango => se ignora");
            return;
        }
        RectTransform laneRect = laneRects[qte.laneID];
        if (!laneRect)
        {
            Debug.LogWarning($"[GHQTEManager] laneRects[{qte.laneID}] es nulo => no spawneamos");
            return;
        }

        GameObject go = Instantiate(qteIndicatorPrefab, laneRect);
        GHQTEIndicator indicator = go.GetComponent<GHQTEIndicator>();
        if (indicator == null)
        {
            Debug.LogWarning("[GHQTEManager] Prefab sin GHQTEIndicator");
            Destroy(go);
            return;
        }

        indicator.Initialize(qte, this, spawnLeadTime, perfectMargin);
        activeIndicators.Add(indicator);

        Debug.Log($"[GHQTEManager] Spawn QTE => lane={qte.laneID}, perfectTime={qte.perfectTime:F2}, dspNow={(float)AudioSettings.dspTime:F2}");
    }

    public void RemoveIndicator(GHQTEIndicator ind)
    {
        activeIndicators.Remove(ind);
    }
}
