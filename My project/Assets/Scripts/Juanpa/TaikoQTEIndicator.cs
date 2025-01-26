using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente que representa una nota QTE en la barra:
/// - Se mueve durante 'spawnLeadTime' hasta el momento perfectTime.
/// - Detecta pulsaciones (Single o Dual),
/// - Destruye si se acierta o se acaba el tiempo.
/// </summary>
public class TaikoQTEIndicator : MonoBehaviour
{
    private TaikoQTEData data;
    private TaikoQTEManager manager;
    private float spawnLeadTime;
    private float perfectMargin;

    private float perfectTime;
    private bool hasChecked = false;

    public void Initialize(TaikoQTEData qte, TaikoQTEManager mgr, float leadTime, float margin)
    {
        data = qte;
        manager = mgr;
        spawnLeadTime = leadTime;
        perfectMargin = margin;

        perfectTime = qte.perfectTime;

        Debug.Log($"[TaikoQTEIndicator] Init => QTEType={data.qteType}, perfectTime={perfectTime:F2}");
        // Podrías ajustar la posición inicial, sprite, etc. aquí.
    }

    private void Update()
    {
        float dspNow = (float)AudioSettings.dspTime;

        // Mover/animar la nota según dspNow vs (perfectTime - spawnLeadTime).
        // Ejemplo: 
        // float totalTravelTime = spawnLeadTime;
        // float elapsed = perfectTime - dspNow;
        // etc. 
        // (Implementa tu animación UI a gusto.)

        // Chequeo de input si dspNow >= (perfectTime - perfectMargin) 
        //   o dspNow <= (perfectTime + perfectMargin), etc.

        if (!hasChecked && dspNow >= perfectTime - perfectMargin)
        {
            CheckInput(dspNow);
            hasChecked = true;
        }

        // Autodestrucción si dspNow > perfectTime + 1s
        if (dspNow > perfectTime + 1f)
        {
            RemoveThis();
        }
    }

    private void CheckInput(float dspNow)
    {
        bool success = false;

        // Single
        if (data.qteType == TaikoQTEType.Single)
        {
            if (data.requiredKeys != null && data.requiredKeys.Length > 0)
            {
                KeyCode singleKey = data.requiredKeys[0];
                if (Input.GetKeyDown(singleKey))
                {
                    float diff = Mathf.Abs(dspNow - perfectTime);
                    if (diff <= perfectMargin)
                    {
                        success = true;
                    }
                }
            }
        }
        else if (data.qteType == TaikoQTEType.Dual)
        {
            // 2 teclas
            if (data.requiredKeys != null && data.requiredKeys.Length >= 2)
            {
                KeyCode k1 = data.requiredKeys[0];
                KeyCode k2 = data.requiredKeys[1];
                // Se considera "perfect" si se pulsa k1 y k2 (uno con KeyDown, el otro con Key?), etc.
                if ((Input.GetKeyDown(k1) && Input.GetKey(k2)) ||
                    (Input.GetKeyDown(k2) && Input.GetKey(k1)))
                {
                    float diff = Mathf.Abs(dspNow - perfectTime);
                    if (diff <= perfectMargin)
                    {
                        success = true;
                    }
                }
            }
        }

        if (success)
        {
            Debug.Log($"[TaikoQTEIndicator] PERFECT => dspNow={dspNow:F2}, perfectTime={perfectTime:F2}, diff={Mathf.Abs(dspNow - perfectTime):F3}");
            // Podrías dar puntos o invocar otro evento
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
