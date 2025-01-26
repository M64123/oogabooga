using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosQTE : MonoBehaviour
{
    public Transform originTransform; // Transform desde donde se dibujan las líneas
    public float[] distances = { 2f, 4f, 6f, 8f }; // Distancias de cada línea hacia la izquierda
    public Color[] gizmoColors = { Color.red, Color.green, Color.blue, Color.yellow }; // Colores de las líneas

    private void OnDrawGizmos()
    {
        // Verificamos que haya un origen asignado y que los arrays tengan datos
        if (originTransform != null && distances.Length == gizmoColors.Length)
        {
            for (int i = 0; i < distances.Length; i++)
            {
                // Establecer el color actual
                Gizmos.color = gizmoColors[i];

                // Calcular el punto final de la línea hacia la izquierda
                Vector3 start = originTransform.position;
                Vector3 end = start + Vector3.left * distances[i];

                // Dibujar la línea
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
