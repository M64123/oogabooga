using UnityEngine;

public class GHQTEIndicator : MonoBehaviour
{
    private GHQTEData data;
    private GHQTEManager manager;
    private float spawnLeadTime;
    private float perfectMargin;
    private float perfectTime;

    private RectTransform rect;
    private Vector2 startPos;
    private Vector2 endPos;
    private bool hasChecked = false;

    public void Initialize(GHQTEData qte, GHQTEManager mgr, float lead, float margin)
    {
        data = qte;
        manager = mgr;
        spawnLeadTime = lead;
        perfectMargin = margin;
        perfectTime = qte.perfectTime;

        rect = GetComponent<RectTransform>();
        float laneHeight = rect.parent.GetComponent<RectTransform>().rect.height;

        // Suponiendo pivote(0.5, 0.5), definimos la parte superior e inferior
        startPos = new Vector2(0f, laneHeight / 2f + 50f);
        endPos = new Vector2(0f, -laneHeight / 2f - 50f);

        rect.anchoredPosition = startPos;

        Debug.Log($"[GHQTEIndicator] Init => lane={data.laneID}, note={data.note}, key={data.key}, perfectTime={perfectTime:F2}");
    }

    private void Update()
    {
        float dspNow = (float)AudioSettings.dspTime;
        float timeUntilPerfect = perfectTime - dspNow;
        float t = 1f - Mathf.Clamp01(timeUntilPerfect / spawnLeadTime);

        rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

        // check input ~ perfectTime
        if (!hasChecked && dspNow >= perfectTime - perfectMargin)
        {
            CheckInput(dspNow);
            hasChecked = true;
        }

        // si dspNow > perfectTime + 1 => se autodestruye
        if (dspNow > perfectTime + 1f)
        {
            RemoveThis();
        }
    }

    private void CheckInput(float dspNow)
    {
        bool success = false;
        if (Input.GetKeyDown(data.key))
        {
            float diff = Mathf.Abs(dspNow - perfectTime);
            if (diff <= perfectMargin)
                success = true;
        }

        if (success)
        {
            Debug.Log($"[GHQTEIndicator] PERFECT => dspNow={dspNow:F2}, lane={data.laneID}, note={data.note}");
            RemoveThis();
        }
        else
        {
            Debug.Log($"[GHQTEIndicator] No perfect => dspNow={dspNow:F2}, needed ~{perfectTime:F2}");
        }
    }

    private void RemoveThis()
    {
        manager.RemoveIndicator(this);
        Destroy(gameObject);
    }
}
