using UnityEngine;
using TMPro; // o UnityEngine.UI si usas Text
// usando TMP en este ejemplo

public class HUDControllerAuto : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text livesText;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged.AddListener(UpdateLives);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnLivesChanged.RemoveListener(UpdateLives);
    }

    private void Start()
    {
        if (livesText != null && playerHealth != null)
            UpdateLives(playerHealth.Lives); // muestra valor inicial
    }

    public void UpdateLives(int lives)
    {
        if (livesText == null) return;
        livesText.text = $"Vidas: {lives}";
    }
}
