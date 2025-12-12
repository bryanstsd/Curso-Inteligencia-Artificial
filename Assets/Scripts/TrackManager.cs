using UnityEngine;
using System.Collections.Generic;

public class TrackManager : MonoBehaviour
{
    public Transform carAgent;      
    public Transform targetObject;  
    public List<Transform> waypoints; 
    
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

        if (distancia < distanciaCambio)
        {
            indiceActual++;

    
            if (indiceActual < waypoints.Count)
            {
                targetObject.position = waypoints[indiceActual].position;
            }
            
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
