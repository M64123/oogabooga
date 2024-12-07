using UnityEngine;
using System.Collections.Generic;

public class Combatgrid : MonoBehaviour
{
    public List<Transform> gridSlots = new List<Transform>(); // Lista de slots en la grid
    public GameObject slotPrefab;      // Prefab del slot
    public Transform gridParent;       // Parent de la grid
    public int initialSlots = 3;       // Número de slots iniciales
    public float slotSpacing = 2.0f;   // Espaciado entre los slots

    public Vector3 slotScale = new Vector3(0.11f, 0.15f, 1f); // Escala personalizada para los slots

    private void Start()
    {
        GenerateInitialSlots();
    }

    private void GenerateInitialSlots()
    {
        for (int i = 0; i < initialSlots; i++)
        {
            AddSlot(i);
        }
    }

    public void AddSlot(int index)
    {
        Vector3 slotPosition = gridParent.position - new Vector3(index * slotSpacing, 0, 0);

        GameObject newSlot = Instantiate(slotPrefab, slotPosition, Quaternion.identity);
        newSlot.transform.SetParent(gridParent, false);
        newSlot.transform.localScale = slotScale;
        newSlot.transform.localPosition = new Vector3(-index * slotSpacing, 0, 0);

        gridSlots.Add(newSlot.transform);
    }

    public int NumberOfOccupiedSlots()
    {
        int occupiedCount = 0;

        foreach (Transform slot in gridSlots)
        {
            if (slot.childCount > 0)
            {
                occupiedCount++;
            }
        }

        return occupiedCount;
    }

    public void ReorganizeSlots()
    {
        List<Transform> dinos = new List<Transform>();

        foreach (Transform slot in gridSlots)
        {
            if (slot.childCount > 0)
            {
                dinos.Add(slot.GetChild(0));
            }
        }

        int slotIndex = 0;
        foreach (Transform dino in dinos)
        {
            dino.SetParent(gridSlots[slotIndex]);
            dino.localPosition = Vector3.zero;
            slotIndex++;
        }
    }

    public void EnsureSlotAvailability()
    {
        if (AllSlotsOccupied())
        {
            AddSlot(gridSlots.Count);
        }
    }

    private bool AllSlotsOccupied()
    {
        foreach (Transform slot in gridSlots)
        {
            if (slot.childCount == 0)
            {
                return false;
            }
        }
        return true;
    }

    public void RemoveTemporarySlots()
    {
        for (int i = gridSlots.Count - 1; i >= initialSlots; i--)
        {
            if (gridSlots[i].childCount == 0)
            {
                Destroy(gridSlots[i].gameObject);
                gridSlots.RemoveAt(i);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (gridSlots == null) return;

        Gizmos.color = Color.blue;
        foreach (Transform slot in gridSlots)
        {
            Gizmos.DrawWireCube(slot.position, slotScale);
        }
    }
}

