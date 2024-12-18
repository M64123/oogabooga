using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEIndicator : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform referenceImage;
    private float okZoneWidth;
    private float goodZoneWidth;
    private float excellentZoneWidth;
    private bool isLeftSide;

    public void Setup(KeyCode key, float okWidth, float goodWidth, float excellentWidth, RectTransform spawnPoint, RectTransform target, bool leftSide)
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform no encontrado en el QTEIndicator.");
            return;
        }

        rectTransform.anchoredPosition = spawnPoint.anchoredPosition;
        isLeftSide = leftSide;
        referenceImage = target;
        okZoneWidth = okWidth;
        goodZoneWidth = goodWidth;
        excellentZoneWidth = excellentWidth;

        if (referenceImage == null)
        {
            Debug.LogError("referenceImage no está asignado en Setup.");
        }
    }

    void Update()
    {
        if (rectTransform == null || referenceImage == null)
        {
            return;
        }

        float moveSpeed = 300f * Time.deltaTime;

        // Movimiento hacia el centro
        if (isLeftSide)
        {
            rectTransform.anchoredPosition += new Vector2(moveSpeed, 0f);
        }
        else
        {
            rectTransform.anchoredPosition -= new Vector2(moveSpeed, 0f);
        }

        // Verificar si alcanzó la referencia
        if (HasReachedReference())
        {
            if (QTEManagerCombat.Instance != null)
            {
                QTEManagerCombat.Instance.OnQTEFail(this);
            }
        }
    }

    public bool HasReachedReference()
    {
        if (referenceImage == null)
        {
            Debug.LogError("referenceImage es null en HasReachedReference.");
            return false;
        }

        float distance = Mathf.Abs(rectTransform.anchoredPosition.x - referenceImage.anchoredPosition.x);
        return distance < .4f; // Un margen pequeño para verificar que alcanzó el centro
    }

    public bool IsInActiveZone()
    {
        if (referenceImage == null)
        {
            Debug.LogError("referenceImage es null en IsInActiveZone.");
            return false;
        }

        float distance = Mathf.Abs(rectTransform.anchoredPosition.x - referenceImage.anchoredPosition.x);
        return distance <= okZoneWidth / 2f; // Compatibilidad con lógica previa
    }

    public string GetAccuracyZone()
    {
        if (referenceImage == null)
        {
            Debug.LogError("referenceImage es null en GetAccuracyZone.");
            return null;
        }

        float distance = Mathf.Abs(rectTransform.anchoredPosition.x - referenceImage.anchoredPosition.x);

        if (distance <= excellentZoneWidth / 2f)
        {
            return "EXCELLENT";
        }
        else if (distance <= goodZoneWidth / 2f)
        {
            return "GOOD";
        }
        else if (distance <= okZoneWidth / 2f)
        {
            return "OK";
        }
        return "FAIL";
    }

    void OnDrawGizmos()
    {
        if (referenceImage == null) return;

        Vector3 center = referenceImage.position;

        // OK Zone
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, new Vector3(okZoneWidth, 20, 0));

        // GOOD Zone
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(goodZoneWidth, 20, 0));

        // EXCELLENT Zone
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(excellentZoneWidth, 20, 0));
    }
}
