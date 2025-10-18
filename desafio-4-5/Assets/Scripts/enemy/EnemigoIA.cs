using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemigoIA : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 2f;
    [SerializeField] private float velocidadAlVer = 3f;
    [SerializeField] private bool usarVelocidadAlVer = false;

    [Header("Detección")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float rangoVision = 6f;
    [SerializeField] private LayerMask capasObstaculos;
    [SerializeField] private LayerMask capaJugador;

    [Header("Restricciones verticales")]
    [SerializeField] private float toleranciaY = 1.0f;

    [Header("Salto pequeño")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private float impulsoSaltoPequeño = 3f;
    [SerializeField] private float tiempoEntreSaltos = 1.5f;

    private Rigidbody2D rb;
    private float siguienteSaltoTime = 0f;
    private bool mirandoDerecha = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (eyePoint == null) eyePoint = transform;
    }

    private void FixedUpdate()
    {
        if (jugador == null) return;

        bool jugadorEnVista = RevisarVisionJugador();

        if (jugadorEnVista)
        {
            float dirX = Mathf.Sign(jugador.position.x - transform.position.x);
            float velocidadActual = usarVelocidadAlVer ? velocidadAlVer : velocidad;
            rb.linearVelocity = new Vector2(dirX * velocidadActual, rb.linearVelocity.y);

            if (dirX > 0 && !mirandoDerecha) Flip();
            if (dirX < 0 && mirandoDerecha) Flip();
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        bool enSuelo = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (enSuelo && Time.time >= siguienteSaltoTime)
        {
            rb.AddForce(Vector2.up * impulsoSaltoPequeño, ForceMode2D.Impulse);
            siguienteSaltoTime = Time.time + tiempoEntreSaltos;
        }
    }

    private bool RevisarVisionJugador()
    {
        Vector2 toPlayer = jugador.position - transform.position;

        if (Mathf.Abs(toPlayer.y) > toleranciaY) return false;

        float distanciaX = Mathf.Abs(toPlayer.x);
        if (distanciaX > rangoVision) return false;

        Vector2 origen = eyePoint.position;
        Vector2 direccion = ((Vector2)jugador.position - origen).normalized;

        // Realizamos un raycast que ignore triggers
        RaycastHit2D hit = Physics2D.Raycast(origen, direccion, rangoVision, ~0);
        if (hit.collider != null)
        {
            if (hit.collider.gameObject == jugador.gameObject) return true;
            // Si el primer hit no es el jugador, entonces hay un obstáculo entre ambos
            return false;
        }

        return false;
    }

    private void Flip()
    {
        mirandoDerecha = !mirandoDerecha;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(eyePoint.position, eyePoint.position + (Vector3.right * rangoVision * (transform.localScale.x > 0 ? 1f : -1f)));
            Gizmos.DrawWireSphere(eyePoint.position, 0.05f);
        }
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
