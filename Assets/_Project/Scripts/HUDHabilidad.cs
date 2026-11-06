using UnityEngine;
using UnityEngine.UI;

public class HUDHabilidad : MonoBehaviour
{
    [Header("Sprites de la Habilidad")]
    [SerializeField] private Sprite spriteInactivo; // El sprite cuando está bloqueado/gris
    [SerializeField] private Sprite spriteActivo;

    private Image imagenHabilidad;
    private ControlarUsuario jugadorControl;

    void Start()
    {
        imagenHabilidad = GetComponent<Image>();
        BuscarJugador();

        // Arranca invisible o gris hasta que se compre
        if (imagenHabilidad != null && spriteInactivo != null)
        {
            imagenHabilidad.sprite = spriteInactivo;
            imagenHabilidad.color = Color.white; // Dejamos el color blanco para que se vea el sprite tal cual es
            imagenHabilidad.fillAmount = 1f;    // Lleno / Bloqueado
        }
    }

    void Update()
    {
        if (jugadorControl == null)
        {
            BuscarJugador();
            return; // Si sigue nulo, salimos para evitar errores en este frame
        }

        // Una vez que ya lo tenemos conectado, actualizamos la UI con normalidad
        if (jugadorControl.tienePoderCorrer)
        {
           if (imagenHabilidad.sprite != spriteActivo && spriteActivo != null)
            {
                imagenHabilidad.sprite = spriteActivo;
            }

            // Mantenemos el color blanco para respetar los colores originales del sprite activo
            imagenHabilidad.color = Color.white; 

            float cooldown = jugadorControl.ObtenerPorcentajeCooldown();

            // Opcional de seguridad: si el cooldown da un valor extraño o negativo, lo fijamos en 1 (lleno)
            imagenHabilidad.fillAmount = Mathf.Clamp(cooldown, 0f, 1f);
        }
        else
        {
            if (imagenHabilidad.sprite != spriteInactivo && spriteInactivo != null)
            {
                imagenHabilidad.sprite = spriteInactivo;
            }

            imagenHabilidad.color = Color.white;
            imagenHabilidad.fillAmount = 1f;
        }
    }

    void BuscarJugador()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador != null)
        {
            jugadorControl = jugador.GetComponent<ControlarUsuario>();
            if (jugadorControl != null)
            {
                Debug.Log("HUD: ¡Jugador vinculado correctamente tras la instanciación!");
            }
        }
    }
}