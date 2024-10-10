using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float transitionDuration = 1f; // Duraci�n de la transici�n
    public Vector3 initialPosition; // Posici�n inicial de la c�mara
    public Vector3 targetPosition; // Posici�n objetivo de la c�mara
    public Vector3 initialRotation; // Rotaci�n inicial de la c�mara
    public Vector3 targetRotation; // Rotaci�n objetivo de la c�mara

    private bool isTransitioning = false;
    private float transitionProgress = 0f;

    void Start()
    {
        // Configurar la posici�n y rotaci�n inicial
        initialPosition = new Vector3(transform.position.x, 3f, transform.position.z);
        targetPosition = new Vector3(transform.position.x, 4f, transform.position.z);

        initialRotation = new Vector3(30f, transform.eulerAngles.y, transform.eulerAngles.z);
        targetRotation = new Vector3(45f, transform.eulerAngles.y, transform.eulerAngles.z);

        // Aplicar la posici�n y rotaci�n inicial
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;

        // Comenzar la transici�n
        StartTransition();
    }

    void Update()
    {
        if (isTransitioning)
        {
            // Actualizar el progreso de la transici�n
            transitionProgress += Time.deltaTime / transitionDuration;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }

            // Interpolar posici�n y rotaci�n
            transform.position = Vector3.Lerp(initialPosition, targetPosition, transitionProgress);
            Vector3 currentRotation = Vector3.Lerp(initialRotation, targetRotation, transitionProgress);
            transform.eulerAngles = currentRotation;
        }

        // Hacer que la c�mara siga al cursor
        FollowCursor();
    }

    void StartTransition()
    {
        isTransitioning = true;
        transitionProgress = 0f;
    }

    void FollowCursor()
    {
        // Obtener la posici�n del cursor en el mundo
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 targetPosition = new Vector3(point.x, transform.position.y, point.z);

            // Suavizar el seguimiento con Lerp
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 2f);
        }
    }
}
