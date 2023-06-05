using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voronoi
{
    // class that generates Delaunay
    public Delaunay delaunay;
    // amount of points to be generated inside the mesh
    public int insidePointsCount = 20;
    // bounding box, that is used for an efficient points generation
    public Vector3 minBounds, maxBounds;

    // initial mesh info
    public Vector3[] meshVertices;
    public int[] meshTriangles;

    // ALL vertices that are used for the algorithms
    private List<Vector3> vertices;

    // mesh triangles with a proper logic
    private List<int> triangles;

    // output array of Voronoi cells
    public List<VoronoiCell> cells;

    public Voronoi(Mesh mesh, Bounds bounds, int count)
    {
        meshVertices = mesh.vertices;
        meshTriangles = mesh.triangles;
        insidePointsCount = count;

        cells = new List<VoronoiCell>();
        vertices = new List<Vector3>();
        triangles = new List<int>();

        minBounds = bounds.min;
        maxBounds = bounds.max;
    }

    public void Generate()
    {
        GenerateVertices();

        //delaunay = new Delaunay(vertices, triangles, meshVertices, meshTriangles);
        delaunay = new Delaunay(vertices, meshVertices, meshTriangles);
        delaunay.Triangulate();

        CreateCells();
    }

    public void CreateCells()
    {
        // each Delaunay vertice is a center of a corresponding Voronoi cell
        for (int i = 0; i < vertices.Count; i++)
        {
            var points = new List<Vector3>();

            // get all the points for the cell using the duality of the Delaunay & Voronoi
            foreach (var t in delaunay.Tetrahedra)
                if (t.ContainsVertex(i))
                    // add the circumcenter
                    points.Add(t.Circumcenter);

            if (points.Count < 4)
                continue;

            VoronoiCell cell = new VoronoiCell(vertices[i], points);

            // after creating the basic cell, it needs to be cut with the faces of the initial mesh
            for (int j = 0; j < triangles.Count; j+=3)
            {
                cell.CutWithPlane(vertices[triangles[j]], vertices[triangles[j + 1]], vertices[triangles[j + 2]]);
            }
            cells.Add(cell);
        }
    }

    // generates vertices up until pointcount and modifies vertices & triangles Lists
    public void GenerateVertices()
    {
        // add initial mesh vertices 
        foreach (var v in meshVertices)
        {
            if (!vertices.Contains(v))
                vertices.Add(v);
        }
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            triangles.Add(vertices.IndexOf(meshVertices[meshTriangles[i]]));
            triangles.Add(vertices.IndexOf(meshVertices[meshTriangles[i + 1]]));
            triangles.Add(vertices.IndexOf(meshVertices[meshTriangles[i + 2]]));
        }

        int pointcount = 0;
        while (pointcount != insidePointsCount)
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

    public bool IsInsideMesh(Vector3 v)
    {
        // https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
        // taking projection of the mesh (ignoring the y coordinate)
        // to get a small set of triangles right above and under the point
        List<int> intersectingTriangles = new List<int>();
        for (int i = 0; i < meshTriangles.Length; i+= 3)
        {
            // getting triangle vertices
            Vector3 p1 = meshVertices[meshTriangles[i]];
            Vector3 p2 = meshVertices[meshTriangles[i+1]];
            Vector3 p3 = meshVertices[meshTriangles[i + 2]];

            // check if point is inside the triangle in 2D
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

        // check if there are no intersections
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
