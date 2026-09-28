using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum EstadoMinotauro { Patrullando, Persiguiendo, Buscando }

[RequireComponent(typeof(NavMeshAgent))]
public class ControladorMinotauro : MonoBehaviour
{
    private NavMeshAgent MeshAgent; // En este caso, será la guía que te provee unity para que el minotauro te persiga
    private Transform transformJugador; // Con esto se toma la posición del jugador
    private Animator animadorMinotauro; // El Animator es como el "esqueleto" del minotauro para que pueda animarse

    [Header("Configuración de Visión y Capas")]
    [SerializeField] private float rangoVision = 12f;
    [Range(0, 360)] [SerializeField] private float anguloVision = 90f;
    [SerializeField] private LayerMask capaParedes; // Asignar la capa 'Paredes' en el inspector

    [Header("Comportamiento e IA")]
    public EstadoMinotauro estadoActual = EstadoMinotauro.Patrullando;
    [SerializeField] private float velocidadPatrulla = 3.5f;
    [SerializeField] private float velocidadPersecucion = 6.0f;
    [SerializeField] private float tiempoMemoriaBusqueda = 5.0f; // Segundos que busca en la última posición conocida
    [SerializeField] private float radioPatrulla = 15.0f;
    [SerializeField] private float tiempoEsperaPuntoPatrulla = 2.0f;

    [Header("Configuración de Ataque")]
    // Distancia a la que el Minotauro suelta el golpe
    [SerializeField] private float distanciaAtaque = 2.5f; 
    // Tiempo de espera entre golpes para que no ataque infinitamente por segundo
    [SerializeField] private float tiempoEntreAtaques = 1.5f; 
    private float cronometroAtaque;

    [Header("Animaciones")] // Animaciones que ya venían incluidas con el minotauro, yo solo pongo su nombre para identificarlas
    [SerializeField] private string Idle = "idle";
    [SerializeField] private string Correr = "run";
    [SerializeField] private string Caminar = "walk_forward";       
    [SerializeField] private string Golpear = "attack1";

    // Variables internas de control de IA
    private Vector3 ultimaPosicionConocida;
    private float temporizadorBusqueda;
    private bool esperandoEnPunto;

    private bool estaAtacando; // Bandera para bloquear interrupciones durante el golpe

    private string animacionActual = "";

    void Start()
    {
        // Obtenemos el componente NavMeshAgent que le pusimos al minotauro
        MeshAgent = GetComponent<NavMeshAgent>();
        // Lo mismo con el componente de Animator
        animadorMinotauro = GetComponent<Animator>();
        // Buscamos al jugador en el mapa usando su tag
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        
        if (jugador != null) // Si se encuentra al jugador, se asigna su transform
    {
        transformJugador = jugador.transform;
    }
    else
    {
        Debug.LogWarning("¡Atención! No se encontró ningún GameObject con el Tag 'Player' en la escena.");
    }

    if (MeshAgent != null)
    {
        MeshAgent.speed = velocidadPatrulla;
        IrANuevoPuntoPatrulla();
    }
    }

    
    void Update()
    {
        if (transformJugador == null || MeshAgent == null) return;

        // IMPORTANTE: Si el agente no está activo o aún no ha sido colocado sobre el NavMesh (Frame 0 de generación), ignoramos el frame.
        if (!MeshAgent.enabled || !MeshAgent.isOnNavMesh) return;  
        
        if (estaAtacando) return; //Si ataca no se le corta la animación de golpe

        // ----------------------------------------------------
        // MÁQUINA DE ESTADOS E IA (Visión y Memoria)
        // ----------------------------------------------------
        bool veAlJugador = PuedeVerAlJugador();

        switch (estadoActual)
        {
            case EstadoMinotauro.Patrullando:
                ActualizarPatrulla(veAlJugador);
                break;

            case EstadoMinotauro.Persiguiendo:
                ActualizarPersecucion(veAlJugador);
                break;

            case EstadoMinotauro.Buscando:
                ActualizarBusqueda(veAlJugador);
                break;
        }
    }

    private void CambiarAnimacion(string nuevaAnimacion)
    {
        if (animacionActual == nuevaAnimacion) return;

        if (animadorMinotauro != null)
        {
            animadorMinotauro.Play(nuevaAnimacion);
            animacionActual = nuevaAnimacion;
        }
    }

    // ----------------------------------------------------
    // SENSOR VISUAL (Campo de Visión)
    // ----------------------------------------------------
    public bool PuedeVerAlJugador()
    {
        if (transformJugador == null) return false;

        Vector3 direccionAlJugador = (transformJugador.position - transform.position).normalized;
        float distanciaAlJugador = Vector3.Distance(transform.position, transformJugador.position);

        // 1. Rango de visión
        if (distanciaAlJugador <= rangoVision)
        {
            // 2. Cono / Ángulo de visión
            if (Vector3.Angle(transform.forward, direccionAlJugador) < anguloVision / 2f)
            {
                // 3. Raycast evitando Muros (Paredes)
                Vector3 origenOjos = transform.position + Vector3.up * 1.5f;
                Vector3 destinoObjetivo = transformJugador.position + Vector3.up * 1.0f;
                Vector3 direccionLineal = (destinoObjetivo - origenOjos).normalized;

                if (!Physics.Raycast(origenOjos, direccionLineal, distanciaAlJugador, capaParedes))
                {
                    return true; // Vista limpia
                }
            }
        }

        return false;
    }

    // ----------------------------------------------------
    // MÉTODOS DE ESTADOS (Patrulla, Persecución, Búsqueda)
    // ----------------------------------------------------
    private void ActualizarPatrulla(bool veAlJugador)
    {
        if (veAlJugador)
        {
            esperandoEnPunto = false;
            StopAllCoroutines();
            
            estadoActual = EstadoMinotauro.Persiguiendo;
            MeshAgent.speed = velocidadPersecucion;
            return;
        }

        // Si se está moviendo hacia un punto de patrulla, reproduce la animación de Caminar
        if (!esperandoEnPunto && animadorMinotauro != null) 
        {
            CambiarAnimacion(Caminar);
        }

        if (!MeshAgent.pathPending && MeshAgent.remainingDistance <= MeshAgent.stoppingDistance)
        {
            if (!esperandoEnPunto)
            {
                StartCoroutine(RutinaEsperarYPatrullar());
            }
        }
    }

    private void ActualizarPersecucion(bool veAlJugador)
    {
        if (veAlJugador)
        {
            ultimaPosicionConocida = transformJugador.position;
            float distanciaActual = Vector3.Distance(transform.position, transformJugador.position);
            cronometroAtaque += Time.deltaTime;

            if (distanciaActual <= distanciaAtaque)
            {
                MeshAgent.isStopped = true;
                if (cronometroAtaque >= tiempoEntreAtaques)
                {
                    Atacar();
                }
            }
            else
            {
                MeshAgent.isStopped = false;
                MeshAgent.SetDestination(transformJugador.position);
                CambiarAnimacion(Correr);
            }
        }
        else
        {
            // Perdió la vista -> Pasa a estado de Búsqueda (Memoria)
            estadoActual = EstadoMinotauro.Buscando;
            temporizadorBusqueda = tiempoMemoriaBusqueda;
            MeshAgent.isStopped = false;
            MeshAgent.SetDestination(ultimaPosicionConocida);
        }
    }

    private void ActualizarBusqueda(bool veAlJugador)
    {
        if (veAlJugador)
        {
            estadoActual = EstadoMinotauro.Persiguiendo;
            MeshAgent.speed = velocidadPersecucion;
            return;
        }

        temporizadorBusqueda -= Time.deltaTime;
        CambiarAnimacion(Caminar);

        // Llegó a la última posición conocida y espera a que venza el temporizador
        if (!MeshAgent.pathPending && MeshAgent.remainingDistance <= MeshAgent.stoppingDistance)
        {
            if (temporizadorBusqueda <= 0)
            {
                estadoActual = EstadoMinotauro.Patrullando;
                MeshAgent.speed = velocidadPatrulla;
                IrANuevoPuntoPatrulla();
            }
        }
    }

    void Atacar()
    {
        cronometroAtaque = 0; // Reiniciar el reloj del ataque
        
        CambiarAnimacion(Golpear);
        
        StartCoroutine(EsperarImpactoDelGolpe()); // Se inicia una corrutina para "esperar", esto para que no nos maten antes de que termine la animación de ataque
    }

    // Esto es una función especial que puede pausar el la ejecución del código por la cantidad que se ponga en "yield return"
    System.Collections.IEnumerator EsperarImpactoDelGolpe()
    {
        estaAtacando = true; // Bloquea el Update() para que la animación termine sí o sí

        // Rotar levemente hacia el jugador al inicio del golpe
        if (transformJugador != null)
        {
            Vector3 direccion = (transformJugador.position - transform.position).normalized;
            direccion.y = 0;
            if (direccion != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direccion);
        }

        // Se "pausa" el código para que termine la animación de ataque y luego nos mate, en este caso 0.8 (Se fue ajustando a medida de varias pruebas)
        yield return new WaitForSeconds(1f);

        if (transformJugador != null)
        {
            float distanciaEnImpacto = Vector3.Distance(transform.position, transformJugador.position);

            // Si el jugador se movió rápido y ya no está en el rango de ataque el ataque falla
            if (distanciaEnImpacto > distanciaAtaque)
            {
                MeshAgent.isStopped = false; 
                estaAtacando = false; // Desbloquea la IA para que vuelva a perseguir/patrullar
                animacionActual = "";
                // Rompemos la corrutina para evitar que mate el jugador
                yield break; 
            }
        }

        // Pasado ese tiempo, se ejecuta el daño real, llevándonos a la pantalla de muerte
        SceneManager.LoadScene("PantallaMuerte"); 
    } 
 
    // ----------------------------------------------------
    // NAVEGACIÓN Y PATRULLA
    // ----------------------------------------------------
    private IEnumerator RutinaEsperarYPatrullar()
    {
        esperandoEnPunto = true;
        CambiarAnimacion(Idle);

        if (animadorMinotauro != null) animadorMinotauro.Play(Idle);
        
        yield return new WaitForSeconds(tiempoEsperaPuntoPatrulla);

        if (estadoActual == EstadoMinotauro.Patrullando)
        {
            IrANuevoPuntoPatrulla();
        }
        esperandoEnPunto = false;
    }

    private void IrANuevoPuntoPatrulla()
    {
        Vector3 puntoAleatorio = ObtenerPuntoAleatorioNavMesh(transform.position, radioPatrulla);
        MeshAgent.isStopped = false;
        MeshAgent.SetDestination(puntoAleatorio);
    }

    private Vector3 ObtenerPuntoAleatorioNavMesh(Vector3 centro, float distancia)
    {
        Vector3 direccionAleatoria = Random.insideUnitSphere * distancia;
        direccionAleatoria += centro;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(direccionAleatoria, out hit, distancia, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return centro;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        Vector3 fov1 = Quaternion.AngleAxis(anguloVision / 2, Vector3.up) * transform.forward;
        Vector3 fov2 = Quaternion.AngleAxis(-anguloVision / 2, Vector3.up) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, fov1 * rangoVision);
        Gizmos.DrawRay(transform.position + Vector3.up * 1.5f, fov2 * rangoVision);
    }
}
