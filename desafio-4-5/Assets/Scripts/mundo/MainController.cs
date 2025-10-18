using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainController: método público para cargar la siguiente escena en Build Settings.
/// Adjuntar al GameObject "MainUI" (Canvas).
/// </summary>
public class MainController : MonoBehaviour
{
    [Tooltip("Si es true, al llegar a la última escena vuelve a la primera (índice 0).")]
    public bool loopToFirst = true;

    /// <summary>
    /// Carga la siguiente escena según el índice en Build Settings.
    /// </summary>
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex; // índice de la escena actual
        int nextIndex = currentIndex + 1; // siguiente posición

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            if (loopToFirst)
            {
                SceneManager.LoadScene(0); // vuelve a la primera escena
            }
            else
            {
                Debug.LogWarning("LoadNextScene: ya estás en la última escena y loopToFirst está desactivado.");
            }
        }
    }

    /// <summary>
    /// Cargar una escena por nombre (alternativa).
    /// </summary>
    /// <param name="sceneName">Nombre de la escena tal como está en Build Settings.</param>
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("LoadSceneByName: sceneName vacío.");
    }

    /// <summary>
    /// Recarga la escena actual.
    /// </summary>
    public void ReloadCurrentScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex);
    }
}
