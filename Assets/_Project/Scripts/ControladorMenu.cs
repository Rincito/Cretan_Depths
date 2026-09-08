using UnityEngine;
using UnityEngine.SceneManagement;

public class ControladorMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelInfo;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.None; // Ponemos esto únicamente por si el mouse se queda con la vista bloqueda, con esto se recupera el cursor
        // Nos aseguramos por código de que la ventana arranque apagada por las dudas
        if (panelInfo != null)
        {
            panelInfo.SetActive(false);
        }
    }

    public void CambiarAEscenaJuego()
    {
        // Carga la escena del laberinto 
        SceneManager.LoadScene("SampleScene"); 
    }

    public void AbrirVentanaInfo()
    {
        if (panelInfo != null)
        {
            panelInfo.SetActive(true); // "Prende" la ventana emergente en la pantalla
        }
    }

    public void CerrarVentanaInfo()
    {
        if (panelInfo != null)
        {
            panelInfo.SetActive(false); // "Apaga" la ventana emergente
        }
    }

    public void SalirDelJuego()
    {
        // Cierra la aplicación de forma definitiva
        Application.Quit();
    }
}
