using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ControladorPantVictoria : MonoBehaviour
{
    public TextMeshProUGUI textoPuntaje; 

    void Start()
    {
        // Saca el puntaje guardado en el disco. Si no encuentra nada, pone 0 por defecto
        int puntajeObtenido = PlayerPrefs.GetInt("PuntajeFinal", 0);

        // Lo muestra en la pantalla
        if (textoPuntaje != null)
        {
            textoPuntaje.text = "Puntaje final: " + puntajeObtenido;
        }
    }

    public void CambiarAEscenaMenu()
    {
        // Carga la escena del menú principal 
        SceneManager.LoadScene("MenuPrincipal"); 
    }
}
