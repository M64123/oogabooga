using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlbumManager : MonoBehaviour
{
    // Lista de “slots” en el HUD. Cada slot tiene (por ejemplo) un identificador (dinoID), imagen, nombre y estadística.
    public List<AlbumSlot> albumSlots;

    void Start()
    {
        List<string> unlockedIDs = SaveManager.Instance.GetUnlockedDinoIDs();

        // Por cada slot, activamos o no el contenido dependiendo si su ID está en la lista.
        foreach (AlbumSlot slot in albumSlots)
        {
            if (unlockedIDs.Contains(slot.dinoID))
            {
                // Activa el slot y carga los datos (imagen, nombre, etc.)
                slot.ShowDino();
            }
            else
            {
                // Mantiene el slot desactivado o muestra el placeholder en blanco.
                slot.HideDino();
            }
        }
    }
}
