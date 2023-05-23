using System;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay
{
    public class Tetrahedron : IEquatable<Tetrahedron>
    {
        public Vector3 A { get; set; }
        public Vector3 B { get; set; }
        public Vector3 C { get; set; }
        public Vector3 D { get; set; }

        public bool IsBad { get; set; }

        public Vector3 Circumcenter { get; set; }
        float CircumradiusSquared { get; set; }

        public Tetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
            CalculateCircumsphere();
        }

        //http://mathworld.wolfram.com/Circumsphere.html
        void CalculateCircumsphere()
        {
            float a = new Matrix4x4(
                new Vector4(A.x, B.x, C.x, D.x),
                new Vector4(A.y, B.y, C.y, D.y),
                new Vector4(A.z, B.z, C.z, D.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float aPosSqr = A.sqrMagnitude;
            float bPosSqr = B.sqrMagnitude;
            float cPosSqr = C.sqrMagnitude;
            float dPosSqr = D.sqrMagnitude;

            float Dx = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.y, B.y, C.y, D.y),
                new Vector4(A.z, B.z, C.z, D.z),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float Dy = -(new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.x, B.x, C.x, D.x),
                new Vector4(A.z, B.z, C.z, D.z),
                new Vector4(1, 1, 1, 1)
            ).determinant);

            float Dz = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.x, B.x, C.x, D.x),
                new Vector4(A.y, B.y, C.y, D.y),
                new Vector4(1, 1, 1, 1)
            ).determinant;

            float c = new Matrix4x4(
                new Vector4(aPosSqr, bPosSqr, cPosSqr, dPosSqr),
                new Vector4(A.x, B.x, C.x, D.x),
                new Vector4(A.y, B.y, C.y, D.y),
                new Vector4(A.z, B.z, C.z, D.z)
            ).determinant;

            Circumcenter = new Vector3(
                Dx / (2 * a),
                Dy / (2 * a),
                Dz / (2 * a)
            );

            CircumradiusSquared = ((Dx * Dx) + (Dy * Dy) + (Dz * Dz) - (4 * a * c)) / (4 * a * a);
        }

        public bool ContainsVertex(Vector3 v)
        {
            return AlmostEqual(v, A)
                || AlmostEqual(v, B)
                || AlmostEqual(v, C)
                || AlmostEqual(v, D);
        }

        public bool CircumCircleContains(Vector3 v)
        {
            Vector3 dist = v - Circumcenter;
            // a slight inaccuracy because there can be 90 degree angles and the circumradius is on the face !
            return dist.sqrMagnitude <= CircumradiusSquared - 0.1f; 
        }

        public List<Vector3> GetVertices()
        {
            var l = new List<Vector3>(4);
            l.Add(A);
            l.Add(B);
            l.Add(C);
            l.Add(D);
            return l;
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

    public class Triangle
    {
        public Vector3 U { get; set; }
        public Vector3 V { get; set; }
        public Vector3 W { get; set; }

        public bool IsBad { get; set; }

        public Triangle(Vector3 u, Vector3 v, Vector3 w)
        {
            U = u;
            V = v;
            W = w;
        }

        public static bool operator ==(Triangle left, Triangle right)
        {
            return (left.U == right.U || left.U == right.V || left.U == right.W)
                && (left.V == right.U || left.V == right.V || left.V == right.W)
                && (left.W == right.U || left.W == right.V || left.W == right.W);
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
            return U.GetHashCode() ^ V.GetHashCode() ^ W.GetHashCode();
        }

    }

    public class Edge
    {
        public Vector3 U { get; set; }
        public Vector3 V { get; set; }

        public bool IsBad { get; set; }

        public Edge(Vector3 u, Vector3 v)
        {
            U = u;
            V = v;
        }

        public static bool operator ==(Edge left, Edge right)
        {
            return (left.U == right.U || left.U == right.V)
                && (left.V == right.U || left.V == right.V);
        }

        public static bool operator !=(Edge left, Edge right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Edge e)
            {
                return this == e;
            }

            return false;
        }

        public bool Equals(Edge e)
        {
            return this == e;
        }

        public override int GetHashCode()
        {
            return U.GetHashCode() ^ V.GetHashCode();
        }


    }

    public static bool AlmostEqual(Vector3 left, Vector3 right)
    {
        return (left - right).sqrMagnitude < 0.01f;
    }
    public static bool AlmostEqual(Edge left, Edge right)
    {
        return (AlmostEqual(left.U, right.U) || AlmostEqual(left.V, right.U))
            && (AlmostEqual(left.U, right.V) || AlmostEqual(left.V, right.U));
    }
    public static bool AlmostEqual(Triangle left, Triangle right)
    {
        return (AlmostEqual(left.U, right.U) || AlmostEqual(left.U, right.V) || AlmostEqual(left.U, right.W))
            && (AlmostEqual(left.V, right.U) || AlmostEqual(left.V, right.V) || AlmostEqual(left.V, right.W))
            && (AlmostEqual(left.W, right.U) || AlmostEqual(left.W, right.V) || AlmostEqual(left.W, right.W));
    }

    public List<Vector3> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }
    public List<Triangle> Triangles { get; private set; }
    public List<Tetrahedron> Tetrahedra { get; private set; }
    public Tetrahedron THETETRAHEDRON;

    public Delaunay()
    {
        Edges = new List<Edge>();
        Triangles = new List<Triangle>();
        Tetrahedra = new List<Tetrahedron>();
        Vertices = new List<Vector3>();
    }

    public void Triangulate(List<Vector3> vertices)
    {
        Edges = new List<Edge>();
        Triangles = new List<Triangle>();
        Tetrahedra = new List<Tetrahedron>();
        Vertices = new List<Vector3>(vertices);

        InitializeTheTetrahedron();

        // adding vertices 
        foreach (var vertex in Vertices)
        {
            List<Triangle> triangles = new List<Triangle>();

            // check every tetrahedron for the delauney rule
            foreach (var t in Tetrahedra)
            {
                if (t.CircumCircleContains(vertex))
                {
                    t.IsBad = true;
                    triangles.Add(new Triangle(t.A, t.B, t.C));
                    triangles.Add(new Triangle(t.A, t.B, t.D));
                    triangles.Add(new Triangle(t.A, t.C, t.D));
                    triangles.Add(new Triangle(t.B, t.C, t.D));
                }
            }

            // check if there are repetitions in triangles
            for (int i = 0; i < triangles.Count; i++)
            {
                for (int j = i + 1; j < triangles.Count; j++)
                {
                    if (AlmostEqual(triangles[i], triangles[j]))
                    {
                        triangles[i].IsBad = true;
                        triangles[j].IsBad = true;
                    }
                }
            }

            Tetrahedra.RemoveAll((Tetrahedron t) => t.IsBad);
            triangles.RemoveAll((Triangle t) => t.IsBad);

            foreach (var triangle in triangles)
            {
                Tetrahedra.Add(new Tetrahedron(triangle.U, triangle.V, triangle.W, vertex));
            }
        }

        // clean up the huge tetrahedron
        Tetrahedra.RemoveAll((Tetrahedron t) => 
            t.ContainsVertex(THETETRAHEDRON.A)
            || t.ContainsVertex(THETETRAHEDRON.B)
            || t.ContainsVertex(THETETRAHEDRON.C)
            || t.ContainsVertex(THETETRAHEDRON.D));

        HashSet<Triangle> triangleSet = new HashSet<Triangle>();
        HashSet<Edge> edgeSet = new HashSet<Edge>();

        foreach (var t in Tetrahedra)
        {
            var abc = new Triangle(t.A, t.B, t.C);
            var abd = new Triangle(t.A, t.B, t.D);
            var acd = new Triangle(t.A, t.C, t.D);
            var bcd = new Triangle(t.B, t.C, t.D);

            if (triangleSet.Add(abc))
            {
                Triangles.Add(abc);
            }

            if (triangleSet.Add(abd))
            {
                Triangles.Add(abd);
            }

            if (triangleSet.Add(acd))
            {
                Triangles.Add(acd);
            }

            if (triangleSet.Add(bcd))
            {
                Triangles.Add(bcd);
            }

            var ab = new Edge(t.A, t.B);
            var bc = new Edge(t.B, t.C);
            var ca = new Edge(t.C, t.A);
            var da = new Edge(t.D, t.A);
            var db = new Edge(t.D, t.B);
            var dc = new Edge(t.D, t.C);

            if (edgeSet.Add(ab))
            {
                Edges.Add(ab);
            }

            if (edgeSet.Add(bc))
            {
                Edges.Add(bc);
            }

            if (edgeSet.Add(ca))
            {
                Edges.Add(ca);
            }

            if (edgeSet.Add(da))
            {
                Edges.Add(da);
            }

            if (edgeSet.Add(db))
            {
                Edges.Add(db);
            }

            if (edgeSet.Add(dc))
            {
                Edges.Add(dc);
            }
        }
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
        float deltaMax = Mathf.Max(dx, dy, dz) * 20; // 20 needs tests!!!!

        Vector3 p1 = new Vector3(minX - 10, minY - 10, minZ - 10); // 10 needs tests !!!!!
        Vector3 p2 = new Vector3(maxX + deltaMax, minY - 1, minZ - 1);
        Vector3 p3 = new Vector3(minX - 1, maxY + deltaMax, minZ - 1);
        Vector3 p4 = new Vector3(minX - 1, minY - 1, maxZ + deltaMax);

        THETETRAHEDRON = new Tetrahedron(p1, p2, p3, p4);
        Tetrahedra.Add(THETETRAHEDRON);
    }
}