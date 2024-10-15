using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform target;        // El transform del jugador
    public float smoothSpeed = 0.125f;  // Velocidad de suavizado
    public Vector3 offset;          // Desplazamiento de la c�mara respecto al jugador

    private float initialX;
    private float initialY;

    void Start()
    {
        if (target != null)
        {
            // Guardar las posiciones iniciales X e Y de la c�mara
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
            Debug.Log("CameraFollowZ: El objetivo (jugador) no est� asignado en Start. Se asignar� cuando el jugador sea instanciado.");
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Mantener X e Y constantes, solo actualizar Z
            float desiredZ = target.position.z + offset.z;
            Vector3 desiredPosition = new Vector3(initialX, initialY, desiredZ);

            // Suavizar el movimiento de la c�mara
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Actualizar la posici�n de la c�mara
            transform.position = smoothedPosition;
        }
    }
}
