using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cancion
{
    public string nombreCancion;

    [Header("Clips de Intro y Transición (opcionales)")]
    public AudioMidiPair introPair;
    public AudioMidiPair transicionPair;

    [Header("Clips de Ataque (en orden cíclico)")]
    public List<AudioMidiPair> ataquePairs = new List<AudioMidiPair>();

    [Header("Clips de Defensa (en orden cíclico)")]
    public List<AudioMidiPair> defensaPairs = new List<AudioMidiPair>();
}
