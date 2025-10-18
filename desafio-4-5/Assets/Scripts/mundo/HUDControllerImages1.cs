// HUDControllerImages.cs
using UnityEngine;
using UnityEngine.UI;

public class HUDControllerImages : MonoBehaviour
{
    [SerializeField] private Jugador jugador;
    [Tooltip("Imágenes que representan cada punto/vida. Orden: 0 = vida 1, 1 = vida 2, ...")]
    [SerializeField] private Image[] vidaImages;

    private void OnEnable()
    {
        if (jugador != null && jugador.OnLivesChanged != null)
            jugador.OnLivesChanged.AddListener(UpdateImages);
    }

    private void OnDisable()
    {
        if (jugador != null && jugador.OnLivesChanged != null)
            jugador.OnLivesChanged.RemoveListener(UpdateImages);
    }

    private void Start()
    {
        if (jugador != null)
            UpdateImages(jugador.Lives); // inicializa estado
    }

    // Recibe cantidad de vidas (entero) y muestra/oculta imágenes
    public void UpdateImages(int lives)
    {
        // Clamp por seguridad
        int clamped = Mathf.Clamp(lives, 0, vidaImages.Length);

        for (int i = 0; i < vidaImages.Length; i++)
        {
            if (vidaImages[i] == null) continue;
            // Activo si el índice es menor que las vidas actuales
            vidaImages[i].gameObject.SetActive(i < clamped);
        }
    }
}
