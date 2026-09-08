using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    // El Singleton: permite que cualquier script del juego acceda al GameManager fácilmente
    public static GameManager instancia;

    [Header("UI del Juego")]
    [SerializeField] private TextMeshProUGUI textoPuntaje; // Elementos de texto de la UI
    [SerializeField] private TextMeshProUGUI textoTemporizador;

    [Header("Configuración del Minotauro")]
    [SerializeField] private float tiempoParaLiberar = 30.0f; // Segundos antes de que aparezca el minotauro
    [SerializeField] private GameObject minotauro;          

    private int puntajeTotal = 0;
    private bool minotauroLiberado = false;
    private bool esInvulnerable = false;
    private bool minotauroEscapo = false;

    void Awake()
    {
        // Configuración del Singleton. Acá se asegura que haya un solo GameManager o "cerebro"
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ActualizarInterfazPuntaje();
        if (minotauro != null)
        {
            minotauro.SetActive(false); // Desactivamos al minotauro hasta su salida programada
        }
    }

    void Update()
    {
        // Si el Minotauro ya salió o se fue no necesitamos seguir descontando tiempo
        if (minotauroLiberado) return;
        if (minotauroEscapo) return;
        // Si el tiempo aún no ha terminado, restamos el tiempo real que pasa
        if (tiempoParaLiberar > 0)
        {
            tiempoParaLiberar -= Time.deltaTime;
            ActualizarInterfazReloj();
        }
        else
        {
            // ¡El tiempo llega a cero y se libera el minotauro
            LiberarAlMinotauro();
        }
    }

    // Este método lo llaman los cofres y monedas cuando se pasa por encima, por eso está público
    public void SumarPuntos(int puntosAsumar)
    {
        puntajeTotal += puntosAsumar;
        ActualizarInterfazPuntaje();
    }

    // Este método es para visualizar el puntaje total sin necesidad de la variable pública (así no modifican la puntuación)
    public int ObtenerPuntaje()
    {
        return puntajeTotal;
    }

    // Actualiza el texto que ve el jugador en la pantalla de su puntaje
    void ActualizarInterfazPuntaje()
    {
        if (textoPuntaje != null)
        {
            textoPuntaje.text = "Puntaje: " + puntajeTotal;
        }
    }

    // Actualiza el texto de la liberación del minotauro
    void ActualizarInterfazReloj()
    {
        if (textoTemporizador != null)
        {
            // Mathf.CeilToInt redondea los decimales hacia arriba para que se vean segundos limpios (30, 29, 28, etc)
            textoTemporizador.text = "El Minotauro se libera en: " + Mathf.CeilToInt(tiempoParaLiberar) + "s";
        }
    }

    // Método para que el minotauro aparezca en la escena y empiece a ejecutar sus comandos (mientras no está activo no ejecuta ninguno de sus scripts)
    void LiberarAlMinotauro()
    {
        minotauroLiberado = true;

        if (textoTemporizador != null)
        {
            textoTemporizador.text = "¡EL MINOTAURO HA SIDO LIBERADO!";
            textoTemporizador.color = Color.red; // Cambia el texto a rojo para dar sensación de peligro
        }

        if (minotauro != null)
        {
            minotauro.SetActive(true); // Enciende al enemigo en el mapa para que empiece a perseguirte
        }
    }

    // Este método lo llama la espada al ser recolectada
    public void ActivarInvulnerabilidad()
    {
        esInvulnerable = true;
    }


    // Este método lo llama el minotauro para saber si seguir persiguiendo al jugador o irse del nivel
    public bool ObtenerInvulnerabilidad()
    {
        return esInvulnerable;
    }

    // Este metódo hace que cambie la UI para avisar al jugador que ya puede juntar las recompensas sin problema
    public void AvisarMinotauroEscapo()
    {
        minotauroEscapo = true;

        if (textoTemporizador != null)
        {
            textoTemporizador.text = "EL MINOTAURO HA DEJADO EL NIVEL";
            textoTemporizador.color = Color.green;
        }
    }
}
