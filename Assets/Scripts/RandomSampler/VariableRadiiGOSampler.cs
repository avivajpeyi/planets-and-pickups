using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableRadiiGOSampler : MonoBehaviour
{
    public int seed;
    public float maxRadius = 1f,minRadius = 0.5f;
    public float regionSize = 10f;
    public int iterationsPerCell = 10;

    public bool debugLevel = false;
    
    Color LightGray =  new Color(220, 220,220); 

    private List<VariableRadiiDiskSampler.Point> points;

    [SerializeField] private GameObject goPrefab;
    private List<GameObject> gos;

    void OnValidate()
    {
        
        points = VariableRadiiDiskSampler.GeneratePoints(seed, maxRadius, minRadius, regionSize, iterationsPerCell);
    }

    void Start()
    {
        gos = new List<GameObject>();
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
        foreach (var pt in points)
        {
            GameObject newG = Instantiate(goPrefab);
            newG.transform.position = new Vector3(pt.x, pt.y, 0);
            newG.transform.localScale = Vector3.one * pt.radius;
            gos.Add(newG);
        }
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


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(regionSize/2,regionSize/2,0.5f),new Vector3(regionSize,regionSize,1f));
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
        Gizmos.DrawWireSphere(
            new Vector3(regionSize/2,regionSize/2,0.5f),
            regionSize/2
        );
        
        
    }
}
