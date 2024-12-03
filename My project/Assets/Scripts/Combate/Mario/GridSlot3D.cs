using UnityEngine;

public class GridSlot3D : MonoBehaviour
{
    public bool IsOccupied()
    {
        // Retorna verdadero si hay un hijo en este slot
        return transform.childCount > 0;
    }

    public void RemoveOccupant()
    {
        // Elimina la relación padre-hijo del objeto en el slot
        if (transform.childCount > 0)
        {
            Transform occupant = transform.GetChild(0);
            occupant.SetParent(null);
        }
    }
}
