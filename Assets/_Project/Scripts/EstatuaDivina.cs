using UnityEngine;
using TMPro; 

public class EstatuaDivina : MonoBehaviour
{
    [Header("Configuración de la Estatua")]
    [SerializeField] private string nombrePoder = "Correr"; 
    [SerializeField] private int costoDracmas = 15;
    [SerializeField] private float multiplicadorPoder = 1.5f; 

    [Header("Efecto Visual (Aura / Luz)")]
    [SerializeField] private GameObject luzOAuraCian;

    [Header("Configuración de Nivel")]
    [SerializeField] private bool esNivelUno = true; 

    private bool esActivaEnEstaPartida = false;
    private bool jugadorCerca = false;
    private bool poderComprado = false;

    void Start()
    {
        if (luzOAuraCian != null)
        {
            luzOAuraCian.SetActive(esActivaEnEstaPartida);
        }
    }

    // Este método lo llama el Generador para "encender" esta estatua en esta partida
    public void ActivarEstatuaConPoder(string poder, int costo)
    {
        esActivaEnEstaPartida = true;
        nombrePoder = poder;
        costoDracmas = costo;

        // Prende el aura/luz cian para que destaque en el laberinto
        if (luzOAuraCian != null)
        {
            luzOAuraCian.SetActive(true);
            Debug.Log("¡Luz encendida en la estatua: " + gameObject.name + "!");
        }

        Debug.Log("¡La estatua de este sector está activa con el poder: " + nombrePoder + "!");
    }

    void Update()
    {
        if (esActivaEnEstaPartida && jugadorCerca && !poderComprado && Input.GetKeyDown(KeyCode.E))
        {
            IntentarComprarBendicion();
        }
    }

    void IntentarComprarBendicion()
    {
       if (GameManager.instancia != null)
        {
            int puntajeActual = GameManager.instancia.ObtenerPuntaje();

            if (puntajeActual >= costoDracmas)
            {
                GameManager.instancia.SumarPuntos(-costoDracmas); 
                OtorgarPoderAlJugador();
                poderComprado = true;
                
                if (luzOAuraCian != null) luzOAuraCian.SetActive(false);

                Debug.Log("¡Bendición recibida: " + nombrePoder + "!");
            }
            else
            {
                Debug.Log("No tienes suficientes dracmas para esta bendición.");
            }
        }
    }

    void OtorgarPoderAlJugador()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
    if (jugador != null)
    {
        ControlarUsuario movimiento = jugador.GetComponent<ControlarUsuario>();

        if (movimiento != null)
        {
            if (nombrePoder == "Correr")
            {
                movimiento.tienePoderCorrer = true; // Desbloquea el uso de Shift
                
                // Acá le avisamos al GameManager o a un script de UI que active el icono de color
                Debug.Log("¡Poder de Correr desbloqueado!");
            }
        }
    }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            Debug.Log("Presiona E para comprar la bendición de " + nombrePoder + " por " + costoDracmas + " dracmas.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
        }
    }
}