using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public Button combatButton; // Botón de combatir
    public Combatgrid combatGrid; // Referencia a la grid de combate
    public float movementSpeed = 2.0f; // Velocidad para el movimiento de combate
    public Transform cameraTransform; // Cámara que sigue la grid durante el movimiento
    public Canvas combatCanvas; // Canvas que se activa al iniciar el combate
    public float stopDistance = 5.0f; // Distancia al punto objetivo donde se detiene la grid

    private bool isCombatActive = false;

    void Start()
    {
        combatButton.onClick.AddListener(OnCombatButtonClick);
        combatButton.gameObject.SetActive(false); // Inicialmente desactivar el botón
    }

    void Update()
    {
        //combatGrid.NumberOfOccupiedSlots() > 0)
       
 
        
    }

    private void OnCombatButtonClick()
    {
       // combatGrid.ReorganizeSlots();
        isCombatActive = true;
    }

    private void MoveGridAndCamera()
    {
        Vector3 movement = Vector3.right * movementSpeed * Time.deltaTime;
        combatGrid.transform.position += movement;
        cameraTransform.position += movement;

        if (combatGrid.transform.position.x >= stopDistance)
        {
            isCombatActive = false;
            combatCanvas.gameObject.SetActive(true);
        }
    }
}
