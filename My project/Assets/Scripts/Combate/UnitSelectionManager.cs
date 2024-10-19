using UnityEngine;

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
                // Intentar colocar la unidad en el "Arena"
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

                        // Regresar la cámara al "Box"
                        cameraController.TransitionToBox();
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
            // Opcional: Cambiar apariencia de la unidad para indicar que está seleccionada
            // Iniciar transición de la cámara al "Arena"
            cameraController.TransitionToArena();
        }
    }

    public void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            // Opcional: Revertir cambios de apariencia
            selectedUnit = null;
            cameraController.TransitionToBox();
        }
    }
}
