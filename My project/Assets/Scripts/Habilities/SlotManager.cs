using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SlotManager : MonoBehaviour
{
    public DropSlot[] slots; // Asigna los DropSlots en el inspector (Ordenados de IZQUIERDA a DERECHA en la jerarquía)
    public Button fightButton; // Botón que inicia la pelea

    private void Start()
    {
        fightButton.interactable = false; // El botón comienza desactivado
        CheckSlots(); // Verificar si hay dinosaurios al iniciar
    }

    private void Update()
    {
        CheckSlots();
    }

    private void CheckSlots()
    {
        bool hasDinos = false;

        // Verificar si hay al menos un dinosaurio en los slots
        foreach (DropSlot slot in slots)
        {
            if (slot.item != null)
            {
                hasDinos = true;
                break;
            }
        }

        // Activar el botón si hay al menos un dinosaurio
        fightButton.interactable = hasDinos;
    }

    public void OrderAndStartBattle()
    {
        // Lista temporal para almacenar los dinosaurios en los slots ocupados
        List<GameObject> dinosEnSlots = new List<GameObject>();

        // Recorrer los slots de derecha a izquierda y almacenar los dinosaurios
        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].item != null)
            {
                dinosEnSlots.Add(slots[i].item);
                slots[i].item = null; // Limpiar los slots
            }
        }

        // Reubicar los dinosaurios de derecha a izquierda en los primeros espacios disponibles
        int newIndex = slots.Length - 1; // Empezamos desde el slot más a la derecha
        for (int i = 0; i < dinosEnSlots.Count; i++)
        {
            slots[newIndex - i].item = dinosEnSlots[i];
            dinosEnSlots[i].transform.SetParent(slots[newIndex - i].transform);
            dinosEnSlots[i].transform.position = slots[newIndex - i].transform.position;
        }

        // Guardar el ID del dinosaurio en la primera posición (el más a la derecha después de ordenar)
        if (slots[slots.Length - 1].item != null)
        {
            string firstDinoID = slots[slots.Length - 1].item.GetComponent<DinoImage>().dinoID;
            PlayerPrefs.SetString("FirstDinoID", firstDinoID);
        }

        // Cargar la escena de combate
        SceneManager.LoadScene("Propuesta Juanpa");
    }
}