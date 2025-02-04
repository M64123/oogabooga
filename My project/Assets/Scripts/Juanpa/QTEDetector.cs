using UnityEngine;

/// <summary>
/// Ejemplo de script que detecta colisiones 2D con GHQTEIndicator
/// si el prefab tiene un 'tag' o un collider 2D.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class QTEDetector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ver si colisiona con GHQTEIndicator
        GHQTEIndicator indicator = collision.GetComponent<GHQTEIndicator>();
        if (indicator)
        {
            Debug.Log($"[QTEDetector] El QTEIndicator de tipo '{indicator.name}' pasó por la barra!");
            // Aquí podrías forzar un check de input, 
            // o algo distinto a la lógica normal.
        }
    }
}
