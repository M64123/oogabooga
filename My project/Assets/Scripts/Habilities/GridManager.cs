using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public DropSlot[] slots; // Asigna los DropSlots en el inspector

    void Start()
    {
        // Ordenar los slots de derecha a izquierda
        System.Array.Sort(slots, (a, b) => b.transform.position.x.CompareTo(a.transform.position.x));

        // Asignar índices de posición (1 es el más a la derecha)
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].slotIndex = i + 1;
        }
    }
}
