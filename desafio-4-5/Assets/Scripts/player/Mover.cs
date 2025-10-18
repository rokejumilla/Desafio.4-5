using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Nuevo sistema de input

public class Mover : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] float velocidad = 5f;

    private Vector2 direccion;
    private Rigidbody2D miRigidbody2D;

    private void OnEnable()
    {
        miRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Leer el input del teclado usando el nuevo sistema
        float moverHorizontal = Keyboard.current.aKey.isPressed ? -1 :
                                Keyboard.current.dKey.isPressed ? 1 : 0;

        direccion = new Vector2(moverHorizontal, 0f);
    }

    private void FixedUpdate()
    {
        miRigidbody2D.AddForce(direccion * velocidad);
    }
}
