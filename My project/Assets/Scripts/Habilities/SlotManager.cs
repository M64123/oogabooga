using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlotManager : MonoBehaviour
{
    public DropSlot[] slots; // Asigna los DropSlots en el Inspector (asegúrate del orden deseado)
    public Button fightButton; // Botón que inicia la pelea

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        fightButton.interactable = false;
        CheckSlots();
    }

    private void Update()
    {
        CheckSlots();
    }

    private void CheckSlots()
    {
        bool hasDinos = false;
        foreach (DropSlot slot in slots)
        {
            if (slot.item != null)
            {
                hasDinos = true;
                break;
            }
        }
        fightButton.interactable = hasDinos;
    }

    public void OrderAndStartBattle()
    {
        List<string> teamIDs = new List<string>();

        // Recorre los slots en el orden deseado (por ejemplo, de derecha a izquierda)
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            Debug.Log($"Slot {slots[i].slotIndex} tiene item: " + (slots[i].item != null ? slots[i].item.name : "null"));
            if (slots[i].item != null)
            {
                // Se asume que el dinosaurio tiene un componente DinoImage que contiene el dinoID.
                string dinoID = slots[i].item.GetComponent<DinoImage>().dinoID;
                teamIDs.Add(dinoID);
                Debug.Log($"Slot {slots[i].slotIndex}: Dino agregado con ID {dinoID}");
                slots[i].item = null; // Limpia el slot.
            }
        }

        if (TeamManager.Instance != null)
        {
            TeamManager.Instance.SetTeam(teamIDs);
            Debug.Log("TeamManager.teamIDs.Count después de SetTeam: " + TeamManager.Instance.teamIDs.Count);
        }
        else
        {
            Debug.LogWarning("SlotManager: No se encontró TeamManager.");
        }

        // Cargar la escena de combate.
        SceneManager.LoadScene("Propuesta Juanpa 1");
    }
}