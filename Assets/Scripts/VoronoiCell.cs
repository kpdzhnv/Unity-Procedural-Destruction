using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCell
{
    public Vector3 seed;
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector3> normals;

    public bool isBad;

    public VoronoiCell(Vector3 seedPos, List<Vector3> points)
    {
        seed = seedPos;
        vertices = new List<Vector3>();
        triangles = new List<int>();
        normals = new List<Vector3>();

        if (points.Count < 4)
        {
            isBad = true;
            return;
        }
        ConvexHull.ConvexHullCalculator calc = new ConvexHull.ConvexHullCalculator();
        calc.GenerateHull(points, true, ref vertices, ref triangles, ref normals);
    }

    //public void ConvexHull()
    //{
    //    if (vertices.Count < 4)
    //    {
    //        isBad = true;
    //        return;
    //    }
    //}

    public void CutWithPlane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 planeNormal = Vector3.Cross(p2 - p1, p3 - p1);
        var newVertices = new List<Vector3>(); 
        var newTriangles = new List<int>();
        var planeTriangles = new List<int>();
        var newNormals = new List<Vector3>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            var v1 = vertices[triangles[i]];
            var v2 = vertices[triangles[i + 1]];
            var v3 = vertices[triangles[i + 2]];
            bool v1Out = IsAbovePlane(v1, p1, p2, p3);
            bool v2Out = IsAbovePlane(v2, p1, p2, p3);
            bool v3Out = IsAbovePlane(v3, p1, p2, p3);
            bool v1On = IsOnPlane(v1, p1, p2, p3);
            bool v2On = IsOnPlane(v2, p1, p2, p3);
            bool v3On = IsOnPlane(v3, p1, p2, p3);

            // if all vertices are outside or some are on the plane
            if (v1Out && v2Out && v3Out ||
                v1On && v2On && v3Out ||
                v1Out && v2On && v3On ||
                v1On && v2Out && v3On ||
                v1On && v2Out && v3Out ||
                v1Out && v2On && v3Out ||
                v1Out && v2Out && v3On)
            {
                continue;
            }
            // if all vertices are inside (automatically includes if any point is on the plane)
            else if (!v1Out && !v2Out && !v3Out)
            {
                newVertices.Add(v1);
                newNormals.Add(normals[triangles[i]]);
                newTriangles.Add(newVertices.Count - 1);

                newVertices.Add(v2);
                newNormals.Add(normals[triangles[i + 1]]);
                newTriangles.Add(newVertices.Count - 1);

                newVertices.Add(v3);
                newNormals.Add(normals[triangles[i + 2]]);
                newTriangles.Add(newVertices.Count - 1);
            }

            // if 1 point is INSIDE (not on the plane), and 2 - outside, we cut 1 triangle
            else if (v1Out && v2Out && !v3Out  ||
                !v1Out && v2Out && v3Out ||
                v1Out && !v2Out && v3Out )
            {
                //    v2 ------ v3
                //      \      / 
                // -------------------- plane
                //        \  /
                //         \/ v1
                // if one point is inside, we make it v1
                if (v1Out && v2Out)
                {
                    var temp = v1;
                    v1 = v3; v3 = v2; v2 = temp;
                }
                else if (v1Out && v3Out)
                {
                    var temp = v1;
                    v1 = v2; v2 = v3; v3 = temp;
                }

                // v1 is now inside for sure, now cut the triangle

                //we need the two points where the plane intersects the triangle.
                Vector3 intersection1;
                Vector3 intersection2;

                if (v2On)
                {
                    intersection1 = v2;

                }
                else
                {
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
                }
                if (v3On)
                {
                    intersection2 = v3;
                }
                else
                {
                    // v1 -> v3 plane intersection (t1 is same for both)
                    float t1 = new Matrix4x4(
                        new Vector4(1, 1, 1, 1),
                        new Vector4(p1.x, p2.x, p3.x, v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v1.z)
                        ).determinant;
                    float t2 = new Matrix4x4(
                        new Vector4(1, 1, 1, 0),
                        new Vector4(p1.x, p2.x, p3.x, v3.x - v1.x),
                        new Vector4(p1.y, p2.y, p3.y, v3.y - v1.y),
                        new Vector4(p1.z, p2.z, p3.z, v3.z - v1.z)
                        ).determinant;

                    float t = -(t1 / t2);
                    intersection2 = new Vector3(
                        v1.x + (v3.x - v1.x) * t,
                        v1.y + (v3.y - v1.y) * t,
                        v1.z + (v3.z - v1.z) * t);

                }
                // add v1
                newVertices.Add(v1);
                newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                newTriangles.Add(newVertices.Count - 1);

                // add intersection1 and intersection2
                newVertices.Add(intersection1);
                newNormals.Add(Vector3.Cross(intersection2 - intersection1, v1 - intersection1));
                newTriangles.Add(newVertices.Count - 1);
                newVertices.Add(intersection1);
                newNormals.Add(planeNormal);
                planeTriangles.Add(newVertices.Count - 1);

                newVertices.Add(intersection2);
                newNormals.Add(Vector3.Cross(v1 - intersection2, intersection1 - intersection2));
                newTriangles.Add(newVertices.Count - 1);
                newVertices.Add(intersection2);
                newNormals.Add(planeNormal);
                planeTriangles.Add(newVertices.Count - 1);
            }

            // if 2 points are INSIDE (not on the plane), and 1 - outside, we cut 2 triangles
            else if (v1Out && !v2Out && !v3Out && !v3On && !v2On ||
            !v1Out && v2Out && !v3Out && !v1On && !v3On ||
            !v1Out && !v2Out && v3Out && !v2On && !v1On)
            {
                //         /\ v2
                //        /  \ 
                // --------------------- plane
                //      /      \ 
                //   v1 -------- v3
                // if one point is outside, we make it v2
                if (!v2Out && !v3Out)
                {
                    var temp = v2;
                    v2 = v1; v1 = v3; v3 = temp;
                }
                else if (!v1Out && !v2Out)
                {
                    var temp = v2;
                    v2 = v3; v3 = v1; v1 = temp;
                }
                // v2 is now outside for sure, now cut the triangle

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
                    v3.x + (v2.x - v3.x) * t,
                    v3.y + (v2.y - v3.y) * t,
                    v3.z + (v2.z - v3.z) * t);

                // add v1 and v1->i1->i2 triangle
                newVertices.Add(v1);
                newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                newTriangles.Add(newVertices.Count - 1);

                // add intersection1 and intersection2
                newVertices.Add(intersection1);
                newNormals.Add(Vector3.Cross(intersection2 - intersection1, v1 - intersection1));
                newTriangles.Add(newVertices.Count - 1);
                newVertices.Add(intersection1);
                newNormals.Add(planeNormal);
                planeTriangles.Add(newVertices.Count - 1);

                newVertices.Add(intersection2);
                newNormals.Add(Vector3.Cross(v1 - intersection2, intersection1 - intersection2));
                newTriangles.Add(newVertices.Count - 1);
                newVertices.Add(intersection2);
                newNormals.Add(planeNormal);
                planeTriangles.Add(newVertices.Count - 1);

                // add v3 and v3->v1->i2 triangle
                newVertices.Add(v3);
                newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                newTriangles.Add(newVertices.Count - 1);

                newVertices.Add(v1);
                newNormals.Add(Vector3.Cross(intersection1 - v1, intersection2 - v1));
                newTriangles.Add(newVertices.Count - 1);

                newVertices.Add(intersection2);
                newNormals.Add(Vector3.Cross(v3 - intersection2, v1 - intersection2));
                newTriangles.Add(newVertices.Count - 1);

            }
        }

        // calculate intersecting plane center and create triangles with it
        Vector3 planeCenter = new Vector3(0,0,0);
        for (int i = 0; i < planeTriangles.Count; i+= 2)
            planeCenter += newVertices[planeTriangles[i]];

        planeCenter /= (planeTriangles.Count / 2);
        newVertices.Add(planeCenter);
        newNormals.Add(planeNormal);
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

    private bool IsAbovePlane(Vector3 v, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a00 = v.x - p1.x, a01 = v.y - p1.y, a02 = v.z - p1.z;
        float a10 = p2.x - p1.x, a11 = p2.y - p1.y, a12 = p2.z - p1.z;
        float a20 = p3.x - p1.x, a21 = p3.y - p1.y, a22 = p3.z - p1.z;

        float det = a00 * a11 * a22 + a01 * a12 * a20 + a02 * a10 * a21 -
            a02 * a11 * a20 - a01 * a10 * a22 - a00 * a12 * a21;
        //return det > 0.001f;
        return det > 0;
    }


    private bool IsOnPlane(Vector3 v, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float a00 = v.x - p1.x, a01 = v.y - p1.y, a02 = v.z - p1.z;
        float a10 = p2.x - p1.x, a11 = p2.y - p1.y, a12 = p2.z - p1.z;
        float a20 = p3.x - p1.x, a21 = p3.y - p1.y, a22 = p3.z - p1.z;

        float det = a00 * a11 * a22 + a01 * a12 * a20 + a02 * a10 * a21 -
            a02 * a11 * a20 - a01 * a10 * a22 - a00 * a12 * a21;
        //return Mathf.Abs(det) < 0.001f;
        return det == 0;
    }
}
