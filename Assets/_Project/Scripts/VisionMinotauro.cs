using UnityEngine;

public class VisionMinotauro : MonoBehaviour
{
    [Header("Configuración de Visión")]
    public float rangoVision = 12f;
    [Range(0, 360)] public float anguloVision = 90f;
    public LayerMask capaJugador;
    public LayerMask capaObstaculos; // Incluye la capa de tus muros/laberinto

    [Header("Referencias")]
    public Transform objetivoJugador;

    [Header("Detección de Espalda (Punto Ciego)")]
    [SerializeField] private float tiempoParaGirarse = 8f; // Segundos que tarda en reaccionar si estás atrás
    private float timerEspalda = 0f;
    [HideInInspector] public bool jugadorEnPuntoCiego = false;

    void Update()
    {
        // Evaluamos constantemente si el jugador está cerca pero fuera de la vista
        ManejarDeteccionEspalda();
    }

    public bool PuedeVerAlJugador()
    {
        if (objetivoJugador == null) return false;

        Vector3 direccionAlJugador = (objetivoJugador.position - transform.position).normalized;
        float distanciaAlJugador = Vector3.Distance(transform.position, objetivoJugador.position);

        // 1. Validar si está dentro del rango
        if (distanciaAlJugador <= rangoVision)
        {
            // 2. Validar si está dentro del ángulo visual (Cono)
            if (Vector3.Angle(transform.forward, direccionAlJugador) < anguloVision / 2f)
            {
                // 3. Validar que no haya muros interrumpiendo la vista (Raycast)
                // Elevamos un poco el origen para no colisionar con el suelo (e.g. 1.5m de altura)
                Vector3 origenOjos = transform.position + Vector3.up * 1.5f;
                Vector3 destinoOjos = objetivoJugador.position + Vector3.up * 1.0f;
                Vector3 dirOjos = (destinoOjos - origenOjos).normalized;

                if (!Physics.Raycast(origenOjos, dirOjos, distanciaAlJugador, capaObstaculos))
                {
                    return true; // Vista clara del jugador
                }
            }
        }

        return false;
    }

    void ManejarDeteccionEspalda()
    {
        if (objetivoJugador == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, objetivoJugador.position);

        // Si el jugador está dentro del radio de visión general, pero NO lo ve de frente (está a los lados o atrás)
        if (distanciaAlJugador <= rangoVision)
        {
            Vector3 direccionAlJugador = (objetivoJugador.position - transform.position).normalized;
            float anguloConJugador = Vector3.Angle(transform.forward, direccionAlJugador);

            // Si está fuera del cono de visión frontal
            if (anguloConJugador >= anguloVision / 2f)
            {
                // Verificamos también que no haya un muro gigante en el medio impidiendo cualquier noción
                Vector3 origenOjos = transform.position + Vector3.up * 1.5f;
                Vector3 destinoOjos = objetivoJugador.position + Vector3.up * 1.0f;
                
                if (!Physics.Raycast(origenOjos, (destinoOjos - origenOjos).normalized, distanciaAlJugador, capaObstaculos))
                {
                    // ¡El jugador está detrás o al costado sin que lo mire directamente!
                    jugadorEnPuntoCiego = true;
                    timerEspalda += Time.deltaTime;

                    if (timerEspalda >= tiempoParaGirarse)
                    {
                        GirarHaciaJugador();
                        timerEspalda = 0f; // Reiniciamos el contador tras girar
                    }
                    return;
                }
            }
        }

        // Si el jugador entra en su campo visual frontal o se aleja demasiado, reiniciamos el temporizador
        timerEspalda = 0f;
        jugadorEnPuntoCiego = false;
    }

    void GirarHaciaJugador()
    {
        Vector3 direccion = (objetivoJugador.position - transform.position).normalized;
        direccion.y = 0; // Mantener plano en el suelo
        transform.rotation = Quaternion.LookRotation(direccion);

        Debug.Log("¡El Minotauro notó que estabas a sus espaldas tras los 7 segundos y se dio vuelta!");
    }

    // Dibujar el rango visual en la escena para depuración
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        Vector3 fovLine1 = Quaternion.AngleAxis(anguloVision / 2, Vector3.up) * transform.forward;
        Vector3 fovLine2 = Quaternion.AngleAxis(-anguloVision / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, fovLine1 * rangoVision);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, fovLine2 * rangoVision);
    }
}
