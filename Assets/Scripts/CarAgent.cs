using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CarAgent : Agent
{
    [Header("Referencias")]
    public TrackManager trackManager; 

    [Header("Configuración Física")]
    public float velocidad = 15f; 
    public float velocidadGiro = 120f; 
    private Rigidbody rb;

    [Header("Entrenamiento")]
    public Transform spawnPoint; 
    public Transform target;     

    [Header("Sistema de Recompensas")]
    public float castigoChoque = -5.0f;
    public float premioMeta = 5.0f;
    public float castigoCarril = -0.002f;

    // VARIABLE NUEVA: Para medir la paciencia
    private float tiempoAtascado = 0f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        
        // Reseteamos el contador de paciencia al nacer
        tiempoAtascado = 0f;

        if (trackManager != null)
        {
            trackManager.ReiniciarRuta();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dirToTarget = (target.position - transform.position).normalized;

        // 1. Orientación (Adelante/Atrás)
        sensor.AddObservation(Vector3.Dot(transform.forward, dirToTarget));

        // 2. Orientación (Derecha/Izquierda)
        sensor.AddObservation(Vector3.Dot(transform.right, dirToTarget));
        
        // 3. Velocidad
        sensor.AddObservation(rb.linearVelocity.magnitude / 20f); 

        // 4. Distancia
        float distancia = Vector3.Distance(transform.position, target.position);
        sensor.AddObservation(distancia / 50f); 
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = actions.ContinuousActions[1]; 
        float turnInput = actions.ContinuousActions[0]; 

        // --- MOVIMIENTO ---
        if (moveInput > 0)
        {
            Vector3 fuerza = transform.forward * moveInput * velocidad * 100f;
            rb.AddForce(fuerza * Time.fixedDeltaTime, ForceMode.Acceleration);
        }

        if (rb.linearVelocity.magnitude > 0.5f)
        {
            float giro = turnInput * velocidadGiro * Time.fixedDeltaTime;
            float direccionMarcha = Vector3.Dot(rb.linearVelocity, transform.forward) >= 0 ? 1 : -1;
            Quaternion rotacionActual = Quaternion.Euler(0f, giro * direccionMarcha, 0f);
            rb.MoveRotation(rb.rotation * rotacionActual);
        }

        // --- LÓGICA DE PACIENCIA (ANTI-ATASCO) ---
        
        bool veoObstaculo = false;
        RaycastHit hit;

        // 1. Detectar si hay un OBSTÁCULO enfrente (12 metros)
        if (Physics.Raycast(transform.position, transform.forward, out hit, 12f))
        {
            if (hit.collider.CompareTag("Obstaculo"))
            {
                veoObstaculo = true;
            }
        }

        // 2. Si veo obstáculo Y estoy casi parado (< 2.0 velocidad)
        if (veoObstaculo && rb.linearVelocity.magnitude < 2.0f)
        {
            tiempoAtascado += Time.fixedDeltaTime; // Sube el cronómetro
            
            // Castigo progresivo por ansiedad (-0.05 por frame duele)
            AddReward(-0.05f); 

            // 3. ¿Se acabó el tiempo? (3 segundos límite)
            if (tiempoAtascado > 3.0f)
            {
                AddReward(-10.0f); // CASTIGO FINAL MASIVO por rendirse
                EndEpisode();      // Matamos al agente
                return;            // Salimos para no procesar nada más
            }
        }
        else
        {
            // Si nos movemos o ya no hay obstáculo, reseteamos
            tiempoAtascado = 0f;
            AddReward(-0.0005f); // Castigo normal por tiempo
        }

        // --- OTRAS RECOMPENSAS ---

        // Impuesto al Volante (Estabilidad)
        float penalizacionGiro = Mathf.Abs(turnInput) * 0.005f; 
        AddReward(-penalizacionGiro);

        // Incentivos de Avance
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        float alineacion = Vector3.Dot(transform.forward, dirToTarget);
        if (alineacion > 0) AddReward(0.005f * alineacion);

        float velocidadHaciaMeta = Vector3.Dot(rb.linearVelocity, dirToTarget);
        if (velocidadHaciaMeta > 0.1f) AddReward(0.002f * velocidadHaciaMeta);
    }

    // --- LÓGICA DE CARRILES ---
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("CarrilIzquierdo"))
        {
            float direccion = Vector3.Dot(transform.forward, other.transform.forward);
            // Tolerancia de 0.5f para permitir cruces
            if (direccion > 0.5f) 
            {
                AddReward(castigoCarril); 
            }
        }
    }

    // --- COLISIONES ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Meta"))
        {
            AddReward(premioMeta);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Muro") || collision.gameObject.CompareTag("Obstaculo"))
        {
            AddReward(castigoChoque);
            EndEpisode();
        }
    }

    // --- DEBUG VISUAL ---
    private void OnDrawGizmos()
    {
        // Rayo Rojo para ver si detecta el obstáculo
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 12f);

        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}