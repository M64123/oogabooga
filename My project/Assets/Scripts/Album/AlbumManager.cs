using System.Collections.Generic;
using UnityEngine;

public class AlbumManager : MonoBehaviour
{
    // Lista de slots asignados manualmente en el Inspector (menu desplegable y ampliable)
    public List<AlbumSlot> albumSlots;

    void Start()
    {
        // Al iniciar el álbum, se actualizan los slots según los datos guardados
        UpdateAlbumSlots();
    }

    // Actualiza cada slot comprobando si su dinoID se encuentra en el JSON (SaveManager)
    public void UpdateAlbumSlots()
    {
        // Obtenemos la lista de IDs desbloqueados (guardados en el JSON) a través del SaveManager
        List<string> unlockedIDs = SaveManager.Instance.GetUnlockedDinoIDs();

        // Recorremos la lista de slots asignados manualmente
        foreach (AlbumSlot slot in albumSlots)
        {
            if (unlockedIDs.Contains(slot.dinoID))
            {
                // Si el ID está guardado, se muestra el slot
                slot.ShowDino();
            }
            else
            {
                // Si no está guardado, se oculta o se muestra con placeholder
                slot.HideDino();
            }
        }
    }
}
