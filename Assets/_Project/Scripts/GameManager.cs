using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // El Singleton: permite que cualquier script del juego acceda al GameManager fácilmente
    public static GameManager instancia;

    [Header("UI del Juego")]
    [SerializeField] private TextMeshProUGUI textoPuntaje; 
    [SerializeField] private TextMeshProUGUI textoConteoPuerta;       

    [Header("Requisito de Salida")]
    [SerializeField] private int monedasRequeridas = 20;

    private int puntajeTotal = 0;

    void Awake()
    {
        // Configuración del Singleton
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
        ActualizarInterfaz();
    }

    // Este método lo llaman los cofres y monedas cuando se pasa por encima
    public void SumarPuntos(int puntosAsumar)
    {
        puntajeTotal += puntosAsumar;
        ActualizarInterfaz();
    }

    // Devuelve el puntaje total acumulado
    public int ObtenerPuntaje()
    {
        return puntajeTotal;
    }

    public int ObtenerPuntosRequeridos()
    {
        return monedasRequeridas;
    }

    public bool PuedeEscapar()
    {
        return puntajeTotal >= monedasRequeridas;
    }

    public int CobrarPeajeYObtenerPuntajeFinal()
    {
        puntajeTotal -= monedasRequeridas;
        if (puntajeTotal < 0) puntajeTotal = 0; // Evita valores negativos por seguridad
        return puntajeTotal;
    }

    void ActualizarInterfaz()
    {
        // 1. Mostrar puntaje acumulado actual
        if (textoPuntaje != null)
        {
            textoPuntaje.text = "Puntaje: " + puntajeTotal;
        }

        // 2. Mostrar cuánto falta o si ya podés huir
        if (textoConteoPuerta != null)
        {
            int faltantes = monedasRequeridas - puntajeTotal;

            if (faltantes > 0)
            {
                textoConteoPuerta.text = "Te faltan " + faltantes + " puntos para abrir la salida";
                textoConteoPuerta.color = Color.white;
            }
            else
            {
                textoConteoPuerta.text = "La salida está libre. HUYE";
                textoConteoPuerta.color = Color.green;
            }
        }
    }
}