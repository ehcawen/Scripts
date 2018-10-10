using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    private List<Segment> segments = new List<Segment>();

    private Mesh mesh;
    private int amountOfPoints = 10;
    private int sampleRate = 8;

    public float angleCosine = -0.98f;
    public GameObject player;
    public float lifeTime;
    public float width = 5.0f;
    public float alpha = 0.5f;
    public Vector3 init_v = new Vector3(8.0f, 0.0f, 0.0f);
    public Color startColor = Color.white;
    public Color endColor = Color.blue;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    // Use this for initialization

    void Start()
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        // rb.velocity = init_v;
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
            p.upDir = position + width* player.transform.TransformDirection(Vector3.up);
            
            sections.Insert(0, p);
        }
    }

    void UpdateMeshData(float t)
    {

        mesh.Clear();
        segments.Clear();
        
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

        // need at least 2 point(to form the Segment) to draw our triangle
        // so segments.Count > 1
        if (sections.Count < sampleRate + 1) return;
        

 
        // use centripetal Catmull-Rom spline here

        // for each section, calculate angles on both sides (top and bottom)
        for (int i =0; i<sections.Count; i+=sampleRate)
        {
            // create a new segment
            Segment seg = new Segment();
            seg.track        = new List<Vector3>();
            seg.top          = new List<Vector3>();
            segments.Add(seg);
            Vector3 lastTrackPoint = new Vector3();
            Vector3 lastTopPoint = new Vector3();

            if (i==0)
            {
                generate(i, ref seg.track);
                generateTop(i, ref seg.top);
            }

            else if (i>0&&i<sections.Count-sampleRate)
            {
                // -------bottom-------- // 
                // calculate the dot product of BA and BC
                Vector3 BA = Vector3.Normalize(lastTrackPoint - sections[i].point);
                Vector3 BC = Vector3.Normalize(sections[i+sampleRate].point - sections[i].point);

                // ------top------ //
                Vector3 ba = Vector3.Normalize(lastTopPoint - sections[i].upDir);
                Vector3 bc = Vector3.Normalize(sections[i + sampleRate].upDir - sections[i].upDir);

                if (Vector3.Dot(BA, BC) > angleCosine || Vector3.Dot(ba, bc) > angleCosine)
                {
                    generate(i, ref seg.track);
                    generateTop(i, ref seg.top);
                }
                else
                {
                    seg.track.Add(sections[i].point);
                    seg.top.Add(sections[i].upDir);
                }
            }
            else
            {
                seg.track.Add(sections[i].point);
                seg.top.Add(sections[i].upDir);
            }
            lastTopPoint = seg.top[0];
            try
            {
                lastTrackPoint = seg.track[0];
            }
            catch (Exception e)
            {
                Console.WriteLine("Generic Exception Handler: {0}", e.ToString());
            }
           
        }

        // set mesh vertices, uv, color and triangles
        Debug.Assert(segments.Count >= 2);

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles        = new List<int>();
        List<Vector2> uv         = new List<Vector2>();
        List<Color> colors       = new List<Color>();

        float accTrackLength = 0.0f;
        float accTopLength    = 0.0f;

        // -------------------------------
        // save the first EdgePoint
        int headTrackIndex = 0;
        int headTopIndex = 1;
        Vector3 lastTrack = segments[0].track[0];
        Vector3 lastTop    = segments[0].top[0];
        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0, 1));
        vertices.Add(segments[0].track[0]);
        vertices.Add(segments[0].top[0]);
        // -------------------------------

        for (int i = 0; i < segments.Count-1; i++)
        {
            int tailIndex = vertices.Count - 1;
            // for each segment
            Segment curSeg = segments[i];
            Segment nextSeg = segments[i + 1];
            int trackCount = curSeg.track.Count;
            int topCount = curSeg.top.Count;
            int [] trackIndex = new int[trackCount+1];
            int[] topIndex = new int[topCount+1];
            float distance = 0;

            trackIndex[0] = headTrackIndex;
            topIndex[0] = headTopIndex;

            Debug.Assert(trackCount==topCount);
            // ------set vertices------

            // track
            for (int j = 1; j <trackCount; j++) 
            {
                // calculate track-track segment length
                distance = Vector3.Distance(lastTrack,curSeg.track[j]);
                accTrackLength += distance;
                lastTrack = curSeg.track[j];
                // store result in temp uv data list (to be processed)
                uv.Add(new Vector2(accTrackLength, 0));
                
                vertices.Add(curSeg.track[j]);
                tailIndex++;
                trackIndex[j] = tailIndex;
            }
            distance = Vector3.Distance(lastTrack, nextSeg.track[0]);
            accTrackLength += distance;
            lastTrack = nextSeg.track[0];
            uv.Add(new Vector2(accTrackLength, 0));
            
            vertices.Add(nextSeg.track[0]);
            tailIndex++;
            trackIndex[trackCount] = tailIndex;
            headTrackIndex = tailIndex;
            // ---------

            // top
            for(int j = 1; j< topCount; j++)
            {
                distance = Vector3.Distance(lastTop, curSeg.top[j]);
                accTopLength += distance;
                lastTop = curSeg.top[j];
                uv.Add(new Vector2(accTopLength, 1));
                
                vertices.Add(curSeg.top[j]);
                tailIndex++;
                topIndex[j] = tailIndex;
            }
            distance = Vector3.Distance(lastTop, nextSeg.top[0]);
            accTopLength += distance;
            lastTop = nextSeg.top[0];
            uv.Add(new Vector2(accTopLength, 1));

            vertices.Add(nextSeg.top[0]);
            tailIndex++;
            topIndex[topCount] = tailIndex;
            headTopIndex = tailIndex;
            // --------

            // ------set triangles------
            for (int j = 0; j < topCount; j++ )
            {
                triangles.Add(trackIndex[j]);
                triangles.Add(topIndex[j]);
                triangles.Add(topIndex[j + 1]);
                triangles.Add(topIndex[j+1]);
                triangles.Add(trackIndex[j+1]);
                triangles.Add(trackIndex[j]);

            }
            

        }

        // ----set uv----
        int uCount = uv.Count;
        for (int i = 0; i < uCount; i++)
        {
            
            float x = uv[i].x;
            float y = uv[i].y;

            Debug.Assert(y == 1.0f || y == 0.0f);

            if (y == 0)
                // if y ==0 the point is on track
            {
                x = x / accTrackLength;
            }
            else
            // the point is on top
            {
                x = x / accTopLength; 
            }
            Color interpolatedColor = Color.Lerp(startColor, endColor, x);
            colors.Add(interpolatedColor);
            uv[i] = new Vector2(x, y);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.colors = colors.ToArray();
    }

     void generate(int i, ref List<Vector3> segPoints)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 p0 = new Vector3();
        if (i == 0) { p0 = 2*sections[i].point - sections[i + sampleRate].point; }
        else{ p0 = sections[i - sampleRate].point; }
        points.Add(p0);
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
                segPoints.Add(newPoint);
            }

            //segPoints.Add(points[points.Count - 2]);
            

        }

    }

    void generateTop(int i, ref List<Vector3> segPoints)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 p0 = new Vector3();
        if (i == 0) { p0 = 2*sections[i].upDir - sections[i + sampleRate].upDir; }
        else { p0 = sections[i - sampleRate].upDir; }
        points.Add(p0);
        points.Add(sections[i].upDir);
        points.Add(sections[i + sampleRate].upDir);
        if (i + 2 * sampleRate < sections.Count)
        {
            points.Add(sections[i + 2 * sampleRate].upDir);
        }
        else
        {
            points.Add(2 * sections[i + sampleRate].upDir - sections[i].upDir);
        }

        for (int k = 0; k < points.Count - 3; k++)
        {
            for (int j = 0; j < amountOfPoints; j++)
            {
                Vector3 newPoint = PointOnCurve(points[k], points[k + 1], points[k + 2], points[k + 3], (1f / amountOfPoints) * j);
                segPoints.Add(newPoint);
            }

            //segPoints.Add(points[points.Count - 2]);


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
