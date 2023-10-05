using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Rail : MonoBehaviour
{
    [Header("Curve")]
    [SerializeField] private List<RailPoint> railPoints;
    [SerializeField] private  float nbPoints;
    [SerializeField] private  float distBetweenNodes;
    private List<Vector3> pointsOnCurve =new List<Vector3>(0);
    public List<Vector3> distancedNodes = new List<Vector3>(0);
    [SerializeField] private bool loop;
    [SerializeField] private List<Transform> forms;

    [Header("Rail")] 
    [SerializeField] private Vector2[] railVertices;
    public MeshFilter meshFilter;
    public int nbRails;
    public float space;

    [Header("Plank")] 
    public GameObject[] plank;
    public Transform plankParent;
    public List<GameObject> planks;

    [Header("Wagon")] 
    public Transform wagon;
    public int previous;
    public int next;
    public float index;
    public float speed;

    [Header("Tool")] 
    [SerializeField] private bool generateRail;
    [SerializeField] private bool generatePlank;
    public bool addNewPoint;
    public bool removeLastPoint;
    [SerializeField] private GameObject railPoint;
    public bool modeForm;
    private bool form;
    [SerializeField] private GameObject formPoint;


    private void Start()
    {
        DrawRailPoints();
        CreateDistancedNodes();
        meshFilter.mesh = GenerateRail();
        GeneratePlank();
    }

    private void Update()
    {
        if (index < 1)
        {
            index +=( Time.deltaTime * speed )/ distBetweenNodes;
        }
        else
        {
            previous = next;
            next = (next + 1) % distancedNodes.Count;
            index = 0;
        }
        wagon.position = Vector3.Lerp(distancedNodes[previous], distancedNodes[next], index)+Vector3.up*1.2f;
        wagon.rotation = Quaternion.Lerp(wagon.rotation,Quaternion.LookRotation(distancedNodes[next] - distancedNodes[previous]),Time.deltaTime*5);
        
    }

    private void OnDrawGizmos()
    {
        DrawRailPoints();
        CreateDistancedNodes();
        foreach (Vector3 pos in distancedNodes)
        {
            Gizmos.DrawSphere(pos,0.1f);
        }

        if (generateRail)
        {
            meshFilter.mesh = GenerateRail();
            generateRail = false;
        }
        if (generatePlank)
        {
            GeneratePlank();
            generatePlank = false;
        }
        if (addNewPoint)
        {
            AddNewPoint();
            addNewPoint = false;
        }
        if (removeLastPoint)
        {
            RemoveLastPoint();
            removeLastPoint = false;
        }

        if (modeForm)
        {
            if (!form)
            {
                form = true;
                EnterFormMode();
            }

            FormMode();

        }
        else
        {
            if (form)
            {
                form = false;
                ExitFormMode();
            }
        }
    }

    void EnterFormMode()
    {
        for (int i = 0; i < railPoints.Count; i++)
        {
            GameObject obj = Instantiate(formPoint, railPoints[i].nextHandle.position, quaternion.identity, transform);
            obj.name = "PolygonEdge" + i;
            forms.Add(obj.transform);
        }
    }
    
    void ExitFormMode()
    {
        foreach (Transform obj in forms)
        {
            DestroyImmediate(obj.gameObject);
        }
        forms.Clear();
    }

    void FormMode()
    {
        for (int i = 0; i < railPoints.Count; i++)
        {
            railPoints[i].nextHandle.position = forms[i].position;
            if(i > 0)  railPoints[i].previousHandle.position = forms[i-1].position;
            else railPoints[i].previousHandle.position = forms[forms.Count-1].position;

            railPoints[i].point.position = (railPoints[i].nextHandle.position + railPoints[i].previousHandle.position) / 2;
        }
    }

    void AddNewPoint()
    {
        GameObject obj = Instantiate(railPoint, transform.position, quaternion.identity,transform);
        obj.name = "Point" + railPoints.Count;
        RailPoint newPoint = new RailPoint();
        newPoint.point = obj.transform;
        newPoint.previousHandle = obj.transform.GetChild(0);
        newPoint.nextHandle = obj.transform.GetChild(1);
        railPoints.Add(newPoint);
        if (modeForm)
        {
            GameObject objPoly = Instantiate(formPoint, railPoints[railPoints.Count-1].nextHandle.position, quaternion.identity, transform);
            objPoly.name = "PolygonEdge" + (railPoints.Count-1);
            forms.Add(objPoly.transform);
        }
    }
    
    void RemoveLastPoint()
    {
        DestroyImmediate(railPoints[railPoints.Count - 1].point.gameObject);
        railPoints.RemoveAt(railPoints.Count - 1);
    }

    void GeneratePlank()
    {
        foreach (GameObject obj in planks)
        { 
            DestroyImmediate(obj);
        }
        planks.Clear();
        
        for (int i = 0; i < distancedNodes.Count; i++)
        {
            Vector3 xAxis;
            if (i != distancedNodes.Count - 1)
            {
                xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[i + 1] - distancedNodes[i]);
            }
            else if (!loop)
            {
                xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[i] - distancedNodes[i-1]);
            }
            else
            {
                xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[0] - distancedNodes[i]);
            }
            planks.Add(Instantiate(plank[Random.Range(0,plank.Length)], distancedNodes[i], Quaternion.LookRotation(xAxis), plankParent));
            planks[planks.Count - 1].transform.localScale = new Vector3(
                planks[planks.Count - 1].transform.localScale.x * (Random.Range(0, 2) == 0 ? 1 : -1),
                planks[planks.Count - 1].transform.localScale.y, planks[planks.Count - 1].transform.localScale.z * Random.Range(1, 1.1f));
        }
    }

    Mesh GenerateRail()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>(0);
        List<int> triangles = new List<int>(0);
        int nbPt = 0;

        for (int x = 0; x < nbRails; x++)
        {
            for (int i = 0; i < distancedNodes.Count; i++)
            {
                Vector3 yAxis = Vector3.up;
                Vector3 xAxis;
                if (i != distancedNodes.Count - 1)
                {
                    xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[i + 1] - distancedNodes[i]);
                }
                else if (!loop)
                {
                    xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[i] - distancedNodes[i-1]);
                }
                else
                {
                    xAxis = Quaternion.Euler(0, 90, 0) * (distancedNodes[0] - distancedNodes[i]);
                }

                xAxis = xAxis.normalized;

                Vector3 pos = distancedNodes[i] + (xAxis * railVertices[0].x) + (-xAxis * (space * (nbRails-1 )/ 2) + xAxis * x * (space)) + yAxis * railVertices[0].y;
                pos = transform.InverseTransformPoint(pos);
                vertices.Add(pos);
                
                for (int j = 1; j < railVertices.Length; j++)
                {
                    pos = distancedNodes[i] + (xAxis * railVertices[j].x) + (-xAxis * (space * (nbRails-1 )/ 2) + xAxis * x * (space)) + yAxis * railVertices[j].y;
                    pos = transform.InverseTransformPoint(pos);
                    vertices.Add(pos);
                    vertices.Add(pos);
                }
                
                pos = distancedNodes[i] + (xAxis * railVertices[0].x) + (-xAxis * (space * (nbRails-1 )/ 2) + xAxis * x * (space)) + yAxis * railVertices[0].y;
                pos = transform.InverseTransformPoint(pos);
                vertices.Add(pos);
            }

            for (int i = 0; i < distancedNodes.Count-1; i++)
            {
                for (int j = 0; j < railVertices.Length; j++)
                {
                    int a = nbPt + (i * railVertices.Length*2) + j * 2;
                    int b = nbPt + (i * railVertices.Length*2) + (j*2+1) % (railVertices.Length * 2);
                    int c = nbPt + ((i+1) * railVertices.Length*2) + j * 2;
                    int d = nbPt + ((i+1) * railVertices.Length*2) + (j*2+1) % (railVertices.Length * 2);
                    triangles.AddRange(GetTrianglesForQuad(a,b,c,d));
                }
            }

            if (loop)
            {
                for (int j = 0; j < railVertices.Length; j++)
                {
                    int a = nbPt + (distancedNodes.Count-1) * railVertices.Length*2 + j*2;
                    int b = nbPt + (distancedNodes.Count-1) * railVertices.Length*2 + (j*2+1)% (railVertices.Length * 2);
                    int c = nbPt + j*2;
                    int d = nbPt + (j*2+1)%(railVertices.Length * 2); 
                    triangles.AddRange(GetTrianglesForQuad(a,b,c,d));
                }
            }

            nbPt = vertices.Count;
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            
            Debug.DrawRay(transform.position + mesh.vertices[i],mesh.normals[i],Color.green,10);
        }
        return mesh;
    }

    int[] GetTrianglesForQuad(int a,int b,int c,int d)
    {
        List<int> triangles = new List<int>(0);
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);
        triangles.Add(b);
        triangles.Add(d);
        triangles.Add(c);
        return triangles.ToArray();
    }
    
    void CreateDistancedNodes()
    {
        if (distBetweenNodes <= 0) return;
        distancedNodes.Clear();
        distancedNodes.Add(railPoints[0].point.position);
        float totalDist = 0;
        for (int i = 1; i < pointsOnCurve.Count; i++)
        {
            totalDist += Vector3.Distance(pointsOnCurve[i], pointsOnCurve[i - 1]);
        }
        int numberOfNodes =  Mathf.RoundToInt(totalDist / distBetweenNodes);
        float distNode = totalDist / numberOfNodes;
        numberOfNodes--;

        int index = 1;
        Vector3 current = pointsOnCurve[0];
        for (int i = 0; i < numberOfNodes; i++)
        {
            if (Vector3.SqrMagnitude(pointsOnCurve[index] - current) < distNode * distNode)
            {
                float dist = distNode - Vector3.Distance(pointsOnCurve[index], current);
                index++;
                for (int j = 0; j < 500; j++)
                {
                    if (Vector3.SqrMagnitude(pointsOnCurve[index] - pointsOnCurve[index - 1]) < dist * dist)
                    {
                        dist -= Vector3.Distance(pointsOnCurve[index], pointsOnCurve[index - 1]);
                        index++;
                    }
                    else
                    {
                        Vector3 pos = pointsOnCurve[index-1] + (pointsOnCurve[index] - pointsOnCurve[index-1]).normalized * dist;
                        distancedNodes.Add(pos);
                        current = pos;
                        break;
                    }
                }
            }
            else
            {
                Vector3 pos = current + (pointsOnCurve[index] - current).normalized * distBetweenNodes;
                distancedNodes.Add(pos);
                current = pos;
            }
        }
        if(!loop)distancedNodes.Add(railPoints[railPoints.Count-1].point.position);
    }
    
    private void DrawRailPoints()
    {
        pointsOnCurve.Clear();
        for (int i = 0; i < railPoints.Count-1; i++)
        {
            DrawPoints(railPoints[i].point.position,railPoints[i].nextHandle.position,railPoints[i+1].previousHandle.position,railPoints[i+1].point.position);
        }
        if (loop)
        {
            DrawPoints(railPoints[railPoints.Count-1].point.position,railPoints[railPoints.Count-1].nextHandle.position,railPoints[0].previousHandle.position,railPoints[0].point.position);
            pointsOnCurve.Add(railPoints[0].point.position);
        }
        else
        {
            pointsOnCurve.Add(railPoints[railPoints.Count-1].point.position);   
        }
    }

    void DrawPoints(Vector3 a,Vector3 b,Vector3 c,Vector3 d)
    {
        for (int i = 0; i < nbPoints; i++)
        {
            Vector3 pos = QuadraticLerp(a, b, c, d, (1 / nbPoints) * i);
            pointsOnCurve.Add(pos);
        }
    }

    Vector3 DoubleLerp(Vector3 a,Vector3 b,Vector3 c,float t)
    {
        Vector3 ab = Vector3.Lerp(a, b, t);
        Vector3 bc = Vector3.Lerp(b, c, t);
        Vector3 abc = Vector3.Lerp(ab, bc, t);
        return abc;
    }

    Vector3 QuadraticLerp(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t)
    {
        Vector3 abc = DoubleLerp(a, b, c, t);
        Vector3 bcd = DoubleLerp(b, c, d, t);
        Vector3 quadratic = Vector3.Lerp(abc, bcd, t);
        return quadratic;
    }
}

[Serializable]
public class RailPoint
{
    public Transform point;
    public Transform previousHandle;
    public Transform nextHandle;
}
