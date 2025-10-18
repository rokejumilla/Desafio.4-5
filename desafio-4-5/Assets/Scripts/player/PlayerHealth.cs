using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private int lives = 5;

    // Event que notifica la cantidad de vidas (int)
    public UnityEvent<int> OnLivesChanged;

    // Propiedad pública para que otros scripts (HUD) puedan leer las vidas
    public int Lives => lives;

    private void Awake()
    {
        // Aseguramos que el evento no sea null (esto evita NullReferenceException al invocar)
        if (OnLivesChanged == null) OnLivesChanged = new UnityEvent<int>();
    }

    private void Start()
    {
        // Emitimos el estado inicial para que el HUD muestre el valor al comenzar la escena
        OnLivesChanged.Invoke(lives);
    }

    public void LoseLife()
    {
        lives--;
        OnLivesChanged.Invoke(lives);

        if (lives <= 0)
        {
            // Aquí puedes manejar la situación de Game Over
            Debug.Log("Game Over");
        }
    }
}
