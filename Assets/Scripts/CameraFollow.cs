using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Aquí arrastraremos el Taxi
    public Vector3 offset;   // Distancia entre cámara y coche
    public float smoothSpeed = 5f; // Qué tan rápido sigue al coche (más alto = más rígido)
    public float rotationSpeed = 10f; // Qué tan rápido gira la cámara

    void Start()
    {
        // Si no definimos un offset manual, calculamos la distancia actual al empezar
        if (offset == Vector3.zero)
        {
            // Calculamos la diferencia relativa local
            offset = target.InverseTransformPoint(transform.position);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Calcular dónde debería estar la cámara (detrás del coche)
        // TransformPoint convierte la posición local (offset) a posición real en el mundo
        Vector3 desiredPosition = target.TransformPoint(offset);

        // 2. Mover la cámara suavemente hacia esa posición (Lerp)
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // 3. Hacer que la cámara mire al coche suavemente
        var direction = target.position - transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}