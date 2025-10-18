using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    public int value = 1;
    public AudioClip pickSfx;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(value);

        if (pickSfx != null)
            AudioSource.PlayClipAtPoint(pickSfx, transform.position);

        Destroy(gameObject);
    }
}
