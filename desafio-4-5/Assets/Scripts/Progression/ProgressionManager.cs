using UnityEngine;
using System;
using System.Collections.Generic;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }

    [Header("Datos")]
    public ProgressionData progressionData;
    public List<LevelData> fallbackLevels = new List<LevelData>();

    [Header("Runtime")]
    public Transform levelContainer;
    public string playerPrefsKey = "CurrentLevelIndex";

    [SerializeField] private int currentLevelIndex = 0;
    private GameObject currentLevelHolder;

    public event Action<LevelData, int> OnLevelLoaded;
    public event Action<LevelData, int> OnLevelUnloaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        currentLevelIndex = PlayerPrefs.GetInt(playerPrefsKey, 0);
        LoadCurrentLevel();
    }

    List<LevelData> GetLevelList()
    {
        if (progressionData != null && progressionData.levels != null && progressionData.levels.Count > 0)
            return progressionData.levels;
        return fallbackLevels;
    }

    public void LoadCurrentLevel() => LoadLevel(currentLevelIndex);

    public void LoadLevel(int index)
    {
        var levels = GetLevelList();
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("[ProgressionManager] No hay niveles asignados.");
            return;
        }

        index = Mathf.Clamp(index, 0, levels.Count - 1);
        currentLevelIndex = index;
        SaveProgress();

        LevelData ld = levels[currentLevelIndex];

        // Unload previo
        if (currentLevelHolder != null)
        {
            OnLevelUnloaded?.Invoke(ld, currentLevelIndex);
            Destroy(currentLevelHolder);
            currentLevelHolder = null;
        }

        // Crear holder y spawnear prefabs
        currentLevelHolder = new GameObject($"Level_{currentLevelIndex}_{ld.levelName}");
        if (levelContainer != null) currentLevelHolder.transform.SetParent(levelContainer, false);

        if (ld.prefabsToSpawn != null)
        {
            for (int i = 0; i < ld.prefabsToSpawn.Length; i++)
            {
                var p = ld.prefabsToSpawn[i];
                if (p != null) Instantiate(p, currentLevelHolder.transform);
            }
        }

        // Música / timer
        if (ld.music != null)
        {
            var asrc = GetComponent<AudioSource>();
            if (asrc == null) asrc = gameObject.AddComponent<AudioSource>();
            asrc.clip = ld.music;
            asrc.loop = true;
            asrc.Play();
        }
        if (ld.timeLimit > 0f)
        {
            var timer = FindObjectOfType<GameTimer>();
            if (timer != null) { timer.SetTime(ld.timeLimit); timer.ResetTimer(); }
        }

        OnLevelLoaded?.Invoke(ld, currentLevelIndex);
        Debug.Log($"[ProgressionManager] Cargado nivel {currentLevelIndex} - {ld.levelName}");
    }

    public void NextLevel()
    {
        var levels = GetLevelList();
        if (levels == null || levels.Count == 0) return;
        int next = currentLevelIndex + 1;
        if (next >= levels.Count) { Debug.Log("[ProgressionManager] Llegaste al final de la colección."); return; }
        LoadLevel(next);
    }

    public void PrevLevel()
    {
        if (currentLevelIndex <= 0) return;
        LoadLevel(currentLevelIndex - 1);
    }

    public void RestartLevel() => LoadLevel(currentLevelIndex);

    public void SaveProgress()
    {
        PlayerPrefs.SetInt(playerPrefsKey, currentLevelIndex);
        PlayerPrefs.Save();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(playerPrefsKey);
        currentLevelIndex = 0;
        SaveProgress();
    }

    // API util
    public int GetCurrentLevelIndex() => currentLevelIndex;
    public int GetTotalLevels() => GetLevelList()?.Count ?? 0;
    public LevelData GetLevelData(int index) => GetLevelList() != null && index >= 0 && index < GetLevelList().Count ? GetLevelList()[index] : null;
}
