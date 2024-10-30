using UnityEngine;

public class CharacterClickHandler : MonoBehaviour
{
    public int characterIndex; // �ndice del personaje en el arreglo del controlador
    private CharacterSelectionController selectionController;

    void Start()
    {
        selectionController = FindObjectOfType<CharacterSelectionController>();
    }

    void OnMouseDown()
    {
        selectionController.OnCharacterClicked(characterIndex);
    }
}
