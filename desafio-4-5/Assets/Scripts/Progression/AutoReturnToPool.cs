// AutoReturnToPool.cs
using UnityEngine;

/// <summary>
/// Script para colocar en prefabs que vayan a volver automáticamente al pool:
/// - Setear originalPrefab en el inspector (el prefab "base" que se pasó al PoolManager).
/// - Ajustar lifetime y/o detectar colisión para devolver.
/// </summary>
public class AutoReturnToPool : MonoBehaviour
{
    [Tooltip("Referencia al prefab 'original' que se usó al crear el pool")]
    public GameObject originalPrefab;

    [Tooltip("Tiempo de vida en segundos tras activarse (0 = desactivado)")]
    public float lifeTime = 3f;

    private void OnEnable()
    {
        if (lifeTime > 0f) Invoke(nameof(Return), lifeTime);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    public void Return()
    {
        if (PoolManager.Instance != null && originalPrefab != null)
        {
            PoolManager.Instance.Return(originalPrefab, gameObject);
        }
        else
        {
            // fallback
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D _) => Return();
    private void OnTriggerEnter2D(Collider2D _) => Return();
}
