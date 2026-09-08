using UnityEngine;

public class ControlarUsuario : MonoBehaviour
{

   [Header("Movimiento y Salto")] // Header no afecta a nivel código, es para crear apartados visuales en unity y ser más ordenado
    [SerializeField] private float velocidad = 6.0f;
    [SerializeField] private float velocidadRotacion = 170.0f;
    [SerializeField] private float fuerzaSalto = 8.0f;
    [SerializeField] private float gravedad = 20.0f;
    private Vector3 direccionMovimiento = Vector3.zero;

    [Header("Cámaras y Mouse")]
    [SerializeField] private GameObject camara3ra;
    [SerializeField] private GameObject camara1ra;
    [SerializeField] private float sensibilidadMouse = 2.0f;
    private float rotacionXCámara = 0.0f;

    [Header("Animaciones")]
    [SerializeField] private Animator animadorPersonaje;
    [SerializeField] private string Quieto = "Idle";   
    [SerializeField] private string Correr = "RunForward"; 
    [SerializeField] private string Saltar = "Jump";

    private CharacterController infoControlador;
    
    
    void Start()
    {
        // Buscamos el componente character controller dentro del mismo objeto
        infoControlador = GetComponent<CharacterController>();

        // Bloquea el mouse en el centro de la pantalla para que no se salga del juego
        Cursor.lockState = CursorLockMode.Locked;

        camara3ra.SetActive(false); // Se "apaga" la cámara de 3ra persona
        camara1ra.SetActive(true); // Se "prende" la cámara de 1ra persona
    }

    void Update()
    {
        // Rotación del mouse (Mirar hacia los lados y arriba/abajo)
        float mouseX = Input.GetAxis("Mouse X") * sensibilidadMouse;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidadMouse;

        // Girar el cuerpo del jugador de izquierda a derecha
        transform.Rotate(0, mouseX, 0);

        // Calcular la rotación arriba/abajo de la cámara (primera persona)
        rotacionXCámara -= mouseY;
        rotacionXCámara = Mathf.Clamp(rotacionXCámara, -60f, 60f); // Evita que la cámara dé una vuelta completa

        // Aplicamos la rotación arriba/abajo a las cámaras locales
        camara1ra.transform.localRotation = Quaternion.Euler(rotacionXCámara, 0, 0); //Los Quaterniones son una unidad de medida diferente, Euler es como un intérprete de los datos y lo transforma a Quaterniones


       // MOVIMIENTO, SALTO Y ANIMACIONES 
        if (infoControlador.isGrounded) // Básicamente si el jugador está en el suelo
        {
            float avance = Input.GetAxis("Vertical");
            float lateral = Input.GetAxis("Horizontal");

            Vector3 direccionForward = transform.forward * avance;
            Vector3 direccionRight = transform.right * lateral;
            direccionMovimiento = (direccionForward + direccionRight).normalized * velocidad; // .normalized para evitar que avance más tocando A, D + Salto, por ejemplo

            // Lógica de animaciones directa (.Play)
            if (animadorPersonaje != null)
            {
                // Si se presiona W, S, A o D (cualquier movimiento), corre con una animación que ya tenía el prefab. Si no, se queda quieto (otra animación del prefab)
                if (avance != 0 || lateral != 0)
                {
                    animadorPersonaje.Play(Correr);
                }
                else
                {
                    animadorPersonaje.Play(Quieto);
                }
            }

            // Lógica del salto
            if (Input.GetButtonDown("Jump"))
            {
                direccionMovimiento.y = fuerzaSalto;
                
                if (animadorPersonaje != null)
                {
                    animadorPersonaje.Play(Saltar); // Al saltar se pone la animación del prefab
                }
            }
        }

        // Aplicamos la gravedad frame a frame progresivamente
        direccionMovimiento.y -= gravedad * Time.deltaTime;

        // Movemos finalmente al contenedor
        infoControlador.Move(direccionMovimiento * Time.deltaTime);


        // Cambio de cámara (tecla C en este caso)
        if (Input.GetKeyDown(KeyCode.C))
        {
            camara3ra.SetActive(!camara3ra.activeSelf); //Se prende una y apaga la otra, un circuito de interruptor por ejemplo
            camara1ra.SetActive(!camara1ra.activeSelf);
        }
    }
}
