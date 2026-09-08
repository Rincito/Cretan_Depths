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
            
            if (GameManager.instancia != null)
            {
                int puntajeFinal = GameManager.instancia.ObtenerPuntaje() ;

                // Se guarda el puntaje en la memoria interna con la clave "PuntajeFinal"
                PlayerPrefs.SetInt("PuntajeFinal", puntajeFinal);
                PlayerPrefs.Save(); // Asegura que se guarde de inmediato
            }

            Cursor.lockState = CursorLockMode.None; // Se activa el cursor

            // Salta a la pantalla de ganar
            SceneManager.LoadScene(escenaVictoria);
        }
        else if (other.CompareTag("Enemy"))
        {   
            // Destruimos por completo al minotauro de la escena
            Destroy(other.gameObject); 
        }
    }
}
