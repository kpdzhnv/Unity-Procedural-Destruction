using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi : MonoBehaviour
{
    public Delaunay delaunay;
    public int count = 1;
    public int size = 1;

    public Vector3[] meshVertices;
    public int[] meshTriangles;

    public List<Vector3> vertices;
    public List<VoronoiCell> cells;

    public Voronoi(Mesh mesh)
    {
        meshVertices = mesh.vertices;
        meshTriangles = mesh.triangles;

        cells = new List<VoronoiCell>();
        vertices = new List<Vector3>();
    }

    public void Generate()
    {
        GenerateVertices();

        delaunay = new Delaunay();
        delaunay.Triangulate(vertices);

        var points = new List<Vector3>();
        foreach (var v in delaunay.Vertices)
        {
            // using the duality of the Delaunay & Voronoi
            foreach (var t in delaunay.Tetrahedra)
                if (t.ContainsVertex(v))
                    points.Add(t.Circumcenter);

            // вообще это плохо...
            if (points.Count < 4)
                continue;

            VoronoiCell cell = new VoronoiCell(v, points);
            cells.Add(cell);
        }
    }

    public void GenerateVertices()
    {

        // add initial mesh vertices 
        foreach (var v in meshVertices)
        {
            if (!vertices.Contains(v))
                vertices.Add(v);
        }

        int pointcount = 0;
        while (pointcount != count)
        {
            float x = size * Random.Range(-1.0f, 1.0f);
            float y = size * Random.Range(-1.0f, 1.0f);
            float z = size * Random.Range(-1.0f, 1.0f);
            var v = new Vector3(x, y, z);
            if (IsInsideMesh(v))
            {
                vertices.Add(v);
                pointcount++;
            }
        }
    }


    public bool IsInsideMesh(Vector3 v)
    {
        foreach (var t in meshTriangles)
            if (false)
                return false;
        return true;
    }

}
