using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GHReceptorMultiLane:
/// - Funciona en la UI (HUD) con un objeto que tenga un "RectTransform" y un "BoxCollider2D" (isTrigger = true).
/// - Maneja varias lanes a la vez. Se guarda los GHQTEIndicator que entran en su trigger y, si el jugador pulsa la tecla
///   correspondiente dentro del margen de tiempo, se logra un PERFECT.
/// - Asegúrate de que tus GHQTEIndicator tengan un "Rigidbody2D" (gravityScale=0) o se muevan apropiadamente en la UI.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GHReceptorMultiLane : MonoBehaviour
{
    [Header("Lane IDs que controla este receptor (p.e. [0,1])")]
    public List<int> laneIDs = new List<int>();

    /// <summary>
    /// QTEIndicators actualmente dentro del trigger.
    /// </summary>
    private List<GHQTEIndicator> indicatorsInside = new List<GHQTEIndicator>();

    private void OnTriggerEnter2D(Collider2D col)
    {
        GHQTEIndicator qteInd = col.GetComponent<GHQTEIndicator>();
        if (qteInd != null && laneIDs.Contains(qteInd.data.laneID))
        {
            // Si no está ya en la lista, se añade.
            if (!indicatorsInside.Contains(qteInd))
            {
                indicatorsInside.Add(qteInd);
                Debug.Log($"[GHReceptorMultiLane] Indicador Lane={qteInd.data.laneID} " +
                          $"ha ENTRADO en el receptor. Lanes controlados={ListToString(laneIDs)}");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        GHQTEIndicator qteInd = col.GetComponent<GHQTEIndicator>();
        if (qteInd != null && indicatorsInside.Contains(qteInd))
        {
            indicatorsInside.Remove(qteInd);
            Debug.Log($"[GHReceptorMultiLane] Indicador Lane={qteInd.data.laneID} ha SALIDO del receptor.");
        }
    }

    private void Update()
    {
        if (indicatorsInside.Count == 0) return;

        // Si detectamos una pulsación de tecla en este frame...
        if (Input.anyKeyDown)
        {
            // Recorremos los QTEIndicator dentro para ver si alguno coincide con la tecla
            foreach (GHQTEIndicator qteInd in indicatorsInside.ToArray())
            {
                if (Input.GetKeyDown(qteInd.data.key))
                {
                    float dspNow = (float)AudioSettings.dspTime;
                    float margin = GHQTEManager.Instance ? GHQTEManager.Instance.perfectMargin : 0.1f;
                    float diff = Mathf.Abs(dspNow - qteInd.data.perfectTime);

                    if (diff <= margin)
                    {
                        // Perfect
                        Debug.Log($"[GHReceptorMultiLane] PERFECT => Lane={qteInd.data.laneID}, diff={diff:F2}");
                        qteInd.DestroyQTE();
                        indicatorsInside.Remove(qteInd);
                    }
                    else
                    {
                        Debug.Log($"[GHReceptorMultiLane] MISS => Lane={qteInd.data.laneID}, diff={diff:F2} fuera de {margin:F2}");
                    }
                    // Si deseas solo un QTE por tecla, podrías poner "break;" aquí
                }
            }
        }
    }

    private string ListToString(List<int> list)
    {
        if (list == null || list.Count == 0) return "Sin lanes";
        return string.Join(", ", list);
    }
}
