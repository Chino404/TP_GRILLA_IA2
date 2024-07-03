using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Security.Cryptography;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Hunter hunter;
    public GameObject boidPrefab;
    public SpatialGrid grid;
    public Transform spawnPoint;
    public float Boidscount;
    [Range(0,4f)]
    public float weightSeparation, weightAlignment, weightCohesion; //El peso que va a tener cada metodo. Cual quiero que sea mas prioritario

    private void Awake()
    {
        Instance = this;    
    }

    
}
