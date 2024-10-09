using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void OnMouseDown()
    {
        if (transform.parent.name == "Arena")
        {
            // Remover la unidad de la arena
            transform.SetParent(GameObject.Find("UnitPlane").transform);

            // Posición aleatoria dentro de los límites
            float minX = -((10 * 1.024f) / 2) * 0.9f;
            float maxX = ((10 * 1.024f) / 2) * 0.9f;
            float randomX = Random.Range(minX, maxX);
            float randomZ = -1f; // Z para el plano frontal
            transform.position = new Vector3(randomX, transform.position.y, randomZ);

            GetComponent<UnitIdleMovement>().enabled = true;
            UnitManager.instance.RemoveUnit();
        }
    }
}
