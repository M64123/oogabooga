using System.Collections;
using UnityEngine;

public class CoinFlipCameraController : MonoBehaviour
{
    public Transform coinTransform; // Asignar en el Inspector
    public Vector3 initialPosition; // Asignar en el Inspector
    public Vector3 finalPosition;   // Asignar en el Inspector
    public Vector3 initialEulerAngles; // Asignar en el Inspector
    public Vector3 finalEulerAngles;   // Asignar en el Inspector
    public float movementDuration = 2f;
    public float followOffsetY = 2f;
    public float curveHeight = 2f; // Altura m�xima de la curva

    private bool isMoving = false;
    private float movementStartTime;
    private Quaternion initialRotation;
    private Quaternion finalRotation;
    private CoinFlip coinFlip;

    void Start()
    {
        // Convertir los �ngulos de Euler a Quaternions
        initialRotation = Quaternion.Euler(initialEulerAngles);
        finalRotation = Quaternion.Euler(finalEulerAngles);

        // Establecer la posici�n y rotaci�n inicial
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
        // Detectar la tecla Espacio
        if (Input.GetKeyDown(KeyCode.Space) && !isMoving)
        {
            // Iniciar el movimiento de la c�mara
            StartCameraMovement();
        }

        // Reiniciar la moneda con la tecla R (opcional)
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (coinFlip != null)
            {
                coinFlip.ResetCoin();
            }
            // Reiniciar la c�mara a la posici�n y rotaci�n inicial
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    void LateUpdate()
    {
        if (!isMoving && coinFlip != null && coinFlip.IsThrown)
        {
            // Seguir a la moneda manteniendo el offset
            Vector3 offset = new Vector3(0f, followOffsetY, 0f);
            Vector3 targetPos = coinTransform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 5f);

            // Opcionalmente, hacer que la c�mara mire hacia la moneda
            // transform.LookAt(coinTransform);
        }
    }

    void StartCameraMovement()
    {
        isMoving = true;
        movementStartTime = Time.time;

        // Iniciar la corutina para mover la c�mara
        StartCoroutine(MoveCameraAlongCurve());
    }

    IEnumerator MoveCameraAlongCurve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < movementDuration)
        {
            elapsedTime = Time.time - movementStartTime;
            float t = Mathf.Clamp01(elapsedTime / movementDuration);

            // Usar una interpolaci�n suave (SmoothStep) para t
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            // Calcular la nueva posici�n en la curva
            Vector3 newPosition = CalculateCurvePosition(smoothT);

            // Interpolar la rotaci�n de forma suave
            Quaternion newRotation = Quaternion.Slerp(initialRotation, finalRotation, smoothT);

            // Actualizar la posici�n y rotaci�n de la c�mara
            transform.position = newPosition;
            transform.rotation = newRotation;

            yield return null;
        }

        // Asegurarse de que la c�mara llegue exactamente a la posici�n y rotaci�n final
        transform.position = finalPosition;
        transform.rotation = finalRotation;

        // Llamar al m�todo para lanzar la moneda
        LaunchCoin();

        // Iniciar el seguimiento de la moneda
        StartCoroutine(FollowCoin());
    }

    Vector3 CalculateCurvePosition(float t)
    {
        // Definir el punto de control para la curva (ajusta la posici�n en Y para controlar la altura)
        Vector3 controlPoint = (initialPosition + finalPosition) / 2 + Vector3.up * curveHeight;

        // Calcular la posici�n en la curva de Bezier cuadr�tica
        Vector3 position = Mathf.Pow(1 - t, 2) * initialPosition
                         + 2 * (1 - t) * t * controlPoint
                         + Mathf.Pow(t, 2) * finalPosition;

        return position;
    }

    void LaunchCoin()
    {
        // Llamar al m�todo FlipCoin() en el script CoinFlip
        if (coinFlip != null)
        {
            coinFlip.FlipCoin();
        }
    }

    IEnumerator FollowCoin()
    {
        // Posici�n inicial de la c�mara
        Vector3 startPosition = transform.position;

        // Offset deseado en relaci�n a la moneda
        Vector3 offset = new Vector3(0f, followOffsetY, 0f);

        // Duraci�n de la transici�n
        float transitionDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;

            // Posici�n objetivo basada en la posici�n de la moneda m�s el offset
            Vector3 targetPos = coinTransform.position + offset;

            // Interpolar la posici�n de la c�mara
            transform.position = Vector3.Lerp(startPosition, targetPos, t);

            yield return null;
        }

        // Asegurarse de que la c�mara est� en la posici�n correcta
        transform.position = coinTransform.position + offset;

        isMoving = false;
    }
}
