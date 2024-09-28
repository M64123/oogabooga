using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacementManager : MonoBehaviour
{
    public GameObject unitPrefab;
    private GridManager gridManager;
    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlaceUnit(hit.point);
            }
        }
    }

    void PlaceUnit(Vector3 position)
    {
        Vector3 gridPos = GetGridPosition(position);
        Instantiate(unitPrefab, gridPos, Quaternion.identity);
    }

    Vector3 GetGridPosition(Vector3 worldPos)
    {
        float tileSize = gridManager.tileSize;
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int z = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector3(x * tileSize, 0, z * tileSize);
    }
}
