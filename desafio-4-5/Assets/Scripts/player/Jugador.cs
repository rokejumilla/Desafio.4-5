// Jugador.cs (igual que el tuyo, con estas adiciones)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events; // <-- Añadido

public class Jugador : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float vida = 5f;

    // --- NUEVO: evento público para notificar cambios de vida (entero)
    public UnityEvent<int> OnLivesChanged;
    // --- NUEVO: propiedad pública para leer vidas como entero
    public int Lives => Mathf.Clamp(Mathf.RoundToInt(vida), 0, int.MaxValue);

    [Header("UI Textos (TextMeshProUGUI)")]
    [SerializeField] private TextMeshProUGUI metaText;     // Texto que aparece al ganar
    [SerializeField] private string mensajeMeta = "¡GANASTE!";

    [SerializeField] private TextMeshProUGUI derrotaText;  // Texto que aparece al perder
    [SerializeField] private string mensajeDerrota = "GAME OVER";

    [Header("Opcional: scripts a desactivar")]
    [Tooltip("Arrastra aquí los scripts (ej. PlayerController) que quieras desactivar al congelar el juego.")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableOnFreeze;

    private bool juegoCongelado = false;

    // Guardamos valores anteriores para restaurar si es necesario
    private float previousTimeScale = 1f;
    private float previousFixedDeltaTime = 0.02f;
    private bool previousAudioPause = false;

    private void Awake()
    {
        // Aseguramos que los textos estén ocultos al inicio
        if (metaText != null) metaText.gameObject.SetActive(false);
        if (derrotaText != null) derrotaText.gameObject.SetActive(false);

        previousFixedDeltaTime = Time.fixedDeltaTime;
        previousTimeScale = Time.timeScale;

        // --- NUEVO: inicializar evento si es null y emitir valor inicial
        if (OnLivesChanged == null) OnLivesChanged = new UnityEvent<int>();
        OnLivesChanged.Invoke(Lives);
    }

    // Modifica la vida del jugador (puede ser daño o curación)
    public void ModificarVida(float puntos)
    {
        vida += puntos;
        Debug.Log($"Vida actual: {vida}. ¿Está vivo? {EstasVivo()}");

        // --- NUEVO: notificar a listeners con el valor entero de vidas
        OnLivesChanged.Invoke(Lives);

        if (!EstasVivo())
        {
            Perder();
        }
    }

    // Devuelve si el jugador sigue vivo
    private bool EstasVivo()
    {
        return vida > 0;
    }

    // Detecta si el jugador entra en un trigger con el tag "Meta"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Meta")) return;

        Ganar();
    }

    // Rutina al llegar a la meta
    private void Ganar()
    {
        if (juegoCongelado) return;

        MostrarTexto(metaText, mensajeMeta);
        FreezeGame();
        Debug.Log("GANASTE");

        // Llamada agregada para avanzar de nivel si existe el ProgressionManager
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.NextLevel();
    }

    // Rutina cuando la vida llega a 0
    private void Perder()
    {
        if (juegoCongelado) return;

        MostrarTexto(derrotaText, mensajeDerrota);
        FreezeGame();
        Debug.Log("GAME OVER");
    }

    // Muestra un texto TMP
    private void MostrarTexto(TextMeshProUGUI textoUI, string mensaje)
    {
        if (textoUI == null)
        {
            Debug.LogWarning("Jugador: falta asignar un TextMeshProUGUI en el Inspector.");
            return;
        }

        textoUI.text = mensaje;
        textoUI.gameObject.SetActive(true);
    }

    // Congela el juego
    private void FreezeGame()
    {
        // Guardar estado actual
        previousTimeScale = Time.timeScale;
        previousFixedDeltaTime = Time.fixedDeltaTime;
        previousAudioPause = AudioListener.pause;

        // Congelar tiempo y física
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        // Pausar audio
        AudioListener.pause = true;

        // Desactivar scripts opcionales
        if (scriptsToDisableOnFreeze != null)
        {
            foreach (var mb in scriptsToDisableOnFreeze)
            {
                if (mb != null) mb.enabled = false;
            }
        }

        juegoCongelado = true;
    }

    // (Opcional) descongelar el juego
    public void UnfreezeGame()
    {
        if (!juegoCongelado) return;

        // Restaurar tiempo y físicas
        Time.timeScale = previousTimeScale;
        Time.fixedDeltaTime = previousFixedDeltaTime;
        AudioListener.pause = previousAudioPause;

        // Reactivar scripts
        if (scriptsToDisableOnFreeze != null)
        {
            foreach (var mb in scriptsToDisableOnFreeze)
            {
                if (mb != null) mb.enabled = true;
            }
        }

        // Ocultar textos
        if (metaText != null) metaText.gameObject.SetActive(false);
        if (derrotaText != null) derrotaText.gameObject.SetActive(false);

        juegoCongelado = false;
    }
}
