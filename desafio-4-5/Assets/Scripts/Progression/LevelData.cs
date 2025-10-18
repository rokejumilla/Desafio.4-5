using UnityEngine;

[CreateAssetMenu(menuName = "Progression/LevelData", fileName = "LevelData_")]
public class LevelData : ScriptableObject
{
    public string levelName = "Nivel 1";
    public AudioClip music;
    public float timeLimit = 0f;       // 0 = no limit
    public GameObject[] prefabsToSpawn;
    // Agrega aquí más datos editables (ambient, tilemap reference, difficulty, etc.)
}
