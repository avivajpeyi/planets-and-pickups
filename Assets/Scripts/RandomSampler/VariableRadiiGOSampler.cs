using System;
using System.Collections;
using System.Collections.Generic;
using MiniPlanetDefense;
using UnityEngine;
using Zenject;
using Zchfvy.Plus;

public class VariableRadiiGOSampler : MonoBehaviour
{
    public bool useRandomSeed = true;
    public int seed;
    public float maxRadius = 1f,minRadius = 0.5f;
    public float regionSize = 10f;
    public int iterationsPerCell = 10;

    public bool debugLevel = false;
    
    Color LightGray =  new Color(220, 220,220); 

    private List<VariableRadiiDiskSampler.Point> points;

    [Inject] private GameArea gameArea;
    [Inject] private Player player;
    private Camera cam;
    [SerializeField] private GameObject goPrefab;
    private List<GameObject> gos;

    private Vector3 center;

    
    
    void OnValidate()
    {
        
        points = VariableRadiiDiskSampler.GeneratePoints(seed, maxRadius, minRadius, regionSize, iterationsPerCell);
    }

    void Start()
    {
        if (useRandomSeed)
            seed = (int) DateTime.Now.Ticks;
        gos = new List<GameObject>();
        center = new Vector3(regionSize / 2, regionSize / 2, 0.5f);
        cam = Camera.main;
        Reset();
        
    }

    private void Reset()
    {
        points = VariableRadiiDiskSampler.GeneratePoints(seed, maxRadius, minRadius, regionSize, iterationsPerCell);

        if (gos.Count > 0)
        {
            foreach (var g in gos)
            {
                Destroy(g);
            }
            gos.Clear();
        }
        for (int i = 0; i < points.Count; i++)
        {
            var pt = points[i];
            GameObject newG = Instantiate(goPrefab);
            Planet p = newG.GetComponent<Planet>();
            newG.transform.position = new Vector3(pt.x, pt.y, 0);
            float diameter = pt.radius * 2.0f;
            newG.transform.localScale = Vector3.one * diameter * 0.5f; // making the planets much smaller than the poisson disk
            p.InitPlanet(newG.transform.position, newG.transform.localScale.x / 2f);
            gos.Add(newG);
            
        }
        
        
        
        gameArea.InitGameArea(center);
        player.Reset( new Vector3(center.x, center.y, player.transform.position.z));
        cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
        
    }

    private void Update()
    {
        if (debugLevel && Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("New level");
            seed += 1;
            Reset();
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
        if (points != null)
        {
            foreach (VariableRadiiDiskSampler.Point point in points)
            {
                
                Color c = Color.Lerp(Color.gray, LightGray, (point.radius-minRadius)/(maxRadius-minRadius));
                c.a = 0.1f;
                Gizmos.color = c;
                Gizmos.DrawSphere(new Vector3(point.x,point.y,0.5f), point.radius);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(new Vector3(point.x,point.y,0.5f), 0.1f);
            }
        }
        
        Gizmos.color = Color.red;
//        GizmosPlus.Circle(center,  Vector3.back*regionSize/2);
//        Gizmos.DrawWireCube(center,new Vector3(regionSize,regionSize,1f));
        
        
    }
}
