using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra cómo asignar el onClick del botón por código apuntando al MainController.
/// Útil si prefieres hacerlo desde script en vez del Inspector.
/// </summary>
public class PlayButtonHook : MonoBehaviour
{
    public Button playButton;              // asignar en inspector (o obtener por GetComponent)
    public MainController mainController;  // referencia al MainController en MainUI

    void Start()
    {
        if (playButton == null || mainController == null)
        {
            Debug.LogError("PlayButtonHook: Asigna playButton y mainController en el Inspector.");
            return;
        }

        // Añadimos la llamada a LoadNextScene cuando se haga click
        playButton.onClick.AddListener(mainController.LoadNextScene);
    }
}
