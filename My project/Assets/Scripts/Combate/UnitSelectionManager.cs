using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelectionManager : MonoBehaviour
{
    public static UnitSelectionManager instance;

    private GameObject selectedUnit = null;
    private CameraController cameraController;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    void Update()
    {
        if (selectedUnit != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Intentar colocar o remover la unidad
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.name == "Arena" && UnitManager.instance.CanPlaceUnit())
                    {
                        // Colocar la unidad en la posición del clic
                        selectedUnit.transform.position = new Vector3(hit.point.x, selectedUnit.transform.position.y, hit.point.z);
                        selectedUnit.transform.SetParent(hit.collider.transform);
                        UnitManager.instance.AddUnit();
                        selectedUnit.GetComponent<UnitIdleMovement>().enabled = false;
                        selectedUnit = null;

                        // Regresar la cámara a la Posición 1 (BoxArea)
                        cameraController.TransitionToPosition1();
                    }
                    else if (hit.collider.gameObject.name == "BoxArea")
                    {
                        // Quitar la unidad del Arena y regresarla al BoxArea
                        selectedUnit.transform.SetParent(hit.collider.transform);
                        selectedUnit.GetComponent<UnitIdleMovement>().enabled = true;
                        UnitManager.instance.RemoveUnit();
                        selectedUnit = null;

                        // Regresar la cámara a la Posición 1 (BoxArea)
                        cameraController.TransitionToPosition1();
                    }
                }
            }
        }
    }

    public void SelectUnit(GameObject unit)
    {
        if (selectedUnit == null)
        {
            selectedUnit = unit;
            // Detener el movimiento de la unidad
            unit.GetComponent<UnitIdleMovement>().enabled = false;
            // Transición a la Posición 2 (Arena)
            cameraController.TransitionToPosition2();
        }
    }

    public void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            // Reanudar el movimiento de la unidad
            selectedUnit.GetComponent<UnitIdleMovement>().enabled = true;
            selectedUnit = null;
            // Regresar la cámara a la Posición 1 (BoxArea)
            cameraController.TransitionToPosition1();
        }
    }
}
