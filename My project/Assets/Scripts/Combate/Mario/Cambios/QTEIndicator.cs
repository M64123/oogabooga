using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTEIndicator : MonoBehaviour
{
    private KeyCode qteKey;
    private float activeZoneWidth;
    private RectTransform referenceImage; // Imagen de referencia para la zona activa
    private RectTransform rectTransform; // Transform del propio QTEIndicator
    private Vector2 startPosition;

    private bool hasPassedZone = false;

    public void Setup(KeyCode key, float zoneWidth, Vector2 startPosition, RectTransform referenceImage)
    {
        this.qteKey = key;
        this.activeZoneWidth = zoneWidth;
        this.startPosition = startPosition;
        this.referenceImage = referenceImage;

        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
        }
        else
        {
            Debug.LogError("El QTEIndicator no tiene un RectTransform.");
        }
    }

    void Update()
    {
        if (rectTransform == null || referenceImage == null) return;

        // Mover el QTE de izquierda a derecha
        rectTransform.anchoredPosition += Vector2.right * Time.deltaTime * 200f;

        // Detectar si ha pasado la zona activa
        if (!hasPassedZone && rectTransform.anchoredPosition.x >= referenceImage.anchoredPosition.x)
        {
            hasPassedZone = true;
            Debug.Log("Has fallado pringao");
            QTEManagerCombat.Instance.OnQTEFail(this);
        }

        // Detectar si el jugador pulsa la tecla en el momento correcto y es el QTE más cercano en la zona
        if (Input.GetKeyDown(qteKey) && IsInActiveZone() && IsFirstInActiveZone())
        {
            Debug.Log("¡QTE Acierto!");
            QTEManagerCombat.Instance.OnQTESuccess(this);
        }
    }

    public bool IsInActiveZone()
    {
        if (referenceImage == null) return false;

        float indicatorX = rectTransform.anchoredPosition.x;
        float activeZoneStart = referenceImage.anchoredPosition.x - activeZoneWidth / 2;
        float activeZoneEnd = referenceImage.anchoredPosition.x + activeZoneWidth / 2;

        return indicatorX >= activeZoneStart && indicatorX <= activeZoneEnd;
    }

    public bool IsFirstInActiveZone()
    {
        // Verificar si es el primer QTE en la zona activa
        return QTEManagerCombat.Instance.GetFirstQTEInActiveZone() == this;
    }

    public Vector2 GetPosition()
    {
        return rectTransform.anchoredPosition;
    }
}