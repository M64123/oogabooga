using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    private bool isSelected = false;

    void OnMouseDown()
    {
        if (!isSelected && transform.parent.name == "BoxArea")
        {
            // Seleccionar la unidad
            isSelected = true;
            GetComponent<UnitIdleMovement>().enabled = false; // Detener movimiento
            UnitSelectionManager.instance.SelectUnit(gameObject);
        }
        else if (isSelected)
        {
            // Deseleccionar la unidad
            isSelected = false;
            GetComponent<UnitIdleMovement>().enabled = true; // Reanudar movimiento
            UnitSelectionManager.instance.DeselectUnit();
        }
    }
}
