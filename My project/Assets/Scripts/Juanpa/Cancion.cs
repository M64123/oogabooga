using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Cancion
{
    [Header("Nombre para identificar la canción")]
    public string nombreCancion;

    [Header("Audio base que suena en loop toda la canción")]
    public AudioClip baseClip;

    [Header("Intro (opcional)")]
    [Tooltip("Activa/desactiva el uso de intro.")]
    public bool useIntro;
    public AudioMidiPair introPair;  // Se usa solo si useIntro == true

    [Header("Transición (opcional)")]
    [Tooltip("Activa/desactiva la transición entre secciones (Ataque↔Defensa).")]
    public bool useTransicion;
    public AudioMidiPair transicionPair; // Se usa solo si useTransicion == true

    [Header("MIDIs para Ataque (en orden cíclico)")]
    public List<MidiAsset> ataqueMidis = new List<MidiAsset>();

    [Header("MIDIs para Defensa (en orden cíclico)")]
    public List<MidiAsset> defensaMidis = new List<MidiAsset>();
}
