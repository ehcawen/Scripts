using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]


// a class to store points on the track
class EdgePoint
{
    public Vector3 point;
    public Vector3 upDir;
    public float time;

    public EdgePoint(Vector3 p, float t)
    {
        point = p;
        time = t;
    }
}

class Segment
{
    public List<Vector3> track;
    public List<Vector3> top;
}

public class TrailEffect : MonoBehaviour
{
    
    private List<EdgePoint> sections = new List<EdgePoint>();
    private List<EdgePoint> track = new List<EdgePoint>();
    private List<Vector3> interpolation = new List<Vector3>();
    
    private Mesh mesh;
    private int amountOfPoints = 2;
    private int sampleRate = 1;
    private float angleCosine = -0.99f;

    public GameObject player;
    public float lifeTime;
    public float width;
    public float alpha = 0.5f;
    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Use this for initialization

    void Start()
    {

    }

    private void FixedUpdate()
    {
        MakeMeshData(Time.time);
        UpdateMeshData(Time.time);
    }

    void MakeMeshData(float t)
    {
        
        // assign the current position
        Vector3 position = player.transform.position;

        if (sections.Count == 0 || (position - sections[0].point).sqrMagnitude > 0)
        {
            
            EdgePoint p = new EdgePoint(position, t);
            p.upDir = player.transform.TransformDirection(Vector3.up);
            
            sections.Insert(0, p);
        }
    }

    void UpdateMeshData(float t)
    {

        mesh.Clear();
        
        // delete expired EdgePoint

        while (sections.Count > 0)
        {
            float intersection = t - sections[sections.Count - 1].time;
            if (intersection > lifeTime)
            {
               
                sections.RemoveAt(sections.Count - 1);
            }
            else break;
            
        }

        // need at least 2 point to draw our triangle
        if (sections.Count < 2) return;
        track.Clear();

 
        // use centripetal Catmull-Rom spline here
        for (int i =0; i<sections.Count; i+=sampleRate)
        {
            
            if (i>sampleRate&&i<sections.Count-sampleRate)
            {
                // calculate the dot product of BA and BC
                Vector3 BA = Vector3.Normalize(track[track.Count-1].point - sections[i].point);
                Vector3 BC = Vector3.Normalize(sections[i+sampleRate].point - sections[i].point);

                if (Vector3.Dot(BA, BC) > angleCosine)
                { 
                    generate(i);
                }
                else
                {
                    track.Add(sections[i]);
                }

            }
            else
            {
                track.Add(sections[i]);
            }
        }  

        // set mesh vertices
        Vector3 [] vertices = new Vector3[track.Count * 2];


        for (int i = 0; i < track.Count; i++)
        {
            EdgePoint curPoint = track[i];
            Vector3 upDir = curPoint.upDir;

            // create 2 vertices
            vertices[i * 2] = curPoint.point;
            vertices[i * 2 + 1] = curPoint.point + upDir * width;
        }


        // set mesh triangles
        int triangleIndex=0;
        triangleIndex = (track.Count - 1) * 2 * 3;
        int[] triangles = new int[triangleIndex];
        int triangleLoop = track.Count - 1;
        for (int i = 0; i < triangleLoop; i++)
        {
            triangles[i * 6 + 0] = i * 2;
            triangles[i * 6 + 1] = i * 2 + 1;
            triangles[i * 6 + 2] = i * 2 + 2;


            triangles[i * 6 + 3] = i * 2 + 2;
            triangles[i * 6 + 4] = i * 2 + 1;
            triangles[i * 6 + 5] = i * 2 + 3;

        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

    }

     void generate(int i)
    {
        List<Vector3> points = new List<Vector3>();
        if (i < 2*sampleRate)
        {
            points.Add(2*sections[i - sampleRate].point-sections[i].point);
        }else
        {
            points.Add(sections[i - 2*sampleRate].point);
        }
        points.Add(sections[i - sampleRate].point);
        points.Add(sections[i].point);
        points.Add(sections[i + sampleRate].point);
        if (i+2*sampleRate<sections.Count)
        {
            points.Add(sections[i + 2*sampleRate].point);
        }
        else
        {
            points.Add(2 * sections[i + sampleRate].point - sections[i].point);
        }

        for (int k = 0; k < points.Count - 3; k++)
        {
            for (int j = 0; j < amountOfPoints; j++)
            {
                Vector3 newPoint = PointOnCurve(points[k], points[k + 1], points[k + 2], points[k + 3], (1f / amountOfPoints) * j);
                EdgePoint ep = new EdgePoint(newPoint, 0);
                ep.upDir = sections[i].upDir;
                track.Add(ep);
            }

            EdgePoint lastPoint = new EdgePoint(points[points.Count-2],0);
            lastPoint.upDir = sections[i + sampleRate].upDir;
            //track.Add(lastPoint);

        }

    }

    static Vector3 PointOnCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 ret = new Vector3();

        float t2 = t * t;
        float t3 = t2 * t;

        ret = 0.5f * ((2.0f * p1) +
        (-p0 + p2) * t +
        (2.0f * p0 - 5.0f * p1 + 4 * p2 - p3) * t2 +
        (-p0 + 3.0f * p1- 3.0f * p2 + p3) * t3);

        return ret;
    }

    
}
