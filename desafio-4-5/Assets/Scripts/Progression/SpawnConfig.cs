using UnityEngine;

[CreateAssetMenu(menuName = "Spawning/SpawnConfig", fileName = "SpawnConfig_")]
public class SpawnConfig : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id = "entry";
        public GameObject prefab;
        public float initialDelay = 0f;
        [Tooltip("Intervalo aleatorio entre spawns (min,max)")]
        public float minInterval = 1f;
        public float maxInterval = 2f;
        [Tooltip("Pool size si usePool = true")]
        public int poolSize = 8;
        [Tooltip("Probabilidad relativa para selección aleatoria")]
        public float weight = 1f;
        public Vector2 spawnOffset = Vector2.zero; // offset relativo al spawner
        public bool usePool = true;
        public bool spawnOnStart = true;
    }

    public Entry[] entries;
    public bool loopForever = true; // si false, el spawner terminará tras un ciclo (opcional)
}
