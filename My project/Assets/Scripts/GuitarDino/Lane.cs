using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Lane : MonoBehaviour
{
    public Melanchall.DryWetMidi.MusicTheory.NoteName noteRestriction;
    public KeyCode input;
    public GameObject notePrefab;

    // Lista que contiene las notas generadas en el loop actual.
    private List<Note> currentNotes = new List<Note>();

    // Los timeStamps se obtienen del MIDI y se reutilizan en cada loop.
    public List<double> timeStamps = new List<double>();

    private int spawnIndex = 0;
    private int inputIndex = 0;

    [Header("Configuración de la Lane")]
    // Para que la lane cause que el dinosaurio del jugador ataque al enemigo.
    public bool isDamageLane = false;
    // Nuevo booleano para que el enemigo ataque al dinosaurio del jugador.
    public bool enemyAttack = false;

    public void SetTimeStamps(Melanchall.DryWetMidi.Interaction.Note[] array)
    {
        timeStamps.Clear();
        foreach (var note in array)
        {
            if (note.NoteName == noteRestriction)
            {
                var metricTimeSpan = TimeConverter.ConvertTo<MetricTimeSpan>(note.Time, SongManager.midiFile.GetTempoMap());
                double newTimeStamp = metricTimeSpan.Minutes * 60 + metricTimeSpan.Seconds + metricTimeSpan.Milliseconds / 1000.0;
                if (!timeStamps.Contains(newTimeStamp))
                {
                    timeStamps.Add(newTimeStamp);
                }
            }
        }
    }

    void Update()
    {
        // Spawneo de nuevas notas para el loop actual.
        if (spawnIndex < timeStamps.Count)
        {
            if (SongManager.GetAudioSourceTime() >= timeStamps[spawnIndex] + SongManager.Instance.midiOutputDelay - SongManager.Instance.noteTime)
            {
                var noteObj = Instantiate(notePrefab, transform);
                var noteComp = noteObj.GetComponent<Note>();
                // Sumar el retraso al timestamp para que la detección se alinee
                noteComp.assignedTime = (float)(timeStamps[spawnIndex] + SongManager.Instance.midiOutputDelay);
                currentNotes.Add(noteComp);
                spawnIndex++;
            }
        }

        // Detección de input para las notas del loop actual.
        if (inputIndex < timeStamps.Count)
        {
            double timestamp = timeStamps[inputIndex];
            double marginOfError = SongManager.Instance.marginOfError;
            double audioTime = SongManager.GetAudioSourceTime() - (SongManager.Instance.inputDelayInMilliseconds / 1000.0);

            // Ajustamos el timestamp sumando el retraso
            double adjustedTimestamp = timestamp + SongManager.Instance.midiOutputDelay;

            if (Input.GetKeyDown(input))
            {
                if (Math.Abs(audioTime - adjustedTimestamp) < marginOfError)
                {
                    // Se llama al método Hit, que ejecuta la acción según el booleano configurado.
                    Hit();
                    Debug.Log($"Hit on {inputIndex} note");
                    if (inputIndex < currentNotes.Count && currentNotes[inputIndex] != null)
                    {
                        Destroy(currentNotes[inputIndex].gameObject);
                    }
                    inputIndex++; // Avanza solo si se acierta.
                }
                else
                {
                    Debug.Log($"Hit inaccurate on {inputIndex} note with {Math.Abs(audioTime - adjustedTimestamp)} delay");
                }
            }
            else if (adjustedTimestamp + marginOfError <= audioTime)
            {
                Miss();
                Debug.Log($"Missed {inputIndex} note");
                inputIndex++;
            }
        }
    }

    /// <summary>
    /// Reinicia los índices y la lista de notas para el loop actual.
    /// </summary>
    public void StartNewLoop()
    {
        spawnIndex = 0;
        inputIndex = 0;
        currentNotes = new List<Note>();
    }

    /// <summary>
    /// Se invoca al acertar una nota. Dependiendo de la configuración de la lane, 
    /// se hará que el dinosaurio del jugador ataque al enemigo (isDamageLane)
    /// o que el enemigo ataque al dinosaurio del jugador (enemyAttack).
    /// </summary>
    private void Hit()
    {
        ScoreManager.Hit();

        // El ataque del jugador solo se ejecuta si está en fase de ataque.
        if (isDamageLane && MeasureManager.Instance.IsAtaque)
        {
            // Lógica: el dinosaurio del jugador ataca al enemigo.
            Dinosaurio playerDino = FindObjectOfType<Dinosaurio>();
            GameObject enemyGO = GameObject.FindGameObjectWithTag("Enemigo");
            if (enemyGO != null)
            {
                EnemyDinosaur enemyDino = enemyGO.GetComponent<EnemyDinosaur>();
                if (playerDino != null && enemyDino != null)
                {
                    int damage = playerDino.DamageValue;
                    Debug.Log("El jugador ataca al enemigo y aplica " + damage + " de daño.");

                    // Activar la animación de ataque en el dinosaurio del jugador.
                    Animator playerAnim = playerDino.GetComponent<Animator>();
                    if (playerAnim != null)
                    {
                        playerAnim.SetTrigger("Attack");
                    }
                    enemyDino.ReceiveDamage(damage);
                }
                else
                {
                    Debug.LogWarning("No se encontró el dinosaurio del jugador o el componente EnemyDinosaur en el enemigo.");
                }
            }
            else
            {
                Debug.LogWarning("No se encontró ningún objeto con el tag 'Enemigo' en la escena.");
            }
        }
    }

    private void Miss()
    {
        ScoreManager.Miss();

        // El ataque del enemigo por fallo solo se ejecuta si está en fase de defensa.
        if (enemyAttack && MeasureManager.Instance.IsDefensa)
        {
            // Buscar al enemigo usando su tag "Enemigo"
            GameObject enemyGO = GameObject.FindGameObjectWithTag("Enemigo");
            Dinosaurio playerDino = FindObjectOfType<Dinosaurio>();

            if (enemyGO != null && playerDino != null)
            {
                EnemyDinosaur enemyDino = enemyGO.GetComponent<EnemyDinosaur>();
                if (enemyDino != null)
                {
                    int damage = enemyDino.DamageValue;
                    Debug.Log("El enemigo ataca al jugador y aplica " + damage + " de daño por fallo.");

                    // Activar la animación de ataque en el enemigo.
                    Animator enemyAnim = enemyGO.GetComponent<Animator>();
                    if (enemyAnim != null)
                    {
                        enemyAnim.SetTrigger("Attack");
                    }
                    else
                    {
                        Debug.LogWarning("No se encontró Animator en el enemigo.");
                    }

                    // Hacer que el dinosaurio del jugador reciba el daño.
                    playerDino.ReceiveDamage(damage);
                }
                else
                {
                    Debug.LogWarning("No se encontró el componente EnemyDinosaur en el objeto enemigo.");
                }
            }
            else
            {
                Debug.LogWarning("No se encontró el enemigo o el dinosaurio del jugador para ejecutar el ataque por fallo.");
            }
        }
    }
}