using System.Collections.Generic;
using UnityEngine;



public class Combatgrid : MonoBehaviour
{
    public GameObject gridSlotPrefab; // Prefab de las casillas
    public Transform gridOrigin; // Origen de la grid
    public int minGridSize = 3; // Tamaño mínimo de la grid
    public float slotSpacing = 1.1f; // Espaciado entre los slots

    private List<GameObject> gridSlots = new List<GameObject>(); // Lista de slots actuales

    private void Start()
    {
        // Generar la grid inicial
        for (int i = 0; i < minGridSize; i++)
        {
            AddSlot();
        }
    }

    public void AddSlot()
    {
        // Calcula la posición del nuevo slot
        Vector3 slotPosition = gridOrigin.position + new Vector3(gridSlots.Count * slotSpacing, 0, 0);

        // Instancia el nuevo slot
        GameObject newSlot = Instantiate(gridSlotPrefab, slotPosition, Quaternion.identity);
        newSlot.transform.SetParent(gridOrigin); // Mantén los slots organizados bajo la grid
        newSlot.tag = "Slot"; // Asegúrate de que el slot tenga el tag correcto
        gridSlots.Add(newSlot);
    }

    public bool AreAllSlotsOccupied()
    {
        // Revisa si todos los slots tienen un hijo (es decir, están ocupados)
        foreach (GameObject slot in gridSlots)
        {
            if (slot.transform.childCount == 0)
            {
                return false; // Hay al menos un slot vacío
            }
        }
        return true; // Todos los slots están ocupados
    }

    public void EnsureEnoughSlots()
    {
        // Si todos los slots están ocupados, añade un nuevo slot
        if (AreAllSlotsOccupied())
        {
            AddSlot();
        }
    }
}

