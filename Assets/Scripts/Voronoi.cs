using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi
{
    public Delaunay delaunay;
    public int count = 1;
    public Vector3 minBounds, maxBounds;

    public Vector3[] meshVertices;
    public int[] meshTriangles;

    public List<Vector3> vertices;
    public List<VoronoiCell> cells;

    public Voronoi(Mesh mesh, Bounds bounds)
    {
        meshVertices = mesh.vertices;
        meshTriangles = mesh.triangles;

        cells = new List<VoronoiCell>();
        vertices = new List<Vector3>();

        minBounds = bounds.min;
        maxBounds = bounds.max;
    }

    public void Generate()
    {
        GenerateVertices();

        delaunay = new Delaunay();
        delaunay.Triangulate(vertices);

        foreach (var v in delaunay.Vertices)
        {
            var points = new List<Vector3>();
            // get all the points for the cell
            // using the duality of the Delaunay & Voronoi
            foreach (var t in delaunay.Tetrahedra)
                if (t.ContainsVertex(v))
                    points.Add(t.Circumcenter);

            if (points.Count < 4)
                continue;

            VoronoiCell cell = new VoronoiCell(v, points);
            cells.Add(cell);
        }
    }

    public void GenerateVertices()
    {
        // add initial mesh vertices 
        foreach (var p in meshVertices)
        {
            if (!vertices.Contains(p))
                vertices.Add(p);
        }

        int pointcount = 0;
        while (pointcount != count)
        {
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            float z = Random.Range(minBounds.z, maxBounds.z);
            var v = new Vector3(x, y, z);
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

            d1 = sign2D(v, p1, p2);
            d2 = sign2D(v, p2, p3);
            d3 = sign2D(v, p3, p1);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            // add triangle to array
            if ( !(has_neg && has_pos))
            {
                intersectingTriangles.Add(i);
                intersectingTriangles.Add(i+1);
                intersectingTriangles.Add(i+2);
            }
        }

        if (intersectingTriangles.Count == 0)
            return false;

        int countinside = 0;
        int countoutside = 0;
        // check the inside\outside mesh rule for the small set of the triangles 
        for (int i = 0; i < intersectingTriangles.Count; i += 3)
        {
            // getting triangle vertices
            Vector3 p1 = meshVertices[meshTriangles[intersectingTriangles[i]]];
            Vector3 p2 = meshVertices[meshTriangles[intersectingTriangles[i+1]]];
            Vector3 p3 = meshVertices[meshTriangles[intersectingTriangles[i+2]]];

            float a00 = v.x - p1.x, a01 = v.y - p1.y, a02 = v.z - p1.z;
            float a10 = p2.x - p1.x, a11 = p2.y - p1.y, a12 = p2.z - p1.z;
            float a20 = p3.x - p1.x, a21 = p3.y - p1.y, a22 = p3.z - p1.z;

            var det = a00 * a11 * a22 + a01 * a12 * a20 + a02 * a10 * a21 -
                a02 * a11 * a20 - a01 * a10 * a22 - a00 * a12 * a21;
            //Debug.Log($"det: {det}, p1: {p1}, p2: {p2}, p3: {p3}");
            if (det > 0)
                countoutside++;
            else
                countinside++;
        }
        return (countinside - countoutside) != 0;
    }

    float sign2D(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

}
