using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance;
    public int maxUnits = 3;
    private int currentUnits = 0;

    void Awake()
    {
        instance = this;
    }

    public bool CanPlaceUnit()
    {
        return currentUnits < maxUnits;
    }

    public void AddUnit()
    {
        currentUnits++;
    }

    public void RemoveUnit()
    {
        if (currentUnits > 0)
            currentUnits--;
    }
}
