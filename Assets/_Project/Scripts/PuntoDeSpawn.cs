using UnityEngine;

public enum TipoSpawn { Moneda, Cofre, Estatua, CualquierCosa }

public class PuntoDeSpawn : MonoBehaviour
{
    public TipoSpawn tipo = TipoSpawn.CualquierCosa;

    // Dibujamos una esferita en el editor para ver fácil los puntos en los prefabs
    private void OnDrawGizmos()
    {
        if (tipo == TipoSpawn.Estatua)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.6f); // Esfera tipo wire para resaltar
        }
        else
        {
            Gizmos.color = (tipo == TipoSpawn.Moneda) ? Color.yellow : 
                           (tipo == TipoSpawn.Cofre) ? Color.magenta : Color.gray;
        }
        
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
