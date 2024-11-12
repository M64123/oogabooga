using UnityEngine;
using System.Collections;

public class CamaraControllerGacha : MonoBehaviour
{
    [Header("Configuraci�n de Interacci�n con el Huevo")]
    public Transform eggTransform;
    public float shakeIntensity = 0.1f;     // Intensidad del temblor
    public float shakeDuration = 0.2f;      // Duraci�n del temblor
    public float moveForwardAmount = 0.5f;  // Distancia hacia adelante al hacer clic en el huevo

    [Header("Configuraci�n de Seguimiento del Cubo")]
    public Vector3 cubeFollowOffset = new Vector3(0f, 2f, -5f);
    public float followSmoothTime = 0.5f; // Tiempo de suavizado para SmoothDamp
    public float lookAtSmoothTime = 0.2f; // Tiempo de suavizado para rotaci�n

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isFollowingCube = false;
    private Transform cubeTransform;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 currentAngularVelocity = Vector3.zero;

    private bool isShaking = false;

    void Start()
    {
        // Guardar la posici�n y rotaci�n original de la c�mara
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    // Llamado desde 'EggBehaviour' cuando el huevo es clicado
    public void OnEggClicked()
    {
        if (!isShaking)
        {
            StartCoroutine(ShakeAndMoveForward());
        }
    }

    // Llamado desde 'EggBehaviour' cuando el cubo es lanzado
    public void StartFollowingCube(Transform cube)
    {
        cubeTransform = cube;
        isFollowingCube = true;
    }

    public void StopFollowingCube()
    {
        isFollowingCube = false;
        StartCoroutine(RestoreCameraPosition());
    }

    IEnumerator ShakeAndMoveForward()
    {
        isShaking = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + transform.forward * moveForwardAmount;

        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / shakeDuration;

            // Mover la c�mara hacia adelante
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Aplicar un peque�o temblor
            Vector3 shakeOffset = Random.insideUnitSphere * shakeIntensity * (1f - t);
            transform.position += shakeOffset;

            yield return null;
        }

        // Asegurarse de que la c�mara est� en la posici�n final correcta
        transform.position = targetPosition;

        // Actualizar la posici�n original para futuros movimientos
        originalPosition = transform.position;

        isShaking = false;
    }

    IEnumerator RestoreCameraPosition()
    {
        float duration = 1f; // Duraci�n de la transici�n de regreso
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            transform.position = Vector3.Lerp(startPosition, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, originalRotation, t);

            yield return null;
        }

        // Asegurar la posici�n y rotaci�n final
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    void LateUpdate()
    {
        if (isFollowingCube && cubeTransform != null)
        {
            // Seguir al cubo con un offset y transici�n suave usando SmoothDamp
            Vector3 desiredPosition = cubeTransform.position + cubeFollowOffset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);

            // Rotaci�n suave hacia el cubo
            Vector3 direction = cubeTransform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / lookAtSmoothTime);
        }
    }
}
