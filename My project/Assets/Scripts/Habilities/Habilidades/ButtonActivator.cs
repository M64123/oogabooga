using UnityEngine;

public class ButtonActivator : MonoBehaviour
{
    // Objeto que se activará o desactivará.
    public GameObject targetObject;

    // Función para activar el objeto.
    public void ActivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("targetObject no está asignado en " + gameObject.name);
        }
    }

    // Función para desactivar el objeto.
    public void DeactivateObject()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("targetObject no está asignado en " + gameObject.name);
        }
    }
}