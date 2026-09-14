using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class ControladorMinotauro : MonoBehaviour
{
    private NavMeshAgent MeshAgent; // En este caso, será la guía que te provee unity para que el minotauro te persiga
    private Transform transformJugador; // Con esto se toma la posición del jugador
    private Animator animadorMinotauro; // El Animator es como el "esqueleto" del minotauro para que pueda animarse

    [Header("Configuración de Ataque")]
    // Distancia a la que el Minotauro suelta el golpe
    [SerializeField] private float distanciaAtaque = 2.5f; 
    // Tiempo de espera entre golpes para que no ataque infinitamente por segundo
    [SerializeField] private float tiempoEntreAtaques = 1.5f; 
    private float cronometroAtaque;

    [Header("Animaciones")] // Animaciones que ya venían incluidas con el minotauro, yo solo pongo su nombre para identificarlas
    [SerializeField] private string Caminar = "walk_forward";       
    [SerializeField] private string Golpear = "attack1";

    [Header("Comportamiento con Espada")] // Condición si se consigue la espada, para que el minotauro tenga otro destino
    [SerializeField] private Transform puertaDeSalida;

    void Start()
    {
        // Obtenemos el componente NavMeshAgent que le pusimos al minotauro
        MeshAgent = GetComponent<NavMeshAgent>();
        // Lo mismo con el componente de Animator
        animadorMinotauro = GetComponent<Animator>();
        // Buscamos al jugador en el mapa usando su tag
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        
        if (jugador != null) // Si se encuentra al jugador, su posición se convierte automáticamente
        {
            transformJugador = jugador.transform;
        }
    }

    
    void Update()
    {
        if (transformJugador == null || MeshAgent == null) return;
        if (GameManager.instancia != null && GameManager.instancia.ObtenerInvulnerabilidad() == true) // Pregunto primero si existe un GameManager y si el usuario es invulnerable
        {
            cronometroAtaque = 0; // El ataque en 0 porque no tiene sentido atacar a algo invulnerable

            if (puertaDeSalida != null) 
            {
                // Se va caminando hacia la puerta de salida y cuando llega, se destruye
                MeshAgent.isStopped = false;
                MeshAgent.SetDestination(puertaDeSalida.position);
                
                if (animadorMinotauro != null)
                {
                    animadorMinotauro.Play(Caminar);
                }

                float distanciaALaPuerta = Vector3.Distance(transform.position, puertaDeSalida.position);

                if (distanciaALaPuerta <= 2f) // A veces el minotauro no llega a la colisión de la puerta, entonces al estar a una distancia cercana, se destruye
                {
                    if (GameManager.instancia != null)
                    {
                        GameManager.instancia.AvisarMinotauroEscapo(); // Función para hacerle saber al jugador que ya no hay ningún peligro
                    }
                    Destroy(gameObject); 
                }
            }
            return; // Cortamos el update aquí para que ignore todo el código de persecución de abajo
        }   
        
        // Calcular la distancia entre el minotauro y el jugador
        float distanciaActual = Vector3.Distance(transform.position, transformJugador.position);

        // Aumentar el cronómetro del ataque con el tiempo real del juego
        cronometroAtaque += Time.deltaTime;

        if (distanciaActual <= distanciaAtaque)
        {
            // Si está muy cerca, frena al agente para que no camine sobre el jugador mientras golpea
            MeshAgent.isStopped = true;

            // Si ya pasó el tiempo de espera, ejecuta el golpe
            if (cronometroAtaque >= tiempoEntreAtaques)
            {
                Atacar();
            }
        }
        else
        {
            // Si el jugador se aleja, el minotauro vuelve a caminar, dándole chances al jugador para esquivar
            MeshAgent.isStopped = false;
            MeshAgent.SetDestination(transformJugador.position);
            if (animadorMinotauro != null)
            {
                animadorMinotauro.Play(Caminar);
            }
        }
    }

    void Atacar()
    {
        cronometroAtaque = 0; // Reiniciar el reloj del ataque
        
        if (animadorMinotauro != null)
        {
            animadorMinotauro.Play(Golpear);
        }
        StartCoroutine(EsperarImpactoDelGolpe()); // Se inicia una corrutina para "esperar", esto para que no nos maten antes de que termine la animación de ataque
    }

    // Esto es una función especial que puede pausar el la ejecución del código por la cantidad que se ponga en "yield return"
    System.Collections.IEnumerator EsperarImpactoDelGolpe()
    {
        // Se "pausa" el código para que termine la animación de ataque y luego nos mate, en este caso 0.8 (Se fue ajustando a medida de varias pruebas)
        yield return new WaitForSeconds(0.8f);

        if (transformJugador != null)
        {
            float distanciaEnImpacto = Vector3.Distance(transform.position, transformJugador.position);

            // Si el jugador se movió rápido y ya no está en el rango de ataque el ataque falla
            if (distanciaEnImpacto > distanciaAtaque)
            {
                MeshAgent.isStopped = false; 

                // Rompemos la corrutina para evitar que mate el jugador
                yield break; 
            }
        }

        // Pasado ese tiempo, se ejecuta el daño real, llevándonos a la pantalla de muerte
        SceneManager.LoadScene("PantallaMuerte"); 
    } 
 
}