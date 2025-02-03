using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AlbumUI : MonoBehaviour
{
    [Header("Referencia a la BD de Dinosaurios")]
    public DinosaurDatabase dinoDatabase;

    [Header("Slots de la página (9 por página)")]
    public List<Image> slotImages; // Asigna en el inspector las imágenes (Slot_0, Slot_1, etc.)

    [Header("Botones de paginación")]
    public Button buttonPrevPage;
    public Button buttonNextPage;

    private int currentPage = 0;
    private int slotsPerPage = 9;

    void Start()
    {
        if (buttonPrevPage != null)
            buttonPrevPage.onClick.AddListener(PrevPage);

        if (buttonNextPage != null)
            buttonNextPage.onClick.AddListener(NextPage);

        ShowPage(currentPage);
    }

    private void ShowPage(int pageIndex)
    {
        // Cálculo de rangos
        int startIndex = pageIndex * slotsPerPage;
        int endIndex = startIndex + slotsPerPage;

        // Obtenemos la lista de dinos en la BD
        var allDinos = dinoDatabase.dinosaurs;

        // Recorremos cada slot
        for (int i = 0; i < slotsPerPage; i++)
        {
            int dinoIndex = startIndex + i;
            if (dinoIndex < allDinos.Count)
            {
                // Hay un dino para este slot
                var dinoDef = allDinos[dinoIndex];
                // Chequeamos si el jugador ya tiene este dino
                bool unlocked = PlayerHasDino(dinoDef.dinoID);

                // Asignamos el sprite si lo tiene, o un placeholder
                if (unlocked)
                {
                    slotImages[i].sprite = dinoDef.dinoSprite;
                    // OPCIONAL: Asigna un "click handler" para cambiar cursor
                    AddClickHandler(slotImages[i].gameObject, dinoDef);
                }
                else
                {
                    slotImages[i].sprite = null; // o sprite "?"
                    RemoveClickHandler(slotImages[i].gameObject);
                }
            }
            else
            {
                // No hay dino para este slot (página incompleta)
                slotImages[i].sprite = null;
                RemoveClickHandler(slotImages[i].gameObject);
            }
        }
    }

    private bool PlayerHasDino(int dinoID)
    {
        // 1) Revisamos la lista de dinos del jugador
        foreach (var dData in GameManager.Instance.playerDinoList)
        {
            // Convertir string a int si es necesario,
            // o cambia el storedID a int en tu DinoData
            if (dData.dinoID == dinoID.ToString())
            {
                return true;
            }
        }
        return false;
    }

    private void AddClickHandler(GameObject slotObject, DinosaurDefinition dinoDef)
    {
        Button btn = slotObject.GetComponent<Button>();
        if (btn != null)
        {
            // Quitamos handlers previos para evitar duplicados
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => { OnClickDinoCard(dinoDef); });
        }
    }

    private void RemoveClickHandler(GameObject slotObject)
    {
        Button btn = slotObject.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
        }
    }

    private void OnClickDinoCard(DinosaurDefinition dinoDef)
    {
        // Aquí cambias el cursor por uno especial
        // Por ejemplo, supongamos tienes un sprite en CursorManager
        // CursorManager.Instance.SetCursorToDino(dinoDef.dinoSprite);
        Debug.Log("¡Has hecho clic en el dino: " + dinoDef.dinoName);
    }

    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage(currentPage);
        }
    }

    private void NextPage()
    {
        if ((currentPage + 1) * slotsPerPage < dinoDatabase.dinosaurs.Count)
        {
            currentPage++;
            ShowPage(currentPage);
        }
    }

    // Método público para abrir/cerrar el panel
    public void ToggleAlbumPanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
        if (panel.activeSelf)
        {
            ShowPage(currentPage);
        }
    }
}
