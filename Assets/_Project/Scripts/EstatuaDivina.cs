using UnityEngine;
using TMPro;
using System.Collections; 

public class EstatuaDivina : MonoBehaviour
{
    [Header("Configuración de la Estatua")]
    [SerializeField] private string nombrePoder = "Botas de Hermes"; 
    [SerializeField] private int costoDracmas = 15;
    [SerializeField] private float multiplicadorPoder = 1.5f;
    [SerializeField] private float distanciaInteraccion = 4.0f; 

    [Header("Efecto Visual (Aura / Luz)")]
    [SerializeField] private GameObject luzOAuraCian;

    [Header("Configuración de Texto Flotante (World Space)")]
    [SerializeField] private GameObject objetoCanvasFlotante; // El Canvas flotante sobre la estatua
    [SerializeField] private TextMeshProUGUI textoFlotante;    // El componente de texto

    [Header("Configuración de Nivel")]
    [SerializeField] private bool esNivelUno = true; 

    private bool esActivaEnEstaPartida = false;
    private bool poderComprado = false;
    private Transform transformJugador;

    void Start()
    {
        if (luzOAuraCian != null)
        {
            luzOAuraCian.SetActive(esActivaEnEstaPartida);
        }

        if (objetoCanvasFlotante != null)
        {
            objetoCanvasFlotante.SetActive(false);
        }

        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            transformJugador = jugador.transform;
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
        if (!esActivaEnEstaPartida || poderComprado || transformJugador == null) return;

        float distancia = Vector3.Distance(transform.position, transformJugador.position);

        if (distancia <= distanciaInteraccion)
        {
            // El jugador entró en el radio: mostramos el texto flotante
            if (objetoCanvasFlotante != null && !objetoCanvasFlotante.activeSelf)
            {
                objetoCanvasFlotante.SetActive(true);
                textoFlotante.text = $"Consigue {costoDracmas} dracmas para obtener las {nombrePoder}\n[Presiona E]";
            }

            // Si presiona E dentro de este radio, intenta comprar
            if (Input.GetKeyDown(KeyCode.E))
            {
                IntentarComprarBendicion();
            }
        }
        else
        {
            // Si el jugador se aleja de la distancia configurada, ocultamos el texto
            if (objetoCanvasFlotante != null && objetoCanvasFlotante.activeSelf)
            {
                objetoCanvasFlotante.SetActive(false);
            }
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

                // Cambiar texto flotante a la bendición conseguida
                if (textoFlotante != null)
                {
                    textoFlotante.text = "Has sido bendecido con las Botas de Hermes";
                }
                StartCoroutine(OcultarTextoTrasEspera(5f));
                Debug.Log("¡Bendición recibida: " + nombrePoder + "!");
            }
            else
            {
                Debug.Log("No tienes suficientes dracmas para esta bendición");
                if (textoFlotante != null)
                {
                    textoFlotante.text = "No tienes suficientes dracmas";
                }
            }
        }
    }

    IEnumerator OcultarTextoTrasEspera(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        if (objetoCanvasFlotante != null)
        {
            objetoCanvasFlotante.SetActive(false);
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
            if (nombrePoder == "Botas de Hermes")
            {
                movimiento.tienePoderCorrer = true; // Desbloquea el uso de Shift
                
                // Acá le avisamos al GameManager o a un script de UI que active el icono de color
                Debug.Log("¡Poder de Correr desbloqueado!");
            }
        }
    }
    }

}