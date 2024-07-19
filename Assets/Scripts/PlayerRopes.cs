using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRopes : MonoBehaviour
{
    public Transform player1;   // El primer jugador
    public Transform player2;   // El segundo jugador
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;  // Dos puntos: inicio y fin de la cuerda
    }

    void Update()
    {
        if (player1 != null && player2 != null)
        {
            // Actualiza los puntos del LineRenderer
            lineRenderer.SetPosition(0, player1.position);
            lineRenderer.SetPosition(1, player2.position);
        }
    }
}
