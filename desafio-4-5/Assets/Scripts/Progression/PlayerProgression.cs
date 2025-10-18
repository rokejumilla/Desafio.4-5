using UnityEngine;
using System;

public class PlayerProgression : MonoBehaviour
{
    public PlayerProgressionData progressionData;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnExperienceChanged;

    public int Level => progressionData.currentLevel;
    public int Experience => progressionData.currentExperience;
    public int ExperienceToNext => progressionData.experienceToNextLevel;

    public void GainExperience(int amount)
    {
        if (amount <= 0) return;
        progressionData.currentExperience += amount;
        OnExperienceChanged?.Invoke(progressionData.currentExperience);

        while (progressionData.currentExperience >= progressionData.experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        progressionData.currentLevel++;
        progressionData.currentExperience -= progressionData.experienceToNextLevel;
        progressionData.experienceToNextLevel = Mathf.CeilToInt(progressionData.experienceToNextLevel * progressionData.expScale);

        OnLevelChanged?.Invoke(progressionData.currentLevel);
        OnExperienceChanged?.Invoke(progressionData.currentExperience);

        // Aquí añadir mejoras desbloqueadas, animaciones, SFX, etc.
        Debug.Log($"Level up! Nuevo nivel: {progressionData.currentLevel}");
    }
}
