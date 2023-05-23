using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi
{
    public Delaunay delaunay;
    public int count = 0;
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

            //VoronoiCell cell = new VoronoiCell(v, points);
            //cells.Add(cell);
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
            float x = size * Random.Range(-0.5f, 0.5f);
            float y = size * Random.Range(-0.5f, 0.5f);
            float z = size * Random.Range(-0.5f, 0.5f);
            var v = new Vector3(x, y, z);
            v = new Vector3(0.0f, 0.2f, 0);
            if (IsInsideMesh(v))
            {
                vertices.Add(v);
                pointcount++;
            }
        }
    }

    // later on, a ray is cast through the vertice to check intersection with the triangles
    public bool IsInsideMesh(Vector3 v)
    {
        // https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
        // taking projection of the mesh (ignoring the y coordinate)
        // to get a small set of triangles
        List<int> intersectingTriangles = new List<int>();
        for (int i = 0; i < meshTriangles.Length; i+= 3)
        {
            // getting triangle vertices
            Vector3 p1 = meshVertices[meshTriangles[i]];
            Vector3 p2 = meshVertices[meshTriangles[i+1]];
            Vector3 p3 = meshVertices[meshTriangles[i + 2]];

            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = sign(v, p1, p2);
            d2 = sign(v, p2, p3);
            d3 = sign(v, p3, p1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            // maybe it makes sense to check if the triangle is full above\under the point to make less raytracing? 
            // add triangle to array
            if ( !(has_neg && has_pos))
            {
                intersectingTriangles.Add(i);
                intersectingTriangles.Add(i+1);
                intersectingTriangles.Add(i+2);
            }
        }

        // raycast 
        for (int i = 0; i < intersectingTriangles.Count; i += 3)
        { 
            
        }

        return true;
    }

    float sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

}
