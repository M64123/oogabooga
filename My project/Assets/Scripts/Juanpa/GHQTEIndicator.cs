using UnityEngine;

public class GHQTEIndicator : MonoBehaviour
{
    // Datos del QTE. Se asignan en Initialize.
    public GHQTEData data;

    private GHQTEManager manager;
    private float leadTime;
    private float perfectMargin;
    private float perfectTime;
    private bool checkedInput = false;

    private RectTransform rect;
    private Vector2 startPos;
    private Vector2 endPos;

    /// <summary>
    /// Llamado justo después de instanciar el prefab.
    /// Recibe GHQTEData y parámetros para el movimiento y la detección.
    /// </summary>
    public void Initialize(GHQTEData qteData, GHQTEManager mgr, float lead, float margin)
    {
        if (qteData == null)
        {
            Debug.LogError("[GHQTEIndicator] Initialize => qteData es null. Destruyendo QTE.");
            Destroy(gameObject);
            return;
        }

        data = qteData;
        manager = mgr;
        leadTime = lead;
        perfectMargin = margin;
        perfectTime = data.perfectTime;

        rect = GetComponent<RectTransform>();
        if (rect && rect.parent)
        {
            float h = rect.parent.GetComponent<RectTransform>().rect.height;

            // Determinamos posición inicial y final (arriba -> abajo)
            startPos = new Vector2(0f, h / 2f + 50f);
            endPos = new Vector2(0f, -h / 2f - 50f);

            rect.anchoredPosition = startPos;
        }

        Debug.Log($"[GHQTEIndicator] Init => Lane={data.laneID}, note={data.note}, key={data.key}");
    }

    private void Update()
    {
        if (data == null) return; // defensivo

        float dspNow = (float)AudioSettings.dspTime;
        float timeUntilPerfect = perfectTime - dspNow;

        // Calculamos t para el movimiento vertical
        float t = 1f - Mathf.Clamp01(timeUntilPerfect / leadTime);
        if (rect)
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        // Opción: detección interna de input
        if (!checkedInput && dspNow >= perfectTime - perfectMargin)
        {
            CheckInput(dspNow);
            checkedInput = true;
        }

        // Destruimos 1s después del perfectTime
        if (dspNow > perfectTime + 1f)
        {
            DestroyQTE();
        }
    }

    /// <summary>
    /// Chequea el input de forma interna, si no se usa Receptor.
    /// </summary>
    private void CheckInput(float dspNow)
    {
        // "data.key" => la tecla asignada al QTE.
        if (Input.GetKeyDown(data.key))
        {
            float diff = Mathf.Abs(dspNow - perfectTime);
            if (diff <= perfectMargin)
            {
                Debug.Log($"[GHQTEIndicator] PERFECT => Lane={data.laneID}, note={data.note}");
                data.qteType?.onSuccess?.Invoke();
                DestroyQTE();
            }
            else
            {
                Debug.Log($"[GHQTEIndicator] MISS => dspNow={dspNow:F2}, needed ~{perfectTime:F2}");
            }
        }
    }

    /// <summary>
    /// Llamado tanto interna como externamente (por un GHReceptor),
    /// para destruir la nota.
    /// </summary>
    public void DestroyQTE()
    {
        // manager?.RemoveIndicator(this); // Opcional si GHQTEManager lleva combos
        Destroy(gameObject);
    }
}
