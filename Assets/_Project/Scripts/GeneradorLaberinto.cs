using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;


public class GeneradorLaberinto : MonoBehaviour
{
    [Header("Tamaño de la Grilla")]
    [SerializeField] private int filas = 6;
    [SerializeField] private int columnas = 6;
    [SerializeField] private float tamanoCelda = 8.0f;

    [Header("Catálogo de Módulos")]
    [SerializeField] private GameObject prefabEntrada;
    [SerializeField] private GameObject prefabSalida;
    [SerializeField] private GameObject[] prefabsCaminos;

    // Estructura lógica del laberinto (0: Sin celda, Bitmask para N, S, E, O)
    private struct CeldaLogica
    {
        public bool norte, sur, este, oeste;
        public bool visitada;
    }

    private CeldaLogica[,] grillaLogica;
    private ModuloLaberinto[,] matrizInstancias;

    void Start()
    {
        GenerarLaberintoGarantizado();
    }

    [ContextMenu("Re-Generar Laberinto")]
    public void GenerarLaberintoGarantizado()
    {
        // 1. Limpiar instanciación previa
        Transform previo = transform.Find("Laberinto_Instanciado");
        if (previo != null) DestroyImmediate(previo.gameObject);

        Transform contenedorMapa = new GameObject("Laberinto_Instanciado").transform;
        contenedorMapa.SetParent(this.transform);

        grillaLogica = new CeldaLogica[filas, columnas];
        matrizInstancias = new ModuloLaberinto[filas, columnas];

        // 2. Crear los caminos lógicos usando DFS (Garantiza conectividad 100% sin salas cerradas)
        GenerarCaminosDFS(0, 0);

        // 3. Colocar los Prefabs correspondientes a la estructura lógica
        InstanciarPrefabs(contenedorMapa);

        NavMeshSurface navMesh = GetComponent<NavMeshSurface>();
    if (navMesh == null)
    {
    navMesh = gameObject.AddComponent<NavMeshSurface>();
    }

    // Configurar para escanear SOLAMENTE a los hijos de este GameObject
    navMesh.collectObjects = CollectObjects.Children;

    // Limpiar datos previos si existían
    navMesh.RemoveData();

    // Hornear la malla dinámicamente
    navMesh.BuildNavMesh();

    Debug.Log("¡NavMesh delimitado y horneado correctamente!");
    }

    // Algoritmo DFS (Depth-First Search) para laberintos perfectos
    private void GenerarCaminosDFS(int x, int z)
    {
        grillaLogica[x, z].visitada = true;

        // Lista de vecinos posibles (Norte, Sur, Este, Oeste)
        List<Vector2Int> vecinos = new List<Vector2Int>
        {
            new Vector2Int(x, z + 1), // Norte
            new Vector2Int(x, z - 1), // Sur
            new Vector2Int(x + 1, z), // Este
            new Vector2Int(x - 1, z)  // Oeste
        };

        DesordenarLista(vecinos);

        foreach (Vector2Int vecino in vecinos)
        {
            int nx = vecino.x;
            int nz = vecino.y;

            // Comprobar que esté dentro del mapa y no haya sido visitado
            if (nx >= 0 && nx < filas && nz >= 0 && nz < columnas && !grillaLogica[nx, nz].visitada)
            {
                // Conectar celda actual con el vecino
                if (nx > x) { grillaLogica[x, z].este = true; grillaLogica[nx, nz].oeste = true; }
                else if (nx < x) { grillaLogica[x, z].oeste = true; grillaLogica[nx, nz].este = true; }
                else if (nz > z) { grillaLogica[x, z].norte = true; grillaLogica[nx, nz].sur = true; }
                else if (nz < z) { grillaLogica[x, z].sur = true; grillaLogica[nx, nz].norte = true; }

                GenerarCaminosDFS(nx, nz);
            }
        }
    }

    private void InstanciarPrefabs(Transform contenedor)
    {
        for (int x = 0; x < filas; x++)
        {
            for (int z = 0; z < columnas; z++)
            {
                Vector3 posicionCelda = new Vector3(x * tamanoCelda, 0, z * tamanoCelda);
                CeldaLogica logica = grillaLogica[x, z];

                GameObject prefabElegido = null;
                Quaternion rotacionElegida = Quaternion.identity;

                // Caso Especial: Entrada (0,0)
                if (x == 0 && z == 0)
                {
                    prefabElegido = prefabEntrada;
                    rotacionElegida = ObtenerRotacionEntradaSalida(prefabEntrada, logica);
                }
                // Caso Especial: Salida (Última celda)
                else if (x == filas - 1 && z == columnas - 1)
                {
                    prefabElegido = prefabSalida;
                    rotacionElegida = ObtenerRotacionEntradaSalida(prefabSalida, logica);
                }
                // Celdas normales del mapa
                else
                {
                    prefabElegido = SeleccionarPrefabYRotacion(logica, out rotacionElegida);
                }

                GameObject nuevoObj = Instantiate(prefabElegido, posicionCelda, rotacionElegida, contenedor);
                matrizInstancias[x, z] = nuevoObj.GetComponent<ModuloLaberinto>();
            }
        }
    }

    private GameObject SeleccionarPrefabYRotacion(CeldaLogica logica, out Quaternion rotacionFinal)
    {
        List<GameObject> candidatos = new List<GameObject>(prefabsCaminos);
        DesordenarLista(candidatos);

        foreach (GameObject prefab in candidatos)
        {
            ModuloLaberinto mod = prefab.GetComponent<ModuloLaberinto>();
            if (mod == null) continue;

            int[] angulos = { 0, 90, 180, 270 };
            foreach (int angulo in angulos)
            {
                if (EsRotacionCompatible(mod, angulo, logica))
                {
                    rotacionFinal = Quaternion.Euler(0, angulo, 0);
                    return prefab;
                }
            }
        }

        rotacionFinal = Quaternion.identity;
        return prefabsCaminos[0];
    }

    private Quaternion ObtenerRotacionEntradaSalida(GameObject prefab, CeldaLogica logica)
{
    ModuloLaberinto mod = prefab.GetComponent<ModuloLaberinto>();
    if (mod == null) return Quaternion.identity;

    // Evaluamos las 4 rotaciones posibles (0, 90, 180, 270 grados)
    int[] angulos = { 0, 90, 180, 270 };

    foreach (int angulo in angulos)
    {
        // Calculamos hacia dónde apunta la salida del prefab tras aplicar esta rotación
        bool tieneNorte = EvaluarSalida(mod, angulo, Vector3.forward);
        bool tieneSur   = EvaluarSalida(mod, angulo, Vector3.back);
        bool tieneEste  = EvaluarSalida(mod, angulo, Vector3.right);
        bool tieneOeste = EvaluarSalida(mod, angulo, Vector3.left);

        // La rotación es válida si coincide EXACTAMENTE con las conexiones lógicas del laberinto
        if (tieneNorte == logica.norte &&
            tieneSur   == logica.sur &&
            tieneEste  == logica.este &&
            tieneOeste == logica.oeste)
        {
            return Quaternion.Euler(0, angulo, 0);
        }
    }

    // Si la pieza solo tiene 1 apertura (Norte) y la conexión viene del Sur, forzamos 180°
    if (logica.sur) return Quaternion.Euler(0, 180, 0);
    if (logica.oeste) return Quaternion.Euler(0, 270, 0);
    if (logica.norte) return Quaternion.Euler(0, 0, 0);
    if (logica.este) return Quaternion.Euler(0, 90, 0);

    return Quaternion.identity;
}

    private bool EsRotacionCompatible(ModuloLaberinto mod, int angulo, CeldaLogica logica)
    {
        bool tieneNorte = EvaluarSalida(mod, angulo, Vector3.forward);
        bool tieneSur   = EvaluarSalida(mod, angulo, Vector3.back);
        bool tieneEste  = EvaluarSalida(mod, angulo, Vector3.right);
        bool tieneOeste = EvaluarSalida(mod, angulo, Vector3.left);

        return (tieneNorte == logica.norte) &&
               (tieneSur   == logica.sur) &&
               (tieneEste  == logica.este) &&
               (tieneOeste == logica.oeste);
    }

    private bool EvaluarSalida(ModuloLaberinto mod, int angulo, Vector3 direccionDeseada)
    {
        Vector3 dirLocal = Quaternion.Inverse(Quaternion.Euler(0, angulo, 0)) * direccionDeseada;

        if (Vector3.Dot(dirLocal, Vector3.forward) > 0.5f) return mod.norte;
        if (Vector3.Dot(dirLocal, Vector3.back) > 0.5f) return mod.sur;
        if (Vector3.Dot(dirLocal, Vector3.right) > 0.5f) return mod.este;
        if (Vector3.Dot(dirLocal, Vector3.left) > 0.5f) return mod.oeste;

        return false;
    }

    private void DesordenarLista<T>(List<T> lista)
    {
        for (int i = 0; i < lista.Count; i++)
        {
            T temp = lista[i];
            int randomIndex = Random.Range(i, lista.Count);
            lista[i] = lista[randomIndex];
            lista[randomIndex] = temp;
        }
    }
}