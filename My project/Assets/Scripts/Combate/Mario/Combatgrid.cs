using System.Collections.Generic;
using UnityEngine;



public class Combatgrid : MonoBehaviour
{
    public GameObject gridSlotPrefab; // Prefab de las casillas
    public Transform gridOrigin;     // Origen de la grid en el suelo
    public int minGridSize = 3;      // Tamaño mínimo de la grid
    public float slotSpacing = 1.1f; // Espaciado entre las casillas

    private List<GameObject> gridSlots = new List<GameObject>();

    private void Start()
    {
        // Genera la grid inicial
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
        newSlot.transform.SetParent(gridOrigin); // Organiza los slots bajo el origen
        newSlot.tag = "Slot"; // Asegúrate de que el prefab tenga el tag "Slot"
        gridSlots.Add(newSlot);
    }

    public void RemoveSlot()
    {
        if (gridSlots.Count > minGridSize)
        {
            GameObject slotToRemove = gridSlots[0];
            gridSlots.RemoveAt(0);
            Destroy(slotToRemove);

            // Reorganiza los slots restantes
            for (int i = 0; i < gridSlots.Count; i++)
            {
                gridSlots[i].transform.position = gridOrigin.position + new Vector3(i * slotSpacing, 0, 0);
            }
        }
    }

    public void UpdateGrid()
    {
        // Ajusta el tamaño de la grid si es necesario
        if (gridSlots.Count > minGridSize && gridSlots[0].transform.childCount == 0)
        {
            RemoveSlot();
        }
    }

    public void EnsureEnoughSlots()
    {
        // Comprueba si todos los slots están ocupados
        bool allSlotsOccupied = true;
        foreach (GameObject slot in gridSlots)
        {
            if (slot.transform.childCount == 0)
            {
                allSlotsOccupied = false;
                break;
            }
        }

        // Si están ocupados, añade un nuevo slot
        if (allSlotsOccupied)
        {
            AddSlot();
        }
    }
}

