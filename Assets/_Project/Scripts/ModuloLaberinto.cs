using UnityEngine;

public class ModuloLaberinto : MonoBehaviour
{
    [Header("Conexiones Locales (Sin Rotar)")]
    public bool norte;
    public bool sur;
    public bool este;
    public bool oeste;

    // Devuelve si el módulo tiene apertura en la dirección consultada considerando su rotación actual
    public bool TieneSalida(Vector3 direccion)
    {
        // Convertimos la dirección global a la rotación local del módulo
        Vector3 dirLocal = Quaternion.Inverse(transform.rotation) * direccion;

        if (Vector3.Dot(dirLocal, Vector3.forward) > 0.5f) return norte;
        if (Vector3.Dot(dirLocal, Vector3.back) > 0.5f) return sur;
        if (Vector3.Dot(dirLocal, Vector3.right) > 0.5f) return este;
        if (Vector3.Dot(dirLocal, Vector3.left) > 0.5f) return oeste;

        return false;
    }
}

