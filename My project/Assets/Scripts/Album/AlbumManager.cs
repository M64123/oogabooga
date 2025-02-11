using System.Collections.Generic;
using UnityEngine;

public class AlbumManager : MonoBehaviour
{
    // Prefab del slot que tiene el script AlbumSlot
    public GameObject albumSlotPrefab;
    // Parent donde se instanciarán los slots (por ejemplo, un objeto con Grid Layout Group)
    public Transform slotsParent;

    void Start()
    {
        // Obtenemos la lista completa de IDs desde el GameManager
        List<string> allDinoIDs = GameManager.Instance.dinoIDs;
        // Obtenemos el diccionario de dinos desbloqueados (los que el jugador ya obtuvo)
        Dictionary<string, GameManager.DinoData> unlockedDinos = GameManager.Instance.GetAllPlayerDinos();

        // Por cada ID disponible, creamos un slot
        foreach (string dinoID in allDinoIDs)
        {
            GameObject slotGO = Instantiate(albumSlotPrefab, slotsParent);
            AlbumSlot slot = slotGO.GetComponent<AlbumSlot>();
            slot.dinoID = dinoID;

            if (unlockedDinos.ContainsKey(dinoID))
            {
                // Si el dino está desbloqueado, se muestra el slot
                slot.ShowDino();
                // Asignamos, por ejemplo, el nombre del dino (puedes ampliarlo según lo necesites)
                slot.dinoNameText.text = unlockedDinos[dinoID].dinoName;
                // Si cuentas con la imagen (sprite) del dino, asigna también: slot.dinoImage.sprite = ...;
            }
            else
            {
                // Si el dino no está desbloqueado, ocultamos el slot (o se podría mostrar un placeholder)
                slot.HideDino();
                // Opcional: si prefieres mostrar el slot con un “???”, en lugar de desactivarlo, haz:
                // slot.gameObject.SetActive(true);
                // slot.dinoNameText.text = "???";
            }
        }
    }
}
