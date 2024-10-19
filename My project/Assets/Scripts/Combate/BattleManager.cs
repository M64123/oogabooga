using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public Button fightButton;

    void Start()
    {
        fightButton.onClick.AddListener(StartBattle);
    }

    void StartBattle()
    {
        // Activar las unidades en la arena
        foreach (Transform unit in GameObject.Find("Arena").transform)
        {
            unit.GetComponent<CombatUnit>().enabled = true;
        }

        // Detener el movimiento de las unidades en el plano frontal
        foreach (Transform unit in GameObject.Find("UnitPlane").transform)
        {
            unit.GetComponent<UnitIdleMovement>().enabled = false;
        }

        // Desactivar el botón para evitar múltiples clics
        fightButton.interactable = false;
    }
}
