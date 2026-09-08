using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorPantMuerte : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void CambiarAEscenaJuego()
    {
        // Carga la escena del laberinto 
        SceneManager.LoadScene("SampleScene"); 
    }

    public void CambiarAEscenaMenu()
    {
        // Carga la escena del menú principal 
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}
