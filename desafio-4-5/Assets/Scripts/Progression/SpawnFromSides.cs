using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))] // opcional, pero recomendado
public class SpawnFromSides : MonoBehaviour
{
    [Header("Prefab & timing")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField, Min(0.05f)] private float spawnInterval = 2f;
    [SerializeField, Min(0f)] private float spawnOffset = 0.5f; // distancia fuera del borde

    [Header("Opciones")]
    [SerializeField] private bool useCoroutine = true;
    [SerializeField] private bool randomSideEachSpawn = true; // si false, se itera secuencialmente
    [SerializeField] private bool useRendererIfNoCollider = true;

    private Collider2D col2D;
    private Renderer rend;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        if (col2D == null && useRendererIfNoCollider)
            rend = GetComponent<Renderer>();

        if (enemyPrefab == null)
            Debug.LogWarning("SpawnFromSides: enemyPrefab no asignado.");
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
            Debug.LogWarning("SpawnFromSides: No Collider2D ni Renderer. Usando bounds 1x1 de fallback.");
        }

        // elegir lado
        int side = randomSideEachSpawn ? Random.Range(0, 4) : NextSide();
        Vector3 spawnPos = CalculateSpawnPosition(bounds, side, spawnOffset);

        // instanciar enemigo (sin rotación)
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    // calcula la posición fuera del borde según el lado
    private Vector3 CalculateSpawnPosition(Bounds b, int sideIndex, float offset)
    {
        float minX = b.min.x;
        float maxX = b.max.x;
        float minY = b.min.y;
        float maxY = b.max.y;

        Vector3 pos = Vector3.zero;

        switch (sideIndex)
        {
            case 0: // Arriba
                pos.x = Random.Range(minX, maxX);
                pos.y = maxY + offset;
                break;
            case 1: // Derecha
                pos.x = maxX + offset;
                pos.y = Random.Range(minY, maxY);
                break;
            case 2: // Abajo
                pos.x = Random.Range(minX, maxX);
                pos.y = minY - offset;
                break;
            case 3: // Izquierda
                pos.x = minX - offset;
                pos.y = Random.Range(minY, maxY);
                break;
            default:
                pos = b.center;
                break;
        }

        // conservar z del prefab (si usas 2D usualmente z = 0)
        pos.z = enemyPrefab.transform.position.z;
        return pos;
    }

    // si no querés totalmente aleatorio, este método itera lados 0..3
    private int sequentialSide = 0;
    private int NextSide()
    {
        int s = sequentialSide;
        sequentialSide = (sequentialSide + 1) % 4;
        return s;
    }

    // Gizmos para visualizar bounds y un ejemplo de puntos de spawn
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

        // dibujar 4 puntos de ejemplo (a distancia offset)
        Vector3 up = new Vector3(Random.value, 0.5f, 0); // no usado, solo para evitar warning
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(new Vector3(Random.Range(b.min.x, b.max.x), b.max.y + spawnOffset, 0), 0.08f);
        Gizmos.DrawSphere(new Vector3(b.max.x + spawnOffset, Random.Range(b.min.y, b.max.y), 0), 0.08f);
        Gizmos.DrawSphere(new Vector3(Random.Range(b.min.x, b.max.x), b.min.y - spawnOffset, 0), 0.08f);
        Gizmos.DrawSphere(new Vector3(b.min.x - spawnOffset, Random.Range(b.min.y, b.max.y), 0), 0.08f);
    }
}
