using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("UI del Menú")]
    [SerializeField] private GameObject menuPausaUI;

    private MonoBehaviour scriptCamaraPlayer;

    private bool juegoPausado = false;

    void Update()
    {
        // Detectar si el jugador presiona ESCAPE
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Reanudar()
    {
        menuPausaUI.SetActive(false);
        Time.timeScale = 1f; // Devuelve el tiempo a velocidad normal
        juegoPausado = false;
        // Reorganizar el cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reactivar la cámara
        if (scriptCamaraPlayer != null)
        {
            scriptCamaraPlayer.enabled = true;
        }
    }

    public void Pausar()
    {

        if (scriptCamaraPlayer == null)
        {
            BuscarCamaraEnEscena();
        }
        menuPausaUI.SetActive(true);
        Time.timeScale = 0f; // Congela el juego (físicas, enemigos, temporizadores)
        juegoPausado = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (scriptCamaraPlayer != null)
        {
            scriptCamaraPlayer.enabled = false;
        }
    }

    private void BuscarCamaraEnEscena()
    {
        // Opción A: Buscar por el tag "MainCamera"
        Camera camaraPrincipal = Camera.main;
        if (camaraPrincipal != null)
        {
            // Reemplazá 'ControlCamara' por el nombre exacto de tu script de rotación del ratón
            scriptCamaraPlayer = camaraPrincipal.GetComponent<MonoBehaviour>(); 
        }

        // Opción B: Si el script de la cámara está en el objeto padre del Player
        if (scriptCamaraPlayer == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                // Busca cualquier script en el Player o en sus hijos (como el hijo Cámara)
                scriptCamaraPlayer = player.GetComponentInChildren<MonoBehaviour>();
            }
        }
    }

    public void CargarMenuPrincipal(string nombreEscenaMenu)
    {
        Time.timeScale = 1f; // IMPORTANTE: Descongelar el tiempo antes de cambiar de escena
        SceneManager.LoadScene(nombreEscenaMenu); // <-- Corregido aquí
    }
}
