// CoinFlipCameraController.cs
using UnityEngine;
using System.Collections;

public class CoinFlipCameraController : MonoBehaviour
{
    public Transform coinTransform; // Asignar en el Inspector
    public Vector3 initialPosition; // Asignar en el Inspector
    public Vector3 finalPosition;   // Asignar en el Inspector
    public Vector3 initialEulerAngles; // Asignar en el Inspector
    public Vector3 finalEulerAngles;   // Asignar en el Inspector
    public float movementDuration = 2f;
    public float followOffsetY = 2f;
    public float curveHeight = 2f; // Altura máxima de la curva

    private bool isMoving = false;
    private float movementStartTime;
    private Quaternion initialRotation;
    private Quaternion finalRotation;
    private CoinFlip coinFlip;

    void Start()
    {
        // Convertir los ángulos de Euler a Quaternions
        initialRotation = Quaternion.Euler(initialEulerAngles);
        finalRotation = Quaternion.Euler(finalEulerAngles);

        // Establecer la posición y rotación inicial
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Obtener la referencia al script CoinFlip
        if (coinTransform != null)
        {
            coinFlip = coinTransform.GetComponent<CoinFlip>();
        }
    }

    void Update()
    {
        // No iniciar el movimiento de la cámara hasta que la moneda haya sido lanzada
        if (!isMoving && coinFlip != null && coinFlip.IsThrown)
        {
            StartCameraMovement();
        }
    }

    void LateUpdate()
    {
        if (isMoving)
            return;

        if (coinFlip != null && coinFlip.IsThrown)
        {
            // Seguir a la moneda manteniendo el offset
            Vector3 offset = new Vector3(0f, followOffsetY, 0f);
            Vector3 targetPos = coinTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);
        }
    }

    void StartCameraMovement()
    {
        isMoving = true;
        movementStartTime = Time.time;

        // Iniciar la corutina para mover la cámara
        StartCoroutine(MoveCameraAlongCurve());
    }

    IEnumerator MoveCameraAlongCurve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < movementDuration)
        {
            elapsedTime = Time.time - movementStartTime;
            float t = Mathf.Clamp01(elapsedTime / movementDuration);

            // Usar una interpolación suave (SmoothStep) para t
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Calcular la nueva posición en la curva
            Vector3 newPosition = CalculateCurvePosition(smoothT);

            // Interpolar la rotación de forma suave
            Quaternion newRotation = Quaternion.Slerp(initialRotation, finalRotation, smoothT);

            // Actualizar la posición y rotación de la cámara
            transform.position = newPosition;
            transform.rotation = newRotation;

            yield return null;
        }

        // Asegurarse de que la cámara llegue exactamente a la posición y rotación final
        transform.position = finalPosition;
        transform.rotation = finalRotation;
    }

    Vector3 CalculateCurvePosition(float t)
    {
        // Definir el punto de control para la curva (ajusta la posición en Y para controlar la altura)
        Vector3 controlPoint = (initialPosition + finalPosition) / 2 + Vector3.up * curveHeight;

        // Calcular la posición en la curva de Bezier cuadrática
        Vector3 position = Mathf.Pow(1 - t, 2) * initialPosition
                         + 2 * (1 - t) * t * controlPoint
                         + Mathf.Pow(t, 2) * finalPosition;

        return position;
    }
}
