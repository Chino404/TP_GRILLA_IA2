﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Security.Principal;

public class SpatialGrid : MonoBehaviour
{
    [SerializeField]private float _time = 2;
    public Hunter hunter;
    public GameObject prefabBoid;
    public List<GridEntity> boidsList = new ();

    [Range(0, 4f)]
    public float weightSeparation, weightAlignment, weightCohesion; //El peso que va a tener cada metodo. Cual quiero que sea mas prioritario

    #region Variables
    public static SpatialGrid Instance;

    //punto de inicio de la grilla en X
    public float x;
    //punto de inicio de la grilla en Z
    public float z;
    //ancho de las celdas
    public float cellWidth;
    //alto de las celdas
    public float cellHeight;
    //cantidad de columnas (el "ancho" de la grilla)
    public int width;
    //cantidad de filas (el "alto" de la grilla)
    public int height;

    //ultimas posiciones conocidas de los elementos, guardadas para comparación.
    private Dictionary<GridEntity, Tuple<int, int>> lastPositions;
    //los "contenedores"
    private HashSet<GridEntity>[,] buckets;

    //el valor de posicion que tienen los elementos cuando no estan en la zona de la grilla.
    /*
     Const es implicitamente statica
     const tengo que ponerle el valor apenas la declaro, readonly puedo hacerlo en el constructor.
     Const solo sirve para tipos de dato primitivos.
     */
    readonly public Tuple<int, int> Outside = Tuple.Create(-1, -1);

    //Una colección vacía a devolver en las queries si no hay nada que devolver
    readonly public GridEntity[] Empty = new GridEntity[0];
    #endregion

    #region FUNCIONES
    private void Awake()
    {
        Instance = this;

        lastPositions = new Dictionary<GridEntity, Tuple<int, int>>();
        buckets = new HashSet<GridEntity>[width, height];

        //creamos todos los hashsets
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                buckets[i, j] = new HashSet<GridEntity>();


        //LOS AGENTES SE TIENEN Q DESTRUIR E INSTANCIAR PERO SIN HACERLO COMO ESTA ABAJO

        //P/alumnos: por que no usamos OfType<>() despues del RecursiveWalker() aca?
        //var ents = RecursiveWalker(transform)
        //    .Select(x => x.GetComponent<GridEntity>())
        //    .Where(x => x != null);

        //foreach (var e in ents)
        //{
        //    e.OnMove += UpdateEntity;
        //    e.OnDestroyEvent += RemoveEntity;
        //    UpdateEntity(e);
        //}
    }

    private void Start()
    {
        if (!prefabBoid) Debug.LogError("El prefab del Boid no está asignado en SpatialGrid. Asignalo desde el Inspector.");
        else InitialBoids();
    }

    private void Update()
    {
        //if (boidsList.Count() < 90) SpawnInitialBoids(1);

        foreach (var elem in boidsList)
        {
            UpdateEntity(elem);
        }
    }

    private void InitialBoids()
    {
        for (int i = 0; i < 90; i++)
        {
            //Escojo un punto Random de la grilla
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(x, x + width * cellWidth), 0, UnityEngine.Random.Range(z, z + height * cellHeight));

            //Lo isntancio en ese punto random
            GameObject newBoid = Instantiate(prefabBoid, spawnPosition, Quaternion.identity);
            newBoid.transform.parent = transform;

            //Me guardo en una variable local el componente GirdEntity del boid
            var gridEntity = newBoid.GetComponent<GridEntity>();

            //Si no lo tiene se lo agrego
            if (!gridEntity) gridEntity = newBoid.AddComponent<GridEntity>();

            gridEntity.OnMove += UpdateEntity;
            gridEntity.OnDestroyEvent += RemoveEntity;
            gridEntity.OnDestroyEvent += SpawnBoid;
            //UpdateEntity(gridEntity);
        }
    }

    public void SpawnBoid(GridEntity entity) => StartCoroutine(TimeSpawnBoids(entity));

    IEnumerator TimeSpawnBoids(GridEntity entity)
    {
        yield return new WaitForSeconds(_time);

        //Escojo un punto Random de la grilla
        Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(x, x + width * cellWidth), 0, UnityEngine.Random.Range(z, z + height * cellHeight));

        //Lo isntancio en ese punto random
        GameObject newBoid = Instantiate(prefabBoid, spawnPosition, Quaternion.identity);
        newBoid.transform.parent = transform;

        //Me guardo en una variable local el componente GirdEntity del boid
        entity = newBoid.GetComponent<GridEntity>();

        //Si no lo tiene se lo agrego
        if (!entity) entity = newBoid.AddComponent<GridEntity>();

        entity.OnMove += UpdateEntity;
        entity.OnDestroyEvent += RemoveEntity;
        entity.OnDestroyEvent += SpawnBoid;
        //UpdateEntity(gridEntity);
    }

    public Vector3 ApplyBounds(Vector3 pos)
    {
        //Me teletransporta al otro lado, el opuesto
        if (pos.x > width * cellWidth) pos.x = 0;
        if (pos.x < -0.5f) pos.x = width * cellWidth;
        if (pos.z > height * cellHeight) pos.z = 0;
        if (pos.z < -0.5f) pos.z = height * cellHeight;

        return pos;
    }

    public void UpdateEntity(GridEntity entity)
    {
        //Si tenia una posicion antes en la grilla
        var lastPos = lastPositions.ContainsKey(entity) ? lastPositions[entity] : Outside;
        //La posicion actual en donde estaria en la grilla
        var currentPos = GetPositionInGrid(entity.gameObject.transform.position);

        //Misma posición, no necesito hacer nada
        if (lastPos.Equals(currentPos))
            return;

        //Lo "sacamos" de la posición anterior
        if (IsInsideGrid(lastPos))
            buckets[lastPos.Item1, lastPos.Item2].Remove(entity);

        //Lo "metemos" a la celda nueva, o lo sacamos si salio de la grilla
        if (IsInsideGrid(currentPos))
        {
            buckets[currentPos.Item1, currentPos.Item2].Add(entity);
            lastPositions[entity] = currentPos;
        }
        else
            lastPositions.Remove(entity);
    }

    public void RemoveEntity(GridEntity entity)
    {
        //Si tenia una posicion antes en la grilla
        var lastPos = lastPositions.ContainsKey(entity) ? lastPositions[entity] : Outside;

        //Si estoy dentro de la grilla, Lo "sacamos" de la posición anterior
        if (IsInsideGrid(lastPos))
        {
            buckets[lastPos.Item1, lastPos.Item2].Remove(entity);

            //Lo quito de las ultimas posiciones
            lastPositions.Remove(entity);
        }

    }

    public IEnumerable<GridEntity> Query(Vector3 aabbFrom, Vector3 aabbTo, Func<Vector3, bool> filterByPosition)
    {
        var from = new Vector3(Mathf.Min(aabbFrom.x, aabbTo.x), 0, Mathf.Min(aabbFrom.z, aabbTo.z));
        var to = new Vector3(Mathf.Max(aabbFrom.x, aabbTo.x), 0, Mathf.Max(aabbFrom.z, aabbTo.z));

        var fromCoord = GetPositionInGrid(from);
        var toCoord = GetPositionInGrid(to);

        //¡Ojo que clampea a 0,0 el Outside! TODO: Checkear cuando descartar el query si estan del mismo lado
        fromCoord = Tuple.Create(Utility.Clampi(fromCoord.Item1, 0, width), Utility.Clampi(fromCoord.Item2, 0, height));
        toCoord = Tuple.Create(Utility.Clampi(toCoord.Item1, 0, width), Utility.Clampi(toCoord.Item2, 0, height));

        if (!IsInsideGrid(fromCoord) && !IsInsideGrid(toCoord))
            return Empty;
        
        // Creamos tuplas de cada celda
        var cols = Generate(fromCoord.Item1, x => x + 1)
            .TakeWhile(x => x < width && x <= toCoord.Item1);

        var rows = Generate(fromCoord.Item2, y => y + 1)
            .TakeWhile(y => y < height && y <= toCoord.Item2);

        var cells = cols.SelectMany(
            col => rows.Select(
                row => Tuple.Create(col, row)
            )
        );

        // Iteramos las que queden dentro del criterio
        return cells
            .SelectMany(cell => buckets[cell.Item1, cell.Item2])
            .Where(e =>
                from.x <= e.transform.position.x && e.transform.position.x <= to.x &&
                from.z <= e.transform.position.z && e.transform.position.z <= to.z
            ).Where(x => filterByPosition(x.transform.position));
    }

    public IEnumerable<GridEntity> Query(Vector3 aabbFrom, Vector3 aabbTo) => Query(aabbFrom, aabbTo, x => true);

    public Tuple<int, int> GetPositionInGrid(Vector3 pos)
    {
        //quita la diferencia, divide segun las celdas y floorea
        return Tuple.Create(Mathf.FloorToInt((pos.x - x) / cellWidth),
                            Mathf.FloorToInt((pos.z - z) / cellHeight));
    }

    public bool IsInsideGrid(Tuple<int, int> position)
    {
        //si es menor a 0 o mayor a width o height, no esta dentro de la grilla
        return 0 <= position.Item1 && position.Item1 < width &&
            0 <= position.Item2 && position.Item2 < height;
    }

    void OnDestroy()
    {
        var ents = RecursiveWalker(transform).Select(x => x.GetComponent<GridEntity>()).Where(x => x != null);
        foreach (var e in ents)
            e.OnMove -= UpdateEntity;
    }

    #region GENERATORS
    private static IEnumerable<Transform> RecursiveWalker(Transform parent)
    {
        foreach (Transform child in parent)
        {
            foreach (Transform grandchild in RecursiveWalker(child))
                yield return grandchild;
            yield return child;
        }
    }

    IEnumerable<T> Generate<T>(T seed, Func<T, T> mutate)
    {
        T accum = seed;
        while (true)
        {
            yield return accum;
            accum = mutate(accum);
        }
    }
    #endregion

    #endregion

    #region GRAPHIC REPRESENTATION
    public bool AreGizmosShutDown;
    public bool activatedGrid;
    public bool showLogs = true;
    private void OnDrawGizmos()
    {
        var rows = Generate(z, curr => curr + cellHeight)
                .Select(row => Tuple.Create(new Vector3(x, 0, row),
                                            new Vector3(x + cellWidth * width, 0, row)));

        //equivalente de rows
        /*for (int i = 0; i <= height; i++)
        {
            Gizmos.DrawLine(new Vector3(x, 0, z + cellHeight * i), new Vector3(x + cellWidth * width,0, z + cellHeight * i));
        }*/

        var cols = Generate(x, curr => curr + cellWidth)
                   .Select(col => Tuple.Create(new Vector3(col, 0, z), new Vector3(col, 0, z + cellHeight * height)));

        var allLines = rows.Take(width + 1).Concat(cols.Take(height + 1));

        foreach (var elem in allLines)
        {
            Gizmos.DrawLine(elem.Item1, elem.Item2);
        }

        if (buckets == null || AreGizmosShutDown) return;

        var originalCol = GUI.color;
        GUI.color = Color.red;
        if (!activatedGrid)
        {
            IEnumerable<GridEntity> allElems = Enumerable.Empty<GridEntity>();
            foreach(var elem in buckets)
                allElems = allElems.Concat(elem);

            int connections = 0;
            foreach (var ent in allElems)
            {
                foreach(var neighbour in allElems.Where(x => x != ent))
                {
                    Gizmos.DrawLine(ent.transform.position, neighbour.transform.position);
                    connections++;
                }
                if(showLogs)
                    Debug.Log("tengo " + connections + " conexiones por individuo");
                connections = 0;
            }
        }
        else
        {
            int connections = 0;
            foreach (var elem in buckets)
            {
                foreach(var ent in elem)
                {
                    foreach (var n in elem.Where(x => x != ent))
                    {
                        Gizmos.DrawLine(ent.transform.position, n.transform.position);
                        connections++;
                    }
                    if(showLogs)
                        Debug.Log("tengo " + connections + " conexiones por individuo");
                    connections = 0;
                }
            }
        }

        GUI.color = originalCol;
        showLogs = false;
    }
    #endregion
}
