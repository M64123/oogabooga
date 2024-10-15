using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform target;        // El transform del jugador
    public float smoothSpeed = 0.125f;  // Velocidad de suavizado
    public Vector3 offset;          // Desplazamiento de la cámara respecto al jugador

    private float initialX;
    private float initialY;

    void Start()
    {
        if (target != null)
        {
            // Guardar las posiciones iniciales X e Y de la cámara
            initialX = transform.position.x;
            initialY = transform.position.y;

            // Calcular el desplazamiento inicial si no se ha establecido
            if (offset == Vector3.zero)
            {
                offset = transform.position - target.position;
            }
        }
        else
        {
            Debug.Log("CameraFollowZ: El objetivo (jugador) no está asignado en Start. Se asignará cuando el jugador sea instanciado.");
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Mantener X e Y constantes, solo actualizar Z
            float desiredZ = target.position.z + offset.z;
            Vector3 desiredPosition = new Vector3(initialX, initialY, desiredZ);

            // Suavizar el movimiento de la cámara
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Actualizar la posición de la cámara
            transform.position = smoothedPosition;
        }
    }
}
