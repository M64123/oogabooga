using UnityEngine;
using UnityEngine.UI;

public class TaikoQTEIndicator : MonoBehaviour
{
    private TaikoQTEData data;
    private TaikoQTEManager manager;
    private float spawnLeadTime;
    private float perfectMargin;

    private float perfectTime;
    private bool hasChecked = false;

    // Posiciones de inicio y fin (en coords locales del rect)
    private Vector2 startPos;
    private Vector2 endPos;

    // Referencia al RectTransform
    private RectTransform rectTransform;

    public void Initialize(TaikoQTEData qte, TaikoQTEManager mgr, float leadTime, float margin)
    {
        data = qte;
        manager = mgr;
        spawnLeadTime = leadTime;
        perfectMargin = margin;
        perfectTime = qte.perfectTime;

        Debug.Log($"[TaikoQTEIndicator] Init => QTEType={data.qteType}, perfectTime={perfectTime:F2}");

        rectTransform = GetComponent<RectTransform>();

        // Calcular pos. de inicio y fin con base en la anchura de la barra contenedora
        // asumiendo que 'transform.parent' es la barra -> qteBarTransform
        var barRect = rectTransform.parent.GetComponent<RectTransform>();

        // Extremos (ejemplo):
        float halfWidth = barRect.rect.width / 2f;

        // La barra en su espacio local: extremo izq = -halfWidth, der = +halfWidth
        // Si tu 'Anchor' central está en (0,0), etc.
        startPos = new Vector2(+halfWidth, 0f); // extremo derecho
        endPos = new Vector2(-halfWidth, 0f); // extremo izquierdo

        // Asignar la posición inicial
        rectTransform.anchoredPosition = startPos;
    }

    private void Update()
    {
        float dspNow = (float)AudioSettings.dspTime;

        // Calculamos cuántos seg faltan para la nota perfecta
        float timeUntilPerfect = perfectTime - dspNow;

        // 'timeUntilPerfect' va desde 'spawnLeadTime' (cuando se spawnea) hasta 0 en el perfectTime
        // ratio = 1 en perfectTime => total: 1 - (timeUntilPerfect / spawnLeadTime)
        float t = 1f - Mathf.Clamp01(timeUntilPerfect / spawnLeadTime);

        // Interpolación lineal
        rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        // Chequear input en la ventana de 'perfectMargin'
        if (!hasChecked && dspNow >= perfectTime - perfectMargin)
        {
            CheckInput(dspNow);
            hasChecked = true;
        }

        // Autodestrucción un poco después del perfectTime
        if (dspNow > perfectTime + 1f)
        {
            RemoveThis();
        }
    }

    private void CheckInput(float dspNow)
    {
        bool success = false;

        if (data.qteType == TaikoQTEType.Single)
        {
            if (data.requiredKeys != null && data.requiredKeys.Length > 0)
            {
                KeyCode k = data.requiredKeys[0];
                if (Input.GetKeyDown(k))
                {
                    float diff = Mathf.Abs(dspNow - perfectTime);
                    if (diff <= perfectMargin)
                        success = true;
                }
            }
        }
        else if (data.qteType == TaikoQTEType.Dual)
        {
            if (data.requiredKeys != null && data.requiredKeys.Length >= 2)
            {
                KeyCode k1 = data.requiredKeys[0];
                KeyCode k2 = data.requiredKeys[1];
                if ((Input.GetKeyDown(k1) && Input.GetKey(k2)) ||
                    (Input.GetKeyDown(k2) && Input.GetKey(k1)))
                {
                    float diff = Mathf.Abs(dspNow - perfectTime);
                    if (diff <= perfectMargin)
                        success = true;
                }
            }
        }

        if (success)
        {
            Debug.Log($"[TaikoQTEIndicator] PERFECT => dspNow={dspNow:F2}, needed ~{perfectTime:F2}");
            RemoveThis();
        }
        else
        {
            Debug.Log($"[TaikoQTEIndicator] No perfect => dspNow={dspNow:F2}, se necesitaba ~{perfectTime:F2}");
        }
    }

    private void RemoveThis()
    {
        manager.RemoveIndicator(this);
        Destroy(gameObject);
    }
}
