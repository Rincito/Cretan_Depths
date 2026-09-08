using UnityEngine;

public class Coleccionables : MonoBehaviour
{

    // Se crea una lista de opciones para el inspector de unity
    private enum TipoColeccionable { Moneda, Cofre, Espada }

    [Header("Configuración del Objeto")] // Header no afecta a nivel código, es para crear apartados visuales en unity y ser más ordenado
    [SerializeField] private TipoColeccionable tipoDeObjeto; // Acá se elige qué es en el inspector
    [SerializeField] private float velocidadRotacion = 100f;

    private int valorPuntos;

    void Start()
    {
        // Dependiendo de lo que elijas en el inspector, asignamos los puntos de forma automática
        if (tipoDeObjeto == TipoColeccionable.Moneda)
        {
            valorPuntos = 1; // La moneda da 1 punto
        }
        else if (tipoDeObjeto == TipoColeccionable.Cofre)
        {
            valorPuntos = 5; // El cofre da 5 puntos
        }
    }

    
    void Update()
    {
        // Los objetos van a girar en el laberinto para hacerlo más llamativo
        
        if (tipoDeObjeto == TipoColeccionable.Moneda)
        {
            transform.Rotate(Vector3.right * velocidadRotacion * Time.deltaTime); // Multiplicamos por Time.deltaTime para que la rotación sea suave y dependa del tiempo real, no de los FPS de la computadora
        }
        else if (tipoDeObjeto == TipoColeccionable.Cofre || tipoDeObjeto == TipoColeccionable.Espada)
        {
            transform.Rotate(Vector3.up * velocidadRotacion * Time.deltaTime);
        }
    }


    void OnTriggerEnter(Collider other) // Función OnTriggerEnter es para cuando se pase por encima del objeto
    {   
        if (other.CompareTag("Player")) // Si el objeto que pasa tiene la etique o tag "Player" se activa, evitando que el minotauro "robe" puntuación
        {
            // Si es  una moneda o un cofre sumamos puntos de forma normal
            if (tipoDeObjeto == TipoColeccionable.Moneda || tipoDeObjeto == TipoColeccionable.Cofre)
            {
                if (GameManager.instancia != null) // Pedimos al GameManager que se encargue de este proceso, ya que es la "mente" del programa por así decirlo
                {
                    GameManager.instancia.SumarPuntos(valorPuntos);
                }
            }
            // Si es una espada activamos la invulnerabilidad en el GameManager
            else if (tipoDeObjeto == TipoColeccionable.Espada)
            {
                if (GameManager.instancia != null)
                {
                    GameManager.instancia.ActivarInvulnerabilidad();
                }
            }

            // Destruimos el objeto recolectado, sea cual sea
            Destroy(gameObject);
        }
    }
}
