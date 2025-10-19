using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Nuevo sistema de input

public class Mover : MonoBehaviour
{
    [Header("Configuracion Movimiento")]
    [SerializeField] float velocidad = 5f;
    [Tooltip("Umbral mínimo de velocidad para considerar que el jugador se mueve (para la animación).")]
    [SerializeField] float umbralMovimiento = 0.1f;

    [Header("Detección Piso")]
    [SerializeField] LayerMask maskPisos; // Asigna las capas de suelo en el inspector
    [SerializeField] Collider2D detectorSuelo; // Collider usado para comprobar contacto (ej. CircleCollider2D o BoxCollider2D)

    // Componentes
    private Rigidbody2D miRigidbody2D;
    private Animator miAnimator;
    private SpriteRenderer miSprite;

    // Control interno
    private Vector2 direccion;

    private void OnEnable()
    {
        miRigidbody2D = GetComponent<Rigidbody2D>();
        miAnimator = GetComponent<Animator>();
        miSprite = GetComponent<SpriteRenderer>();

        // Si no asignaste un detector desde el Inspector, intenta obtener un Collider2D del mismo objeto
        if (detectorSuelo == null)
        {
            detectorSuelo = GetComponent<Collider2D>();
        }
    }

    private void Update()
    {
        // Leer el input del teclado usando el nuevo sistema
        float moverHorizontal = Keyboard.current.aKey.isPressed ? -1f :
                                Keyboard.current.dKey.isPressed ? 1f : 0f;

        direccion = new Vector2(moverHorizontal, 0f);

        // Flip del sprite: si el jugador presiona izquierda/derecha, giramos.
        if (moverHorizontal != 0f)
        {
            miSprite.flipX = moverHorizontal < 0f;
        }
        else
        {
            // Si no está presionando, mantenemos la orientación según la velocidad real (opcional)
            float vx = miRigidbody2D.velocity.x;
            if (Mathf.Abs(vx) > umbralMovimiento) miSprite.flipX = vx < 0f;
        }

        // Actualizar parámetros del Animator (velocidad y enAire)
        int velocidadAnim = Mathf.Abs(miRigidbody2D.velocity.x) > umbralMovimiento ? 1 : 0;
        miAnimator.SetInteger("Velocidad", velocidadAnim);

        bool enAire = !EstaEnPiso();
        miAnimator.SetBool("EnAire", enAire);
    }

    private void FixedUpdate()
    {
        // Aplicar fuerza para mover
        miRigidbody2D.AddForce(direccion * velocidad, ForceMode2D.Force);

        // (Opcional) limitar velocidad máxima horizontal para evitar aceleración infinita
        float maxVelX = 7f;
        Vector2 v = miRigidbody2D.velocity;
        v.x = Mathf.Clamp(v.x, -maxVelX, maxVelX);
        miRigidbody2D.velocity = new Vector2(v.x, v.y);
    }

    private bool EstaEnPiso()
    {
        if (detectorSuelo == null) return false;
        // Usa IsTouchingLayers para saber si el collider está tocando las capas del suelo
        return detectorSuelo.IsTouchingLayers(maskPisos);
    }
}
