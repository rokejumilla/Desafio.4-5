// BackgroundInvokeToggle.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal; // solo necesario si usás Light2D

[DisallowMultipleComponent]
public class BackgroundInvokeToggle : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera targetCamera;
    [Tooltip("Global Light 2D (opcional)")]
    [SerializeField] private Light2D globalLight2D;

    [Header("Colores")]
    [SerializeField] private Color dayColor = new Color(0.48f, 0.78f, 1f);
    [SerializeField] private Color nightColor = new Color(0.05f, 0.05f, 0.15f);

    [Header("Invoke settings")]
    [SerializeField, Min(0f)] private float startDelay = 0f;
    [SerializeField, Min(0.01f)] private float repeatInterval = 10f;

    [Header("Opcional - transición suave")]
    [SerializeField] private bool smoothTransition = false;
    [SerializeField, Min(0.01f)] private float transitionDuration = 1.5f;
    [SerializeField] private bool smoothStep = true; // usa SmoothStep para suavizar t

    private Coroutine transitionCoroutine;

    private void Reset()
    {
        // valores por defecto amigables
        startDelay = 0f;
        repeatInterval = 10f;
        transitionDuration = 1.5f;
        dayColor = new Color(0.48f, 0.78f, 1f);
        nightColor = new Color(0.05f, 0.05f, 0.15f);
    }

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera != null) targetCamera.backgroundColor = dayColor;
        if (globalLight2D != null) globalLight2D.color = dayColor;

        // arranca la repetición (InvokeRepeating)
        InvokeRepeating(nameof(ToggleDayNight), startDelay, repeatInterval);
    }

    private void ToggleDayNight()
    {
        if (targetCamera == null) return;

        // decidir color destino
        Color current = targetCamera.backgroundColor;
        Color target = (ApproximatelyEqual(current, dayColor)) ? nightColor : dayColor;

        if (smoothTransition)
        {
            // si hay otra transición en curso, la paramos
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionTo(target, transitionDuration));
        }
        else
        {
            // cambio instantáneo
            targetCamera.backgroundColor = target;
            if (globalLight2D != null) globalLight2D.color = target;
        }
    }

    private IEnumerator TransitionTo(Color target, float duration)
    {
        if (targetCamera == null)
        {
            transitionCoroutine = null;
            yield break;
        }

        Color fromCam = targetCamera.backgroundColor;
        Color fromLight = globalLight2D != null ? globalLight2D.color : Color.white;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (smoothStep) t = Mathf.SmoothStep(0f, 1f, t);

            Color c = Color.Lerp(fromCam, target, t);
            targetCamera.backgroundColor = c;

            if (globalLight2D != null)
                globalLight2D.color = Color.Lerp(fromLight, target, t);

            yield return null;
        }

        // asegurar valor final
        targetCamera.backgroundColor = target;
        if (globalLight2D != null) globalLight2D.color = target;

        transitionCoroutine = null;
    }

    private bool ApproximatelyEqual(Color a, Color b, float eps = 0.01f)
    {
        return Mathf.Abs(a.r - b.r) <= eps
            && Mathf.Abs(a.g - b.g) <= eps
            && Mathf.Abs(a.b - b.b) <= eps
            && Mathf.Abs(a.a - b.a) <= eps;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ToggleDayNight));
        if (transitionCoroutine != null) { StopCoroutine(transitionCoroutine); transitionCoroutine = null; }
    }

    // Métodos públicos para controlar en runtime
    public void ForceDay()
    {
        if (transitionCoroutine != null) { StopCoroutine(transitionCoroutine); transitionCoroutine = null; }
        if (targetCamera) targetCamera.backgroundColor = dayColor;
        if (globalLight2D != null) globalLight2D.color = dayColor;
    }

    public void ForceNight()
    {
        if (transitionCoroutine != null) { StopCoroutine(transitionCoroutine); transitionCoroutine = null; }
        if (targetCamera) targetCamera.backgroundColor = nightColor;
        if (globalLight2D != null) globalLight2D.color = nightColor;
    }

    public void SetRepeatInterval(float newInterval)
    {
        // reinicia invoke con nuevo intervalo conservando el mismo startDelay
        CancelInvoke(nameof(ToggleDayNight));
        repeatInterval = Mathf.Max(0.01f, newInterval);
        InvokeRepeating(nameof(ToggleDayNight), 0f, repeatInterval);
    }
}
