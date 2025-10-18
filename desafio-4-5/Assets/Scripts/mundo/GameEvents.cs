using System;

public static class GameEvents
{
    public static event Action OnPause;
    public static event Action OnResume;
    public static event Action OnGameOver;
    public static event Action OnVictory;

    public static void TriggerPause() => OnPause?.Invoke();
    public static void TriggerResume() => OnResume?.Invoke();
    public static void TriggerGameOver() => OnGameOver?.Invoke();
    public static void TriggerVictory() => OnVictory?.Invoke();
}
