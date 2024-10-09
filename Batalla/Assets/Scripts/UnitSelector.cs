using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelector : MonoBehaviour
{
    void OnMouseDown()
    {
        if (transform.parent.name == "Arena")
        {
            transform.SetParent(GameObject.Find("UnitPlane").transform);
            // Posición aleatoria dentro de los límites
            float minX = -((10 * 1.024f) / 2) * 0.9f;
            float maxX = ((10 * 1.024f) / 2) * 0.9f;
            float randomX = UnityEngine.Random.Range(minX, maxX);
            transform.position = new Vector3(randomX, transform.position.y, -1); // Z = -1 para el plano frontal
            GetComponent<UnitMovement>().enabled = true;
            UnitManager.instance.RemoveUnit();
        }
    }
}
