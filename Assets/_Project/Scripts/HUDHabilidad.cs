using UnityEngine;
using UnityEngine.UI;

public class HUDHabilidad : MonoBehaviour
{
    private Image imagenHabilidad;
    private ControlarUsuario jugadorControl;

    void Start()
    {
        imagenHabilidad = GetComponent<Image>();
        BuscarJugador();

        // Arranca invisible o gris hasta que se compre
        if (imagenHabilidad != null)
        {
            imagenHabilidad.color = Color.gray; // Gris antes de comprar
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
            // Cambiamos a color blanco (o el color original de tu sprite)
            imagenHabilidad.color = Color.white; 

            // Actualizamos el círculo radial de recarga
            imagenHabilidad.fillAmount = jugadorControl.ObtenerPorcentajeCooldown();
        }
        else
        {
            // Si todavía no compró el poder en la estatua, se queda en gris
            imagenHabilidad.color = Color.gray;
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