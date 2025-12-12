using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public Transform carAgent;      // Arrastra tu Taxi
    public Transform targetObject;  // Arrastra el objeto "Target" (La bola roja/cubo que persigue)
    public List<Transform> waypoints; // Arrastra aquí tus Puntos 0, 1, 2... en orden
    
    [Header("Configuración")]
    public float distanciaCambio = 3.0f; // Qué tan cerca debe estar para pasar al siguiente
    private int indiceActual = 0;

    void Start()
    {
        ReiniciarRuta();
    }

    void Update()
    {
        // Calculamos distancia entre el coche y el objetivo actual
        float distancia = Vector3.Distance(carAgent.position, targetObject.position);

        // Si el coche llega al objetivo actual (Zanahoria)
        if (distancia < distanciaCambio)
        {
            // Pasamos al siguiente punto
            indiceActual++;

            // Verificamos si quedan puntos en la lista
            if (indiceActual < waypoints.Count)
            {
                targetObject.position = waypoints[indiceActual].position;
            }
            // Si ya no quedan puntos, significa que está llegando a la Meta Final
            // El script CarAgent se encargará de reiniciar el episodio al tocar el trigger "Meta"
        }
    }

    // Esta función la llamaremos desde el Agente cuando choque o gane
    public void ReiniciarRuta()
    {
        indiceActual = 0;
        if (waypoints.Count > 0)
        {
            targetObject.position = waypoints[0].position;
        }
    }
}