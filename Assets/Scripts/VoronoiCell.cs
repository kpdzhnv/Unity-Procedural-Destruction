using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCell
{
    public Vector3 seed;
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector3> normals;

    public VoronoiCell(Vector3 seedPos, List<Vector3> points)
    {
        seed = seedPos;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();

        ConvexHull.ConvexHullCalculator calc = new ConvexHull.ConvexHullCalculator();
        calc.GenerateHull(points, true, ref vertices, ref triangles, ref normals);
    }
}
