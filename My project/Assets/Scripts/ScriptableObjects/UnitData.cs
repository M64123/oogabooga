using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnit", menuName = "Units/Unit Data")]
public class UnitData : MonoBehaviour
{
    public string unitName;
    public float maxHealth;
    public float damage;
    public float speed;
    public float detectionRange;
    public GameObject unitPrefab;

}
