using UnityEngine;

public class GHQTEIndicator : MonoBehaviour
{
    // Hacemos 'data' público para leerlo en GHReceptor
    public GHQTEData data;

    private GHQTEManager manager;
    private float leadTime;
    private float perfectMargin;
    private float perfectTime;
    private bool checkedInput = false;

    private RectTransform rect;
    private Vector2 startPos;
    private Vector2 endPos;

    public void Initialize(GHQTEData qteData, GHQTEManager mgr, float lead, float margin)
    {
        data = qteData;  // Asignamos
        manager = mgr;
        leadTime = lead;
        perfectMargin = margin;
        perfectTime = qteData.perfectTime;

        rect = GetComponent<RectTransform>();
        if (rect && rect.parent)
        {
            float h = rect.parent.GetComponent<RectTransform>().rect.height;
            startPos = new Vector2(0f, h / 2f + 50f);
            endPos = new Vector2(0f, -h / 2f - 50f);
            rect.anchoredPosition = startPos;
        }

        Debug.Log($"[GHQTEIndicator] Init => Lane={data.laneID}, note={data.note}, key={data.key}");
    }

    private void Update()
    {
        float dspNow = (float)AudioSettings.dspTime;
        float timeUntilPerfect = perfectTime - dspNow;

        float t = 1f - Mathf.Clamp01(timeUntilPerfect / leadTime);
        if (rect)
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        // Check input si no usamos Receptor, etc. (puedes comentar esta parte si prefieres el Receptor)
        if (!checkedInput && dspNow >= perfectTime - perfectMargin)
        {
            CheckInput(dspNow);
            checkedInput = true;
        }

        // Borrar 1s luego
        if (dspNow > perfectTime + 1f)
        {
            DestroyQTE();
        }
    }

    // Si sigues queriendo la detección "por tiempo" aquí, la dejas
    // Si no, se usará la de GHReceptor.
    private void CheckInput(float dspNow)
    {
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
                Debug.Log($"[GHQTEIndicator] Fallo => dspNow={dspNow:F2}, needed ~{perfectTime:F2}");
            }
        }
    }

    // Llamado por GHReceptor
    public void DestroyQTE()
    {
        // manager?.RemoveIndicator(this); // opcional
        Destroy(gameObject);
    }
}
