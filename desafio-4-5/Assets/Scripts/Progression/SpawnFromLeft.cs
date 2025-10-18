using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))] // opcional pero recomendado
public class SpawnFromLeft : MonoBehaviour
{
    [Header("Prefab & timing")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(0.05f)] private float spawnInterval = 2f;
    [SerializeField, Min(0f)] private float spawnOffset = 0.5f; // distancia fuera del borde (izquierda)

    [Header("Opciones")]
    [SerializeField] private bool useCoroutine = true;
    [SerializeField] private bool useRendererIfNoCollider = true; // si no hay collider, usar renderer para bounds

    private Collider2D col2D;
    private Renderer rend;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        if (col2D == null && useRendererIfNoCollider)
            rend = GetComponent<Renderer>();

        if (enemyPrefab == null)
            Debug.LogWarning("SpawnFromLeft: enemyPrefab no asignado.");
    }

    private void Start()
    {
        if (useCoroutine)
            StartCoroutine(SpawnLoop());
        else
            InvokeRepeating(nameof(SpawnOnce), 0.5f, spawnInterval);
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.1f);
        while (true)
        {
            SpawnOnce();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Llamable desde otros scripts para forzar spawn inmediato
    public void SpawnOnce()
    {
        if (enemyPrefab == null) return;

        // obtener bounds en espacio mundial
        Bounds bounds;
        if (col2D != null)
        {
            bounds = col2D.bounds;
        }
        else if (rend != null)
        {
            bounds = rend.bounds;
        }
        else
        {
            // Fallback: asumir un cuadrado de tamaño 1 centrado en transform
            Vector3 center = transform.position;
            bounds = new Bounds(center, Vector3.one);
            Debug.LogWarning("SpawnFromLeft: No Collider2D ni Renderer. Usando bounds 1x1 de fallback.");
        }

        Vector3 spawnPos = CalculateLeftSpawnPosition(bounds, spawnOffset);

        // instanciar enemigo (sin rotación)
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    // calcula la posición fuera del borde (siempre por la izquierda)
    private Vector3 CalculateLeftSpawnPosition(Bounds b, float offset)
    {
        float minX = b.min.x;
        float minY = b.min.y;
        float maxY = b.max.y;

        Vector3 pos;
        pos.x = minX - offset;
        pos.y = Random.Range(minY, maxY);
        pos.z = enemyPrefab != null ? enemyPrefab.transform.position.z : 0f; // conservar z del prefab

        return pos;
    }

    // Gizmos para visualizar bounds y un ejemplo del punto de spawn (solo izquierda)
    private void OnDrawGizmosSelected()
    {
        Collider2D c = GetComponent<Collider2D>();
        Renderer r = GetComponent<Renderer>();
        Bounds b;

        if (c != null) b = c.bounds;
        else if (r != null) b = r.bounds;
        else b = new Bounds(transform.position, Vector3.one);

        // dibujar rectángulo de bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(b.center, b.size);

        // dibujar punto de ejemplo en la izquierda (a distancia offset)
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(new Vector3(b.min.x - spawnOffset, Random.Range(b.min.y, b.max.y), 0), 0.08f);
    }
}
