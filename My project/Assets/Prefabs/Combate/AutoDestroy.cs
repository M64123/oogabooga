using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 2f; // Tiempo en segundos antes de destruir el prefab

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
