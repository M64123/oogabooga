using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Manager principal de QTE estilo Taiko:
/// - Mantiene una lista de TaikoQTEData con (tipo, perfectTime, teclas),
/// - Spawnea un TaikoQTEIndicator 2s antes de cada perfectTime,
/// - Detecta la pulsación en el momento perfecto.
/// 
/// Integrable con un AudioController: 
/// cuando arranque Combate, cargar QTEs de Combate; 
/// cuando arranque Defensa, cargar QTEs de Defensa, etc.
/// </summary>
public class TaikoQTEManager : MonoBehaviour
{
    [Header("QTE Prefabs")]
    [Tooltip("Prefab para QTE tipo Single (una sola tecla).")]
    public GameObject qteSinglePrefab;

    [Tooltip("Prefab para QTE tipo Dual (dos teclas).")]
    public GameObject qteDualPrefab;

    [Header("Barra de QTE")]
    [Tooltip("Donde se instancian los QTEIndicators (UI).")]
    public RectTransform qteBarTransform;

    [Header("Timing y Margen")]
    [Tooltip("Segundos antes del perfectTime en que aparece la nota en pantalla.")]
    public float spawnLeadTime = 2f;

    [Tooltip("Margen de error para un golpe Perfect.")]
    public float perfectMargin = 0.1f;

    private List<TaikoQTEData> upcomingQTEs = new List<TaikoQTEData>();
    private List<TaikoQTEIndicator> activeIndicators = new List<TaikoQTEIndicator>();

    private bool managerActive = false;

    private void Update()
    {
        if (!managerActive) return;

        float dspNow = (float)AudioSettings.dspTime;

        // Spawnear QTEs cuando se alcance su spawnTime
        for (int i = 0; i < upcomingQTEs.Count; i++)
        {
            TaikoQTEData qte = upcomingQTEs[i];
            float spawnTime = qte.perfectTime - spawnLeadTime;
            if (dspNow >= spawnTime)
            {
                // Instanciamos un indicador
                SpawnIndicator(qte);
                upcomingQTEs.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Inicializa el QTEManager: borra QTEs pendientes y limpia los existentes.
    /// Llamar cuando arranque la música / sección (Combate/Defensa).
    /// </summary>
    public void StartQTEManager()
    {
        managerActive = true;
        upcomingQTEs.Clear();

        foreach (var ind in activeIndicators)
        {
            if (ind != null) Destroy(ind.gameObject);
        }
        activeIndicators.Clear();

        Debug.Log("[TaikoQTEManager] StartQTEManager => Limpieza e inicia.");
    }

    /// <summary>
    /// Detiene el QTEManager y destruye los QTEs activos.
    /// </summary>
    public void StopQTEManager()
    {
        managerActive = false;
        upcomingQTEs.Clear();

        foreach (var ind in activeIndicators)
        {
            if (ind != null) Destroy(ind.gameObject);
        }
        activeIndicators.Clear();

        Debug.Log("[TaikoQTEManager] StopQTEManager => Limpieza total.");
    }

    /// <summary>
    /// Agregar QTEData (ej: parseado de MIDI) a la cola.
    /// Ordena por tiempo para spawnear en orden.
    /// </summary>
    public void AddQTEs(List<TaikoQTEData> newQTEs)
    {
        upcomingQTEs.AddRange(newQTEs);
        upcomingQTEs.Sort((a, b) => a.perfectTime.CompareTo(b.perfectTime));
        Debug.Log($"[TaikoQTEManager] Agregados {newQTEs.Count} QTEs a la cola.");
    }

    private void SpawnIndicator(TaikoQTEData qte)
    {
        // Elegir prefab
        GameObject prefab = (qte.qteType == TaikoQTEType.Single) ? qteSinglePrefab : qteDualPrefab;
        if (prefab == null)
        {
            Debug.LogWarning($"[TaikoQTEManager] Prefab no asignado para {qte.qteType}");
            return;
        }

        // Instanciar
        GameObject go = Instantiate(prefab, qteBarTransform);
        TaikoQTEIndicator indicator = go.GetComponent<TaikoQTEIndicator>();
        if (indicator == null)
        {
            Debug.LogWarning("[TaikoQTEManager] Prefab sin TaikoQTEIndicator");
            return;
        }

        // Configurar
        indicator.Initialize(qte, this, spawnLeadTime, perfectMargin);
        activeIndicators.Add(indicator);

        Debug.Log($"[TaikoQTEManager] Spawn QTE => type={qte.qteType}, perfectTime={qte.perfectTime:F2}, dspNow={(float)AudioSettings.dspTime:F2}");
    }

    /// <summary>
    /// Llamado por TaikoQTEIndicator cuando termina (Destroy).
    /// </summary>
    public void RemoveIndicator(TaikoQTEIndicator ind)
    {
        activeIndicators.Remove(ind);
    }
}
