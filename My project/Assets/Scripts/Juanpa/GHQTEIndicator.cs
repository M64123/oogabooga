using UnityEngine;
using UnityEngine.UI;

public class GHQTEIndicator : MonoBehaviour
{
    private GHQTEData data;
    private GHQTEManager manager;
    private float spawnLeadTime;
    private float perfectMargin;

    private float perfectTime;
    private bool hasChecked = false;

    private RectTransform rectTransform;
    private Vector2 startPos;
    private Vector2 endPos;

    public void Initialize(GHQTEData qte, GHQTEManager mgr, float lead, float margin)
    {
        data = qte;
        manager = mgr;
        spawnLeadTime = lead;
        perfectMargin = margin;
        perfectTime = qte.perfectTime;

        Debug.Log($"[GHQTEIndicator] Init => lane={data.laneID}, key={data.key}, perfectTime={perfectTime:F2}");

        rectTransform = GetComponent<RectTransform>();

        // Suponemos la laneRect ancla en el centro (Pivot(0.5,0.5)).
        // Queremos mover la nota de top a bottom en la vertical. 
        // LaneRect height => rectTransform.parent.GetComponent<RectTransform>().rect.height
        float laneHeight = rectTransform.parent.GetComponent<RectTransform>().rect.height;

        // startPos: arriba => y = +laneHeight/2
        // endPos:   abajo => y = -laneHeight/2
        // x = 0 (centrado en la lane)
        startPos = new Vector2(0f, laneHeight / 2f + 50f); // un poco fuera de la lane
        endPos = new Vector2(0f, -laneHeight / 2f - 50f);

        rectTransform.anchoredPosition = startPos;
    }

    private void Update()
    {
        float dspNow = (float)AudioSettings.dspTime;

        // tiempo faltante hasta el perfect
        float timeUntilPerfect = perfectTime - dspNow;
        // 0 => recien spawn, spawnLeadTime => ...
        // ratio=1 en dspNow >= perfectTime
        float t = 1f - Mathf.Clamp01(timeUntilPerfect / spawnLeadTime);

        // mover vertical
        rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        // check input
        if (!hasChecked && dspNow >= perfectTime - perfectMargin)
        {
            CheckInput(dspNow);
            hasChecked = true;
        }

        // autodestrucción
        if (dspNow > perfectTime + 1f)
        {
            RemoveThis();
        }
    }

    private void CheckInput(float dspNow)
    {
        bool success = false;

        // si la lane usa 1 KeyCode, chequeamos .GetKeyDown
        if (Input.GetKeyDown(data.key))
        {
            float diff = Mathf.Abs(dspNow - perfectTime);
            if (diff <= perfectMargin)
            {
                success = true;
            }
        }

        if (success)
        {
            Debug.Log($"[GHQTEIndicator] PERFECT => dspNow={dspNow:F2}, needed ~{perfectTime:F2}, lane={data.laneID}");
            RemoveThis();
        }
        else
        {
            Debug.Log($"[GHQTEIndicator] No perfect => dspNow={dspNow:F2}, se necesitaba ~{perfectTime:F2}");
        }
    }

    private void RemoveThis()
    {
        manager.RemoveIndicator(this);
        Destroy(gameObject);
    }
}
