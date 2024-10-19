using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float transitionDuration = 1f; // Duración de la transición
    public Vector3 boxPosition = new Vector3(0f, 3f, -6f);
    public Vector3 arenaPosition = new Vector3(0f, 4f, -6f);
    public Vector3 boxRotation = new Vector3(30f, 0f, 0f);
    public Vector3 arenaRotation = new Vector3(45f, 0f, 0f);

    private Vector3 targetPosition;
    private Vector3 targetRotation;
    private bool isTransitioning = false;
    private float transitionProgress = 0f;
    private Vector3 startPosition;
    private Vector3 startRotation;

    void Start()
    {
        // Inicialmente, la cámara está en la posición del "Box"
        transform.position = boxPosition;
        transform.eulerAngles = boxRotation;
    }

    void Update()
    {
        // Verificar si se está en transición
        if (isTransitioning)
        {
            // Actualizar el progreso de la transición
            transitionProgress += Time.deltaTime / transitionDuration;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }

            // Interpolar posición y rotación
            transform.position = Vector3.Lerp(startPosition, targetPosition, transitionProgress);
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, transitionProgress);
            transform.eulerAngles = currentRotation;
        }
    }

    public void TransitionToArena()
    {
        // Iniciar transición a la posición del "Arena"
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        targetPosition = arenaPosition;
        targetRotation = arenaRotation;
        transitionProgress = 0f;
        isTransitioning = true;
    }

    public void TransitionToBox()
    {
        // Iniciar transición a la posición del "Box"
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        targetPosition = boxPosition;
        targetRotation = boxRotation;
        transitionProgress = 0f;
        isTransitioning = true;
    }
}
