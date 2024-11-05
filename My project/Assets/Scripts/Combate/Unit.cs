using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SelectionManager.Instance.allUnitsList.Add(gameObject);   
    }

    private void OnDestroy()
    {
        SelectionManager.Instance.allUnitsList.Remove(gameObject);
    }
}
