using System.Collections.Generic;
using UnityEngine;

// Plataforma móvil kinematic que transporta al jugador mientras está encima.
// Para sincronizar varias plataformas: mismo "period" y distinto "timeOffset".
[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Tooltip("Desplazamiento total (en mundo) respecto a la posición inicial")]
    public Vector3 moveOffset = new Vector3(6f, 0f, 0f);
    [Tooltip("Segundos que tarda en ir y volver")]
    public float period = 4f;
    [Tooltip("Desfase en segundos para sincronizar/alternar plataformas")]
    public float timeOffset = 0f;

    Rigidbody rb;
    Collider col;
    Vector3 startPos;
    readonly HashSet<Rigidbody> riders = new HashSet<Rigidbody>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.None;
        col = GetComponent<Collider>();
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        // Movimiento suave de ida y vuelta (coseno) basado en tiempo global => sincronizado
        float t = (Time.time + timeOffset) / Mathf.Max(0.01f, period);
        float k = 0.5f - 0.5f * Mathf.Cos(t * Mathf.PI * 2f);
        Vector3 newPos = startPos + moveOffset * k;

        Vector3 delta = newPos - rb.position;
        rb.MovePosition(newPos);

        foreach (Rigidbody rider in riders)
            if (rider != null) rider.position += delta;
    }

    void OnCollisionEnter(Collision collision)
    {
        TryAddRider(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        TryAddRider(collision);
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.rigidbody != null) riders.Remove(collision.rigidbody);
    }

    void TryAddRider(Collision collision)
    {
        Rigidbody other = collision.rigidbody;
        if (other == null || other.isKinematic) return;
        // Solo si el objeto está encima (no si choca de costado)
        if (collision.collider.bounds.min.y >= col.bounds.max.y - 0.2f)
            riders.Add(other);
        else
            riders.Remove(other);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 s = Application.isPlaying ? startPos : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(s, s + moveOffset);
        Gizmos.DrawWireCube(s + moveOffset, transform.localScale);
    }
}