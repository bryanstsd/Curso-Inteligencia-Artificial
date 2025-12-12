using UnityEngine;

public class SimpleCarController : MonoBehaviour
{
    [Header("Configuración del Vehículo")]
    public float velocidad = 20f;      // Potencia de aceleración
    public float velocidadGiro = 100f; // Qué tan rápido gira
    public float velocidadRetroceso = 10f; // Velocidad marcha atrás

    private Rigidbody rb;
    private float inputVertical;
    private float inputHorizontal;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Capturamos las teclas (WASD o Flechas)
        inputVertical = Input.GetAxis("Vertical");
        inputHorizontal = Input.GetAxis("Horizontal");
    }

    void FixedUpdate()
    {
        MoverCoche();
        GirarCoche();
    }

    void MoverCoche()
    {
        // Determinamos si va hacia adelante o atrás para ajustar la velocidad
        float velocidadActual = (inputVertical > 0) ? velocidad : velocidadRetroceso;

        // Si estamos acelerando o frenando
        if (inputVertical != 0)
        {
            // Aplicamos fuerza relativa (hacia donde mira el coche)
            // Multiplicamos por 100 para usar números más cómodos en el inspector
            Vector3 fuerza = transform.forward * inputVertical * velocidadActual * 100f;
            rb.AddForce(fuerza * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    void GirarCoche()
    {
        // Solo giramos si el coche se está moviendo (opcional, pero más realista)
        if (inputHorizontal != 0 && rb.linearVelocity.magnitude > 1f)
        {
            // Calculamos la rotación en el eje Y (arriba)
            // Invertimos el giro si vamos marcha atrás para que sea natural (-1 si vamos atrás)
            float direccionMarcha = inputVertical >= 0 ? 1 : -1;
            
            float giro = inputHorizontal * velocidadGiro * direccionMarcha * Time.fixedDeltaTime;
            Quaternion rotacionActual = Quaternion.Euler(0f, giro, 0f);
            
            rb.MoveRotation(rb.rotation * rotacionActual);
        }
    }
}