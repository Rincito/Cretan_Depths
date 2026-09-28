using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaSalida : MonoBehaviour
{
    [Header("Configuración de Escena")]
    
    [SerializeField] private string escenaVictoria = "PantallaWin"; 

    // Este método se activa automáticamente cuando algo entra en el trigger de la puerta
    private void OnTriggerEnter(Collider other)
    {
        // Comprobamos si lo que cruzó la puerta es el jugador (o en caso contrario, el minotauro)
        if (other.CompareTag("Player"))
        {
            
            if (GameManager.instancia.PuedeEscapar())
                {
                    // Descuenta los 20 puntos y nos devuelve el sobrante para el menú final
                    int puntajeFinal = GameManager.instancia.CobrarPeajeYObtenerPuntajeFinal();

                    PlayerPrefs.SetInt("PuntajeFinal", puntajeFinal);
                    PlayerPrefs.Save(); 

                    Cursor.lockState = CursorLockMode.None; // Liberar el cursor
                    SceneManager.LoadScene(escenaVictoria);
                }
                else
                {
                    Debug.Log("No tienes suficientes puntos para pagar el peaje de salida.");
                }
        }
    }
}

