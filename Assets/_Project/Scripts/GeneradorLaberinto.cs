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

    [Header("Generador de Loot")]
    [SerializeField] private GeneradorLoot generadorLoot;

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
        int intentosMaximos = 10;
        int intentoActual = 0;
        bool esValido = false;

        while (!esValido && intentoActual < intentosMaximos)
        {
            intentoActual++;

            // 1. Limpiar instanciación previa
            Transform previo = transform.Find("Laberinto_Instanciado");
            if (previo != null) DestroyImmediate(previo.gameObject);

            Transform contenedorMapa = new GameObject("Laberinto_Instanciado").transform;
            contenedorMapa.SetParent(this.transform);

            grillaLogica = new CeldaLogica[filas, columnas];
            matrizInstancias = new ModuloLaberinto[filas, columnas];

            // 2. Crear los caminos lógicos usando DFS
            GenerarCaminosDFS(0, 0);

            AbrirCrucesDeCuatroSalidas(2);

            // 3. Colocar los Prefabs
            InstanciarPrefabs(contenedorMapa);

            // 4. Validar si los prefabs colocados coinciden con la grilla
            esValido = ValidarConectividadTotal();

            if (!esValido)
            {
                Debug.LogWarning($"[Intento {intentoActual}] Se detectaron salas inaccesibles. Reintentando...");
            }
        }

        if (!esValido)
        {
            Debug.LogError("No se pudo generar un laberinto válido tras varios intentos. Revisa las aperturas de tus Prefabs.");
            return;
        }

        // 5. Hornear NavMesh solo si el laberinto es válido
        NavMeshSurface navMesh = GetComponent<NavMeshSurface>();
        if (navMesh == null)
        {
            navMesh = gameObject.AddComponent<NavMeshSurface>();
        }

        navMesh.collectObjects = CollectObjects.Children;
        navMesh.RemoveData();
        navMesh.BuildNavMesh();
        if (generadorLoot != null)
        {
            generadorLoot.GenerarLootEnEscena();
        }
        Debug.Log("<color=green>¡NavMesh horneado con éxito!</color>");
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

 public bool ValidarConectividadTotal()
    {
        int totalCeldas = filas * columnas;
        HashSet<Vector2Int> visitadas = new HashSet<Vector2Int>();
        Queue<Vector2Int> porVisitar = new Queue<Vector2Int>();

        // Empezamos desde la entrada (0,0)
        Vector2Int inicio = new Vector2Int(0, 0);
        porVisitar.Enqueue(inicio);
        visitadas.Add(inicio);

        while (porVisitar.Count > 0)
        {
            Vector2Int actual = porVisitar.Dequeue();
            // CORREGIDO: Usamos matrizInstancias en lugar de mapa
            ModuloLaberinto moduloActual = matrizInstancias[actual.x, actual.y];

            // Vector2Int.up incrementa Y (que en el mundo 3D equivale a avanzar en Z / Norte)
            Vector2Int[] direcciones = { Vector2Int.up, Vector2Int.down, Vector2Int.right, Vector2Int.left };

            foreach (var dir in direcciones)
            {
                Vector2Int vecino = actual + dir;

                // Validar límites del mapa
                if (vecino.x >= 0 && vecino.x < filas && vecino.y >= 0 && vecino.y < columnas)
                {
                    if (!visitadas.Contains(vecino))
                    {
                        // CORREGIDO: Usamos matrizInstancias en lugar de mapa
                        ModuloLaberinto moduloVecino = matrizInstancias[vecino.x, vecino.y];

                        // Comprobar si hay paso entre el módulo actual y el vecino
                        if (HayConexionValida(moduloActual, moduloVecino, dir))
                        {
                            visitadas.Add(vecino);
                            porVisitar.Enqueue(vecino);
                        }
                    }
                }
            }
        }

        bool todoConectado = (visitadas.Count == totalCeldas);

        if (todoConectado)
        {
            Debug.Log($"<color=green>✓ LABERINTO VÁLIDO:</color> Las {totalCeldas} celdas son accesibles.");
        }
        else
        {
            Debug.LogWarning($"<color=red>✗ ALERTA DE SALA CERRADA:</color> Solo se puede acceder a {visitadas.Count} de {totalCeldas} celdas.");
        }

        return todoConectado;
    }

private bool HayConexionValida(ModuloLaberinto origen, ModuloLaberinto destino, Vector2Int direccion)
    {
        if (origen == null || destino == null) return false;

        // Evaluamos si el módulo origen tiene salida hacia esa dirección considerando su rotación
        bool origenTieneSalida = EvaluarSalida(origen, (int)origen.transform.eulerAngles.y, DireccionAVector(direccion));
        bool destinoTieneSalida = EvaluarSalida(destino, (int)destino.transform.eulerAngles.y, DireccionAVector(-direccion));

        return origenTieneSalida && destinoTieneSalida;
    }

    private Vector3 DireccionAVector(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return Vector3.forward;
        if (dir == Vector2Int.down) return Vector3.back;
        if (dir == Vector2Int.right) return Vector3.right;
        if (dir == Vector2Int.left) return Vector3.left;
        return Vector3.zero;
    }
private void AbrirCrucesDeCuatroSalidas(int cantidadDeseada)
{
    List<Vector2Int> candidatosCruce = new List<Vector2Int>();

    // 1. Buscar celdas interiores que no estén en el borde y tengan espacio en los 4 lados
    for (int x = 1; x < filas - 1; x++)
    {
        for (int z = 1; z < columnas - 1; z++)
        {
            // Contamos cuántas conexiones activas tiene actualmente la celda
            int conexiones = 0;
            if (grillaLogica[x, z].norte) conexiones++;
            if (grillaLogica[x, z].sur) conexiones++;
            if (grillaLogica[x, z].este) conexiones++;
            if (grillaLogica[x, z].oeste) conexiones++;

            // Si ya tiene 2 o 3 salidas, es un excelente candidato para convertirse en un cruce de 4
            if (conexiones >= 2)
            {
                candidatosCruce.Add(new Vector2Int(x, z));
            }
        }
    }

    // 2. Mezclar la lista de candidatos
    DesordenarLista(candidatosCruce);

    int crucesCreados = 0;

    // 3. Convertir solo la cantidad especificada en cruces de 4 salidas
    foreach (Vector2Int pos in candidatosCruce)
    {
        if (crucesCreados >= cantidadDeseada) break;

        int x = pos.x;
        int z = pos.y;

        // Abrimos las 4 salidas en la celda lógica
        grillaLogica[x, z].norte = true;
        grillaLogica[x, z].sur = true;
        grillaLogica[x, z].este = true;
        grillaLogica[x, z].oeste = true;

        // También debemos abrir la conexión correspondiente en sus 4 vecinos
        grillaLogica[x, z + 1].sur = true; // Vecino Norte
        grillaLogica[x, z - 1].norte = true; // Vecino Sur
        grillaLogica[x + 1, z].oeste = true; // Vecino Este
        grillaLogica[x - 1, z].este = true;  // Vecino Oeste

        crucesCreados++;
    }
}
}