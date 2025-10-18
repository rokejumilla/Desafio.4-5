using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// GameTimer: cuenta regresiva. Cuando llega a 0 congela la escena (timeScale = 0),
/// pausa audio (opcional), detiene Rigidbody/Animator y muestra un mensaje TMP:
/// "TIEMPO AGOTADO\nPERDISTE!!!"
/// Para usarlo solo necesitas tener TextMeshPro en tu proyecto y un TextMeshProUGUI
/// para el temporizador (el script intenta obtenerlo del mismo GameObject si no se asigna).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class GameTimer : MonoBehaviour
{
    [Header("Ajustes del temporizador")]
    [Tooltip("Segundos iniciales del temporizador")]
    public float startTime = 30f;

    [Tooltip("TMP que mostrará el reloj. Si no se asigna, se busca en este GameObject.")]
    public TextMeshProUGUI timerText;

    [Header("Mensaje final (opcional)")]
    [Tooltip("TMP que mostrará el mensaje final. Si está vacío, el script lo creará en la Canvas.")]
    public TextMeshProUGUI messageText;

    [Tooltip("Texto que aparece cuando termina el tiempo")]
    [TextArea]
    public string endMessage = "TIEMPO AGOTADO\nPERDISTE!!!";

    [Tooltip("Pausar audio al terminar")]
    public bool pauseAudio = true;

    float timeRemaining;
    bool finished = false;

    // Para poder restaurar estados cuando se hace ResetTimer()
    struct Rigidbody2DState { public bool simulated; public Vector2 velocity; public float angularVelocity; }
    struct RigidbodyState { public bool isKinematic; public Vector3 velocity; }
    struct AnimatorState { public bool enabled; }

    private Dictionary<Rigidbody2D, Rigidbody2DState> r2dStates = new Dictionary<Rigidbody2D, Rigidbody2DState>();
    private Dictionary<Rigidbody, RigidbodyState> r3dStates = new Dictionary<Rigidbody, RigidbodyState>();
    private Dictionary<Animator, AnimatorState> animatorStates = new Dictionary<Animator, AnimatorState>();

    void Awake()
    {
        // Si no asignaste timerText, intenta obtener uno en este GameObject.
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();

        // Si no hay ninguno, ok — el Start intentará manejarlo (pero lo ideal es asignarlo).
    }

    void Start()
    {
        timeRemaining = Mathf.Max(0f, startTime);

        if (timerText == null)
        {
            Debug.LogWarning("[GameTimer] No hay TextMeshProUGUI asignado para mostrar el temporizador. Añade uno o asigna timerText en el inspector.");
        }
        else
        {
            UpdateTimerText();
        }

        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (finished) return;

        // Cuenta regresiva normal con Time.deltaTime (funciona solo mientras timeScale > 0)
        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText();
            FinishTimer();
        }
        else
        {
            UpdateTimerText();
        }
    }

    void UpdateTimerText()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void FinishTimer()
    {
        finished = true;

        // 1) Congelar el tiempo (detiene la mayoría de la física y lógica dependiente de deltaTime)
        Time.timeScale = 0f;

        // 2) Pausar audio si corresponde
        if (pauseAudio)
            AudioListener.pause = true;

        // 3) Detener Rigidbodies y física 2D/3D para "congelar" movimiento inmediato
        r2dStates.Clear();
        var r2ds = FindObjectsOfType<Rigidbody2D>();
        foreach (var r in r2ds)
        {
            // Guardar estado original
            Rigidbody2DState s = new Rigidbody2DState
            {
                simulated = r.simulated,
                velocity = r.linearVelocity,
                angularVelocity = r.angularVelocity
            };
            r2dStates[r] = s;

            // Detener
            r.linearVelocity = Vector2.zero;
            r.angularVelocity = 0f;
            r.simulated = false;
        }

        r3dStates.Clear();
        var r3ds = FindObjectsOfType<Rigidbody>();
        foreach (var r in r3ds)
        {
            // Guardar estado original
            RigidbodyState s = new RigidbodyState
            {
                isKinematic = r.isKinematic,
                velocity = r.linearVelocity
            };
            r3dStates[r] = s;

            // Detener (convertir a kinematic para detener efectos físicos)
            r.linearVelocity = Vector3.zero;
            r.isKinematic = true;
        }

        // 4) Pausar animadores (esto evita que las animaciones sigan en Update)
        animatorStates.Clear();
        var anims = FindObjectsOfType<Animator>();
        foreach (var a in anims)
        {
            AnimatorState s = new AnimatorState { enabled = a.enabled };
            animatorStates[a] = s;

            a.enabled = false;
        }

        // 5) Mostrar el mensaje final
        ShowEndMessage();
    }

    void ShowEndMessage()
    {
        if (messageText != null)
        {
            messageText.text = endMessage;
            messageText.gameObject.SetActive(true);
            return;
        }

        // Si no te dieron un messageText, creamos uno sencillo en la Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Crear canvas si no existe
            GameObject cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
        }

        GameObject go = new GameObject("GameTimer_Message");
        go.transform.SetParent(canvas.transform, false);

        TextMeshProUGUI msg = go.AddComponent<TextMeshProUGUI>();
        msg.alignment = TextAlignmentOptions.Center;
        msg.enableWordWrapping = true;
        msg.text = endMessage;
        msg.fontSize = 72; // puedes ajustar desde el inspector si prefieres crear messageText manualmente
        msg.raycastTarget = false;

        RectTransform rt = msg.rectTransform;
        rt.anchorMin = new Vector2(0.1f, 0.35f);
        rt.anchorMax = new Vector2(0.9f, 0.7f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // mantener referencia para posibles resets
        messageText = msg;
    }


    // Método público para forzar el fin desde otro script (útil para pruebas)
    public void ForceFinish()
    {
        if (!finished)
        {
            timeRemaining = 0f;
            FinishTimer();
        }
    }

    /// <summary>
    /// Ajusta el tiempo actual y el startTime base.
    /// No altera el estado de pausa/finish salvo actualizar el texto.
    /// </summary>
    public void SetTime(float t)
    {
        startTime = t;   // si usas startTime como base
        timeRemaining = t;
        UpdateTimerText();
    }

    /// <summary>
    /// Resetea el temporizador al startTime, rehace física/animaciones y UI.
    /// </summary>
    public void ResetTimer()
    {
        // Restaurar flags/estados
        // 1) Reactivar timeScale y audio
        Time.timeScale = 1f;
        if (pauseAudio)
            AudioListener.pause = false;

        // 2) Restaurar Rigidbody2D
        foreach (var kv in r2dStates)
        {
            var r = kv.Key;
            if (r == null) continue;
            var s = kv.Value;
            r.simulated = s.simulated;
            r.linearVelocity = s.velocity;
            r.angularVelocity = s.angularVelocity;
        }
        r2dStates.Clear();

        // 3) Restaurar Rigidbody 3D
        foreach (var kv in r3dStates)
        {
            var r = kv.Key;
            if (r == null) continue;
            var s = kv.Value;
            r.isKinematic = s.isKinematic;
            r.linearVelocity = s.velocity;
        }
        r3dStates.Clear();

        // 4) Restaurar animadores
        foreach (var kv in animatorStates)
        {
            var a = kv.Key;
            if (a == null) continue;
            var s = kv.Value;
            a.enabled = s.enabled;
        }
        animatorStates.Clear();

        // 5) Reiniciar bandera y tiempo
        finished = false;
        timeRemaining = Mathf.Max(0f, startTime);

        // 6) Ocultar mensaje final si existe
        if (messageText != null)
            messageText.gameObject.SetActive(false);

        // 7) Actualizar UI
        UpdateTimerText();
    }
}
