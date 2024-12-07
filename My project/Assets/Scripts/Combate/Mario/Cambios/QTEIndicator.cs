using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEIndicator : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform referenceImage;
    private float activeZoneWidth;
    private bool isLeftSide;

    public void Setup(KeyCode key, float activeWidth, RectTransform spawnPoint, RectTransform target, bool leftSide)
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
        activeZoneWidth = activeWidth;

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
        return distance < 1f; // Un margen pequeño para verificar que alcanzó el centro
    }

    public bool IsInActiveZone()
    {
        if (referenceImage == null)
        {
            Debug.LogError("referenceImage es null en IsInActiveZone.");
            return false;
        }

        float distance = Mathf.Abs(rectTransform.anchoredPosition.x - referenceImage.anchoredPosition.x);
        return distance <= activeZoneWidth / 2f;
    }
}