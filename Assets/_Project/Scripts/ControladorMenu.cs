using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ControladorMenu : MonoBehaviour
{
    [SerializeField] private GameObject panelInfo;

    [Header("Configuración de Video")]
    [SerializeField] private GameObject panelVideo;
    [SerializeField] private VideoPlayer videoPlayer;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        if (panelInfo != null)
        {
            panelInfo.SetActive(false);
        }

        // --- AUTO-DETECTAR SI FALTAN LAS REFERENCIAS ---
        if (panelVideo == null)
        {
            // Busca automáticamente un objeto llamado "PanelVideo" en la escena
            GameObject encontrado = GameObject.Find("PanelVideo");
            if (encontrado != null) panelVideo = encontrado;
        }

        if (videoPlayer == null && panelVideo != null)
        {
            // Busca automáticamente el VideoPlayer dentro del panel
            videoPlayer = panelVideo.GetComponent<VideoPlayer>();
        }
        // ----------------------------------------------

        // Nos aseguramos de que el panel del video esté apagado al inicio
        if (panelVideo != null)
        {
            panelVideo.SetActive(false);
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += AlTerminarVideo;
        }
    }

    public void IniciarConVideo()
    {
        if (videoPlayer != null && panelVideo != null)
        {
            panelVideo.SetActive(true);
            videoPlayer.Prepare(); // Carga el video en memoria
            videoPlayer.prepareCompleted += (source) => {
                videoPlayer.Play(); // Arranca fluido una vez preparado
            };
        }
    }

    private void AlTerminarVideo(VideoPlayer vp)
    {
        CambiarAEscenaJuego();
    }

    public void CambiarAEscenaJuego()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void AbrirVentanaInfo()
    {
        if (panelInfo != null) panelInfo.SetActive(true);
    }

    public void CerrarVentanaInfo()
    {
        if (panelInfo != null) panelInfo.SetActive(false);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }
}