using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Nuevo sistema de input

public class Saltar : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private float fuerzaSalto = 5f;

    private bool puedoSaltar = true;
    private bool saltando = false;

    private Rigidbody2D miRigidbody2D;

    private void OnEnable()
    {
        miRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // En el nuevo sistema usamos Keyboard.current
        if (Keyboard.current.spaceKey.wasPressedThisFrame && puedoSaltar)
        {
            puedoSaltar = false;
        }
    }

    private void FixedUpdate()
    {
        if (!puedoSaltar && !saltando)
        {
            // Impulso vertical
            miRigidbody2D.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            saltando = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Al tocar el suelo, se puede volver a saltar
        puedoSaltar = true;
        saltando = false;
    }
}

