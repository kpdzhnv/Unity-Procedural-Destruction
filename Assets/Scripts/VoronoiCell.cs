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

    public void CutWithPlane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        // check the order of the vertices relatively to the cell center
        if (isOutside(seed, p1, p2, p3))
        {
            var t = p2;
            p2 = p3;
            p3 = t;
        }
        var newVertices = new List<Vector3>(); 
        var newTriangles = new List<int>();
        var planeTriangles = new List<int>();
        var newNormals = new List<Vector3>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            bool v1Out = isOutside(vertices[triangles[i]], p1, p2, p3);
            bool v2Out = isOutside(vertices[triangles[i + 1]], p1, p2, p3);
            bool v3Out = isOutside(vertices[triangles[i + 2]], p1, p2, p3);
            // if all vertices are outside
            if (v1Out && v2Out && v3Out)
                continue;
            // if all vertices are inside
            else if (!v1Out && !v2Out && !v3Out)
            {
                // if vertice is already in new Vertices, do not add it
                int tri1Index = newVertices.IndexOf(vertices[triangles[i]]);
                if (tri1Index > -1)
                    newTriangles.Add(tri1Index);
                else
                {
                    newVertices.Add(vertices[triangles[i]]);
                    newNormals.Add(normals[triangles[i]]);
                    newTriangles.Add(newVertices.Count - 1);
                }

                int tri2Index = newVertices.IndexOf(vertices[triangles[i + 1]]);
                if (tri2Index > -1)
                    newTriangles.Add(tri2Index);
                else
                {
                    newVertices.Add(vertices[triangles[i + 1]]);
                    newNormals.Add(normals[triangles[i + 1]]);
                    newTriangles.Add(newVertices.Count - 1);
                }

                int tri3Index = newVertices.IndexOf(vertices[triangles[i + 2]]);
                if (tri3Index > -1)
                    newTriangles.Add(tri2Index);
                else
                {
                    newVertices.Add(vertices[triangles[i + 2]]);
                    newNormals.Add(normals[triangles[i + 2]]);
                    newTriangles.Add(newVertices.Count - 1);
                }
                // yes I am sorry for this, but there are to many "if"s
                continue;
            }
            // if 1 point is on one side, and 2 - on the other, we cut triangles
            else
            {
                var v1 = vertices[triangles[i]];
                var v2 = vertices[triangles[i + 1]];
                var v3 = vertices[triangles[i + 2]];
                //    v2 ------ v3
                //      \      / 
                // -------------------- plane
                //        \  /
                //         \/ v1
                // if one point is inside, we make it v1
                if (v1Out && v2Out)
                {
                    var t = v1;
                    v1 = v3; v3 = v2; v2 = t;
                }
                else if (v1Out && v3Out)
                {
                    var t = v1;
                    v1 = v2; v2 = v3; v3 = t;
                }

                // v1 is now inside for sure, now cut the triangle
                if (v2Out && v3Out)
                {
                    //we need the two points where the plane intersects the triangle.
                    Vector3 intersection1;
                    Vector3 intersection2;

                    // https://mathworld.wolfram.com/Line-PlaneIntersection.html
                    // v1 -> v2 plane intersection
                    float t1 = new Matrix4x4(
                        new Vector4(1, 1, 1, 1),
                        new Vector4(p1.x, p2.x, p3.x, v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v1.z)
                        ).determinant;
                    float t2 = new Matrix4x4(
                        new Vector4(1, 1, 1, 0),
                        new Vector4(p1.x, p2.x, p3.x, v2.x - v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v2.y - v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v2.z - v1.z)
                        ).determinant;

                    float t = -(t1 / t2);
                    intersection1 = new Vector3(
                        v1.x + (v2.x - v1.x) * t,
                        v1.y + (v2.y - v1.y) * t,
                        v1.z + (v2.z - v1.z) * t);

                    // v1 -> v3 plane intersection (t1 is same for both)
                    t2 = new Matrix4x4(
                        new Vector4(1, 1, 1, 0),
                        new Vector4(p1.x, p2.x, p3.x, v3.x - v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v3.y - v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v3.z - v1.z)
                        ).determinant;

                    t = -(t1 / t2);
                    intersection2 = new Vector3(
                        v1.x + (v3.x - v1.x) * t,
                        v1.y + (v3.y - v1.y) * t,
                        v1.z + (v3.z - v1.z) * t);

                    // add v1
                    int v1Index = newVertices.IndexOf(v1);
                    if (v1Index > -1)
                        newTriangles.Add(v1Index);
                    else
                    {
                        newVertices.Add(v1);
                        newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                        newTriangles.Add(newVertices.Count - 1);
                    }
                    // add intersection1 and intersection2
                    int i1Index = newVertices.IndexOf(intersection1);
                    if (i1Index > -1)
                    {
                        newTriangles.Add(i1Index);
                        planeTriangles.Add(i1Index);
                    }
                    else
                    {
                        newVertices.Add(intersection1);
                        newNormals.Add(Vector3.Cross(intersection2 - intersection1, v1 - intersection1));
                        newTriangles.Add(newVertices.Count - 1);
                        planeTriangles.Add(newVertices.Count - 1);
                    }
                    int i2Index = newVertices.IndexOf(intersection2);
                    if (i2Index > -1)
                    {
                        newTriangles.Add(i2Index);
                        planeTriangles.Add(i2Index);
                    }
                    else
                    {
                        newVertices.Add(intersection2);
                        newNormals.Add(Vector3.Cross(v1 - intersection2, intersection1 - intersection2));
                        newTriangles.Add(newVertices.Count - 1);
                        planeTriangles.Add(newVertices.Count - 1);
                    }
                    continue;
                }

                //         /\ v2
                //        /  \ 
                // --------------------- plane
                //      /      \ 
                //   v1 -------- v3
                // if one point is outside, we make it v2
                if (!v2Out && !v3Out)
                {
                    var t = v2;
                    v2 = v1; v1 = v3; v3 = t;
                }
                else if (!v1Out && !v2Out)
                {
                    var t = v2;
                    v2 = v3; v3 = v1; v1 = t;
                }
                // v2 is now outside for sure, now cut the triangle
                if (!v1Out && !v3Out)
                {
                    //we need the two points where the plane intersects the triangle.
                    Vector3 intersection1;
                    Vector3 intersection2;

                    // https://mathworld.wolfram.com/Line-PlaneIntersection.html
                    // v1 -> v2 plane intersection
                    float t1 = new Matrix4x4(
                        new Vector4(1, 1, 1, 1),
                        new Vector4(p1.x, p2.x, p3.x, v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v1.z)
                        ).determinant;
                    float t2 = new Matrix4x4(
                        new Vector4(1, 1, 1, 0),
                        new Vector4(p1.x, p2.x, p3.x, v2.x - v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v2.y - v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v2.z - v1.z)
                        ).determinant;

                    float t = -(t1 / t2);
                    intersection1 = new Vector3(
                        v1.x + (v2.x - v1.x) * t,
                        v1.y + (v2.y - v1.y) * t,
                        v1.z + (v2.z - v1.z) * t);

                    // v3 -> v2 plane intersection
                    t1 = new Matrix4x4(
                        new Vector4(1, 1, 1, 1),
                        new Vector4(p1.x, p2.x, p3.x, v3.x),
                        new Vector4(p1.y, p2.y, p3.y, v3.y),
                        new Vector4(p1.z, p2.z, p3.z, v3.z)
                        ).determinant;
                    t2 = new Matrix4x4(
                        new Vector4(1, 1, 1, 0),
                        new Vector4(p1.x, p2.x, p3.x, v2.x - v3.x),
                        new Vector4(p1.y, p2.y, p3.y, v2.y - v3.y),
                        new Vector4(p1.z, p2.z, p3.z, v2.z - v3.z)
                        ).determinant;

                    t = -(t1 / t2);
                    intersection2 = new Vector3(
                        v1.x + (v2.x - v3.x) * t,
                        v1.y + (v2.y - v3.y) * t,
                        v1.z + (v2.z - v3.z) * t);

                    // add v1 and v1->i1->i2 triangle
                    int v1Index = newVertices.IndexOf(v1);
                    if (v1Index > -1)
                        newTriangles.Add(v1Index);
                    else
                    {
                        newVertices.Add(v1);
                        newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                        newTriangles.Add(newVertices.Count - 1);
                    }
                    // add intersection1 and intersection2
                    int i1Index = newVertices.IndexOf(intersection1);
                    if (i1Index > -1)
                    {
                        newTriangles.Add(i1Index);
                        planeTriangles.Add(i1Index);
                    }
                    else
                    {
                        newVertices.Add(intersection1);
                        newNormals.Add(Vector3.Cross(intersection2 - intersection1, v1 - intersection1));
                        newTriangles.Add(newVertices.Count - 1);
                        planeTriangles.Add(newVertices.Count - 1);
                    }
                    int i2Index = newVertices.IndexOf(intersection2);
                    if (i2Index > -1)
                    {
                        newTriangles.Add(i2Index);
                        planeTriangles.Add(i2Index);
                    }
                    else
                    {
                        newVertices.Add(intersection2);
                        newNormals.Add(Vector3.Cross(v1 - intersection2, intersection1 - intersection2));
                        newTriangles.Add(newVertices.Count - 1);
                        planeTriangles.Add(newVertices.Count - 1);
                    }

                    // add v3 and v3->v1->i2 triangle
                    int v3Index = newVertices.IndexOf(v3);
                    if (v3Index > -1)
                        newTriangles.Add(v3Index);
                    else
                    {
                        newVertices.Add(v1);
                        newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                        newTriangles.Add(newVertices.Count - 1);
                    }
                    newTriangles.Add(newVertices.IndexOf(v1));
                    newTriangles.Add(newVertices.IndexOf(intersection2));
                    // ending the "1 vertice is inside" part
                    continue;
                }
            }
        }
        Vector3 planeCenter = new Vector3(0,0,0);
        for (int i = 0; i < planeTriangles.Count; i+= 2)
            planeCenter += newVertices[planeTriangles[i]];
        planeCenter /= (planeTriangles.Count / 2);
        newVertices.Add(planeCenter);
        newNormals.Add(Vector3.Cross(p2 - p1, p3 - p1));
        for (int i = 0; i < planeTriangles.Count; i += 2)
        {
            newTriangles.Add(planeTriangles[i]);
            newTriangles.Add(newVertices.Count - 1);
            newTriangles.Add(planeTriangles[i + 1]);
        }

        vertices = newVertices;
        triangles = newTriangles;
        normals = newNormals;
    }

    private bool isOutside(Vector3 v, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a00 = v.x - p1.x, a01 = v.y - p1.y, a02 = v.z - p1.z;
        float a10 = p2.x - p1.x, a11 = p2.y - p1.y, a12 = p2.z - p1.z;
        float a20 = p3.x - p1.x, a21 = p3.y - p1.y, a22 = p3.z - p1.z;

        var det = a00 * a11 * a22 + a01 * a12 * a20 + a02 * a10 * a21 -
            a02 * a11 * a20 - a01 * a10 * a22 - a00 * a12 * a21;
        return det >= 0;
    }
}
