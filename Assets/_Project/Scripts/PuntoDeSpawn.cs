using UnityEngine;

public enum TipoSpawn { Moneda, Cofre, CualquierCosa }

public class PuntoDeSpawn : MonoBehaviour
{
    public TipoSpawn tipo = TipoSpawn.CualquierCosa;

    // Dibujamos una esferita en el editor para ver fácil los puntos en los prefabs
    private void OnDrawGizmos()
    {
        Gizmos.color = (tipo == TipoSpawn.Moneda) ? Color.yellow : 
                       (tipo == TipoSpawn.Cofre) ? Color.magenta : Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
