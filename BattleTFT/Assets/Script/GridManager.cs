using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int rows = 4;
    public int columns = 8;
    public float tileSize = 2.0f;
    public GameObject tilePrefab;
    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < columns; x++)
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, z * tileSize);
                Instantiate(tilePrefab, pos, Quaternion.identity, transform);
            }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
