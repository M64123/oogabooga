using UnityEngine;

public class GridSlot3D : MonoBehaviour
{
    public bool IsOccupied()
    {
        return transform.childCount > 0;
    }

    public void SnapDino(Transform dino)
    {
        if (!IsOccupied())
        {
            dino.SetParent(transform);
            dino.localPosition = Vector3.zero;
        }
    }
}
