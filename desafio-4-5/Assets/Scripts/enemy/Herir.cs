using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Herir : MonoBehaviour
{
    [Header("Configuración de puntos (máx 5)")]
    [SerializeField, Tooltip("Cantidad inicial de puntos (máx 1).")]
    private int puntosIniciales = 5;

    [Header("TextMeshPro que representan los puntos (0 = punto1 ... 4 = punto5)")]
    [SerializeField] private TextMeshProUGUI[] puntosUI = new TextMeshProUGUI[5];

    // Estado interno
    private int puntosRestantes;
    private int lastShownIndex = -1; // índice del slot actualmente visible (-1 = ninguno)

    private void Start()
    {
        // Clamp de seguridad
        puntosIniciales = Mathf.Clamp(puntosIniciales, 0, puntosUI.Length);
        puntosRestantes = puntosIniciales;

        // Inicializar textos por si no los pusiste en el inspector
        for (int i = 0; i < puntosUI.Length; i++)
        {
            if (puntosUI[i] != null && string.IsNullOrEmpty(puntosUI[i].text))
                puntosUI[i].text = (i + 1).ToString(); // garantiza que cada slot muestre su número correcto
        }

        // Oculta todas las UIs al inicio
        OcultarTodasLasUIs();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        // Intentar obtener el componente Jugador (si lo usas)
        Jugador jugador = collision.gameObject.GetComponent<Jugador>();
        if (jugador != null)
        {
            jugador.ModificarVida(-1f); // resta 1 vida
            Debug.Log("Se aplicó 1 punto de daño al jugador.");
        }
        else
        {
            Debug.LogWarning("El objeto con tag 'Player' no tiene el componente Jugador.");
        }

        // Mostrar el siguiente número (5, luego 4, ...)
        MostrarSiguienteNumero();
    }

    // Muestra el número correspondiente al punto que se acaba de quitar:
    // ej: si puntosRestantes == 5 -> muestra el slot índice 4 (número "5") y luego decrementa.
    // Además se asegura de ocultar el anterior mostrado (si existe).
    private void MostrarSiguienteNumero()
    {
        if (puntosRestantes <= 0)
        {
            Debug.Log("No quedan puntos para quitar/mostrar.");
            return;
        }

        int indexToShow = puntosRestantes - 1; // 5->4, 4->3, ...
        // Oculta el anterior mostrado (si lo hay y distinto al nuevo)
        if (lastShownIndex >= 0 && lastShownIndex < puntosUI.Length && lastShownIndex != indexToShow)
        {
            if (puntosUI[lastShownIndex] != null)
                puntosUI[lastShownIndex].gameObject.SetActive(false);
        }

        // Muestra el nuevo (si existe)
        if (indexToShow >= 0 && indexToShow < puntosUI.Length)
        {
            TextMeshProUGUI slot = puntosUI[indexToShow];
            if (slot != null)
            {
                // Aseguramos que el texto muestre el número correcto (opcional)
                slot.text = (indexToShow + 1).ToString();

                // Hacemos visible ese slot (y ocultamos cualquier otro por seguridad)
                slot.gameObject.SetActive(true);
                lastShownIndex = indexToShow;
            }
            else
            {
                Debug.LogWarning($"puntosUI[{indexToShow}] es null.");
            }
        }
        else
        {
            Debug.LogWarning("Índice a mostrar fuera de rango.");
        }

        puntosRestantes -= 1;

        // Si ya no quedan puntos (puntosRestantes == 0), ocultamos el último visible para que no exista "0".
        if (puntosRestantes == 0 && lastShownIndex >= 0 && lastShownIndex < puntosUI.Length)
        {
            // ocultar el último mostrado (opcional: si quieres que el "1" quede visible hasta que sea golpeado otra vez, 
            // elimina este bloque)
            puntosUI[lastShownIndex].gameObject.SetActive(false);
            lastShownIndex = -1;
        }

        Debug.Log($"Mostrado punto índice {indexToShow}. Puntos restantes ahora: {puntosRestantes}.");
    }

    private void OcultarTodasLasUIs()
    {
        for (int i = 0; i < puntosUI.Length; i++)
        {
            if (puntosUI[i] != null)
                puntosUI[i].gameObject.SetActive(false);
        }
        lastShownIndex = -1;
    }

    // Método público para reiniciar estado desde otro script o desde el Inspector (botón)
    public void ResetPuntos()
    {
        puntosRestantes = Mathf.Clamp(puntosIniciales, 0, puntosUI.Length);
        OcultarTodasLasUIs();
        Debug.Log("Puntos reiniciados y UIs ocultas.");
    }
}
