using System;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay
{
    public class Triangle
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }

        public bool IsBad { get; set; }
        public bool IsBoundary { get; set; }

        public Triangle(int u, int v, int w)
        {
            A = u;
            B = v;
            C = w;
        }

        public static bool operator ==(Triangle left, Triangle right)
        {
            return (left.A == right.A || left.A == right.B || left.A == right.C)
                && (left.B == right.A || left.B == right.B || left.B == right.C)
                && (left.C == right.A || left.C == right.B || left.C == right.C);
        }

        public static bool operator !=(Triangle left, Triangle right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle e)
            {
                return this == e;
            }

            return false;
        }

        public bool Equals(Triangle e)
        {
            return this == e;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
        }

    }

    public class Tetrahedron : IEquatable<Tetrahedron>
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }

        public bool IsBad { get; set; }

        public Vector3 Circumcenter { get; set; }
        float CircumradiusSquared { get; set; }

        public Tetrahedron(int a, int b, int c, int d, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            CalculateCircumsphere(v1, v2, v3, v4);
        }
        public Vector3 GetCentroid(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            return new Vector3((v1.x + v2.x + v3.x + v4.x) / 4.0f, (v1.y + v2.y + v3.y + v4.y) / 4.0f, (v1.z + v2.z + v3.z + v4.z) / 4.0f);
        }

        //http://mathworld.wolfram.com/Circumsphere.html
        // yes, I am sorry for this, please pass the vertices to this method
        void CalculateCircumsphere(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            float a = new Matrix4x4(
                new Vector4(v1.x, v2.x, v3.x, v4.x),
                new Vector4(v1.y, v2.y, v3.y, v4.y),
                new Vector4(v1.z, v2.z, v3.z, v4.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float aPosSqr = v1.sqrMagnitude;
            float bPosSqr = v2.sqrMagnitude;
            float cPosSqr = v3.sqrMagnitude;
            float dPosSqr = v4.sqrMagnitude;

            float Dx = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(v1.y, v2.y, v3.y, v4.y),
                new Vector4(v1.z, v2.z, v3.z, v4.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float Dy = -(new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(v1.x, v2.x, v3.x, v4.x),
                new Vector4(v1.z, v2.z, v3.z, v4.z),
                new Vector4(1, 1, 1, 1)
            ).determinant);

            float Dz = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(v1.x, v2.x, v3.x, v4.x),
                new Vector4(v1.y, v2.y, v3.y, v4.y),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float c = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(v1.x, v2.x, v3.x, v4.x),
                new Vector4(v1.y, v2.y, v3.y, v4.y),
                new Vector4(v1.z, v2.z, v3.z, v4.z)
            ).determinant;

            Circumcenter = new Vector3(
                Dx / (2 * a),
                Dy / (2 * a),
                Dz / (2 * a)
            );

            CircumradiusSquared = ((Dx * Dx) + (Dy * Dy) + (Dz * Dz) - (4 * a * c)) / (4 * a * a);
        }

        public bool ContainsVertex(int v)
        {
            return v == A
                || v == B
                || v == C
                || v == D;
        }

        public bool CircumCircleContains(Vector3 v)
        {
            Vector3 dist = v - Circumcenter;
            return dist.sqrMagnitude < CircumradiusSquared;
        }

        public static bool operator ==(Tetrahedron left, Tetrahedron right)
        {
            return (left.A == right.A || left.A == right.B || left.A == right.C || left.A == right.D)
                && (left.B == right.A || left.B == right.B || left.B == right.C || left.B == right.D)
                && (left.C == right.A || left.C == right.B || left.C == right.C || left.C == right.D)
                && (left.D == right.A || left.D == right.B || left.D == right.C || left.D == right.D);
        }

        public static bool operator !=(Tetrahedron left, Tetrahedron right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Tetrahedron t)
            {
                return this == t;
            }

            return false;
        }

        public bool Equals(Tetrahedron t)
        {
            return this == t;
        }

        public override int GetHashCode()
        {
            return A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode() ^ D.GetHashCode();
        }
    }

    public static bool AlmostEqual(Triangle left, Triangle right)
    {
        return (left.A == right.A || left.A == right.B || left.A == right.C)
            && (left.B == right.A || left.B == right.B || left.B == right.C)
            && (left.C == right.A || left.C == right.B || left.C == right.C);
    }

    public List<Vector3> Vertices { get; private set; }
    // initial mesh info
    public Vector3[] meshVertices;
    public int[] meshTriangles;
    public List<Tetrahedron> Tetrahedra { get; private set; }
    public Tetrahedron THETETRAHEDRON;
    public Delaunay(List<Vector3> vertices, Vector3[] mv, int[] mt)
    {
        Tetrahedra = new List<Tetrahedron>();
        Vertices = new List<Vector3>(vertices);
        meshVertices = mv;
        meshTriangles = mt;
    }

    public void Triangulate()
    {
        InitializeTheTetrahedron();

        // adding vertices 
        for (int i = 0; i < Vertices.Count; i++)
        {
            List<Triangle> triangles = new List<Triangle>();

            // check every tetrahedron for the delaunay rule
            foreach (var tet in Tetrahedra)
            {
                if (tet.CircumCircleContains(Vertices[i]))
                {
                    tet.IsBad = true;
                    triangles.Add(new Triangle(tet.A, tet.B, tet.C));
                    triangles.Add(new Triangle(tet.A, tet.B, tet.D));
                    triangles.Add(new Triangle(tet.A, tet.C, tet.D));
                    triangles.Add(new Triangle(tet.B, tet.C, tet.D));
                }
            }
            // check if there are repetitions in triangles (if an added vertice was in sphere of multiple tetrahedra, one triangle is added multiple times)
            for (int j = 0; j < triangles.Count; j++)
                for (int k = j + 1; k < triangles.Count; k++)
                {
                    if (AlmostEqual(triangles[j], triangles[k]))
                    {
                        triangles[j].IsBad = true;
                        triangles[k].IsBad = true;
                    }
                }

            Tetrahedra.RemoveAll((Tetrahedron t) => t.IsBad);
            triangles.RemoveAll((Triangle t) => t.IsBad);

            foreach (var triangle in triangles)
            {
                var tet = new Tetrahedron(triangle.A, triangle.B, triangle.C, i, Vertices[triangle.A], Vertices[triangle.B], Vertices[triangle.C], Vertices[i]);
                Tetrahedra.Add(tet);
            }
        }

        //RemoveTheTetrahedron();

        var tetsToFLip = new List<Tetrahedron>();
        // flip tets
        //while (tetsToFLip.Count > 0)
        //{
        //    // yes...
        //}

        // clean up the extra tetrahedra that are generated for the non-convex meshes
        foreach (var tet in Tetrahedra)
        {
            if (!IsInsideMesh(tet.GetCentroid(Vertices[tet.A], Vertices[tet.B], Vertices[tet.C], Vertices[tet.D])))
                tet.IsBad = true;
        }
        Tetrahedra.RemoveAll((Tetrahedron t) => t.IsBad);
    }

    // initialization of a VERY huge tetrahedron that has all vertices included
    private void InitializeTheTetrahedron()
    {
        float minX = Vertices[0].x;
        float minY = Vertices[0].y;
        float minZ = Vertices[0].z;
        float maxX = minX;
        float maxY = minY;
        float maxZ = minZ;

        foreach (var vertex in Vertices)
        {
            if (vertex.x < minX) minX = vertex.x;
            if (vertex.x > maxX) maxX = vertex.x;
            if (vertex.y < minY) minY = vertex.y;
            if (vertex.y > maxY) maxY = vertex.y;
            if (vertex.z < minZ) minZ = vertex.z;
            if (vertex.z > maxZ) maxZ = vertex.z;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float deltaMax = Mathf.Max(dx, dy, dz) * 100;
        float offset = 100;

        Vector3 p1 = new Vector3(minX - offset, minY - offset, minZ - offset);
        Vector3 p2 = new Vector3(maxX + deltaMax, minY - offset, minZ - offset);
        Vector3 p3 = new Vector3(minX - offset, maxY + deltaMax, minZ - offset);
        Vector3 p4 = new Vector3(minX - offset, minY - offset, maxZ + deltaMax);

        int ind = Vertices.Count;
        Vertices.Add(p1);
        Vertices.Add(p2);
        Vertices.Add(p3);
        Vertices.Add(p4);
        THETETRAHEDRON = new Tetrahedron(ind, ind + 1, ind + 2, ind + 3, p1, p2, p3, p4);
        Tetrahedra.Add(THETETRAHEDRON);
    }

    // cleanup of a VERY huge tetrahedron that has all vertices included
    private void RemoveTheTetrahedron()
    {
        // clean up the huge tetrahedron
        Tetrahedra.RemoveAll((Tetrahedron t) =>
            t.ContainsVertex(THETETRAHEDRON.A)
            || t.ContainsVertex(THETETRAHEDRON.B)
            || t.ContainsVertex(THETETRAHEDRON.C)
            || t.ContainsVertex(THETETRAHEDRON.D));
        Vertices.RemoveAt(Vertices.Count - 1);
        Vertices.RemoveAt(Vertices.Count - 1);
        Vertices.RemoveAt(Vertices.Count - 1);
        Vertices.RemoveAt(Vertices.Count - 1);

    }

    public bool IsInsideMesh(Vector3 v)
    {
        // https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
        // taking projection of the mesh (ignoring the y coordinate)
        // to get a small set of triangles right aboce and under the point
        List<int> intersectingTriangles = new List<int>();
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            // getting triangle vertices
            Vector3 p1 = meshVertices[meshTriangles[i]];
            Vector3 p2 = meshVertices[meshTriangles[i + 1]];
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
            if (!(has_neg && has_pos))
            {
                intersectingTriangles.Add(i);
                intersectingTriangles.Add(i + 1);
                intersectingTriangles.Add(i + 2);
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
            Vector3 p2 = meshVertices[meshTriangles[intersectingTriangles[i + 1]]];
            Vector3 p3 = meshVertices[meshTriangles[intersectingTriangles[i + 2]]];

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