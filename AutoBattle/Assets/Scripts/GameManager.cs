using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public TMP_Text BarbarianText;
    public TMP_Text RangerText;
    public TMP_Text ShamanText;
    public TMP_Text FighterText;
    public TMP_Text BarbarianDesc;
    public TMP_Text RangerDesc;
    public TMP_Text ShamanDesc;
    public TMP_Text FighterDesc;
    void Start()
    {
        BarbarianText.text = "Class: Barbarian";
        RangerText.text = "Class: Ranger";
        ShamanText.text = "Class: Shaman";
        FighterText.text = "Class: Fighter";
        BarbarianDesc.text = "Zoro One Piece";
        RangerDesc.text = "Usopp One Piece";
        ShamanDesc.text = "Nami One Piece";
        FighterDesc.text = "Luffy One Piece";

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
