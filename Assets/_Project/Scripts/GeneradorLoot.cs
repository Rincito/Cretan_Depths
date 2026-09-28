using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneradorLoot : MonoBehaviour
{
    [Header("Prefabs de Objetos")]
    [SerializeField] private GameObject prefabMoneda;
    [SerializeField] private GameObject prefabCofre;

    [Header("Cantidades a Generar")]
    [SerializeField] private int cantidadMonedas = 40;
    [SerializeField] private int cantidadCofres = 5;

    private void Start()
    {
        // Si tu mapa se genera por código en Start(), podés llamar a esto desde ahí
        // o usar una pequeña espera si los prefabs se instancian en el primer frame.
        GenerarLootEnEscena();
    }

    public void GenerarLootEnEscena()
    {
        // 1. Buscar todos los puntos de spawn instanciados en la escena
        PuntoDeSpawn[] todosLosPuntos = FindObjectsOfType<PuntoDeSpawn>();
        
        List<PuntoDeSpawn> puntosMonedas = new List<PuntoDeSpawn>();
        List<PuntoDeSpawn> puntosCofres = new List<PuntoDeSpawn>();
        List<PuntoDeSpawn> puntosComunes = new List<PuntoDeSpawn>();

        // 2. Clasificar los puntos encontrados
        foreach (PuntoDeSpawn p in todosLosPuntos)
        {
            if (p.tipo == TipoSpawn.Moneda) puntosMonedas.Add(p);
            else if (p.tipo == TipoSpawn.Cofre) puntosCofres.Add(p);
            else puntosComunes.Add(p);
        }

        // 3. Spawnear Cofres
        GenerarObjetos(prefabCofre, cantidadCofres, puntosCofres, puntosComunes);

        // 4. Spawnear Monedas
        GenerarObjetos(prefabMoneda, cantidadMonedas, puntosMonedas, puntosComunes);
    }

    private void GenerarObjetos(GameObject prefab, int cantidadDeseada, List<PuntoDeSpawn> listaEspecifica, List<PuntoDeSpawn> listaComun)
    {
        int generados = 0;

        while (generados < cantidadDeseada)
        {
            PuntoDeSpawn puntoElegido = null;

            // Primero intentamos usar la lista específica, si se agotan pasamos a la común
            if (listaEspecifica.Count > 0)
            {
                int indiceAleatorio = Random.Range(0, listaEspecifica.Count);
                puntoElegido = listaEspecifica[indiceAleatorio];
                listaEspecifica.RemoveAt(indiceAleatorio); // Lo eliminamos para no repetir punto
            }
            else if (listaComun.Count > 0)
            {
                int indiceAleatorio = Random.Range(0, listaComun.Count);
                puntoElegido = listaComun[indiceAleatorio];
                listaComun.RemoveAt(indiceAleatorio);
            }
            else
            {
                Debug.LogWarning($"Se acabaron los puntos de spawn disponibles para {prefab.name}. Se generaron {generados} de {cantidadDeseada}.");
                break; // No hay más puntos libres
            }

            // Instanciar el objeto en la posición y rotación del punto de spawn
            if (puntoElegido != null)
            {
                Instantiate(prefab, puntoElegido.transform.position, puntoElegido.transform.rotation);
                generados++;
            }
        }
    }
}