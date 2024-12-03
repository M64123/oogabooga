using UnityEngine;

public class GridSlot3D : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Detecta si un dinosaurio entra en el slot
        if (other.CompareTag("Dino") && transform.childCount == 0)
        {
            other.transform.SetParent(transform);
            other.transform.localPosition = Vector3.zero;
        }
    }
}
