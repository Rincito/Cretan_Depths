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
