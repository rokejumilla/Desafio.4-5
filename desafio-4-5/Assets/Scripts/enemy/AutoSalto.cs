using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSalto : MonoBehaviour
{
    [Header("Configuración AutoSalto")]
    [SerializeField] private float fuerzaSalto = 2.5f;
    [SerializeField] private LayerMask groundLayer; // asignar la capa de suelo en inspector
    [SerializeField] private bool soloAlTocarSuelo = true; // si true, solo salta al tocar suelo

    private Rigidbody2D miRigidbody2D;

    private void OnEnable()
    {
        miRigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (soloAlTocarSuelo)
        {
            // comprobamos si la colisión fue con el suelo (por la capa)
            if (((1 << collision.gameObject.layer) & groundLayer) == 0)
                return;
        }

        // Aplicar un impulso pequeño hacia arriba
        miRigidbody2D.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
    }
}
