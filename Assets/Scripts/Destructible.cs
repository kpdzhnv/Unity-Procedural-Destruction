using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public Voronoi voronoi;
    bool debuggg;
    //public int count = 1;
    //public int size = 1;

    MeshFilter mf;
    Collider mc;

    // Start is called before the first frame update  
    void Start()
    {
        debuggg = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            mf = GetComponent<MeshFilter>();
            mc = GetComponent<Collider>();
            voronoi = new Voronoi(mf.sharedMesh, mc.bounds);

            voronoi.Generate();
            foreach(VoronoiCell cell in voronoi.cells)
            {
                // instantiate part
                var part = new GameObject("part");

                part.transform.SetParent(transform, false);
                part.transform.localPosition = Vector3.zero;
                part.transform.localRotation = Quaternion.identity;
                part.transform.localScale = Vector3.one;

                var mesh = new Mesh();
                mesh.SetVertices(cell.vertices);
                mesh.SetTriangles(cell.triangles, 0);
                mesh.SetNormals(cell.normals);

                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                part.AddComponent<MeshCollider>().sharedMesh = mesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().material;

                //part.AddComponent<Rigidbody>();
            }
            
            // ----------------------- enabling debugging info --------------------------------
            debuggg = true;

            //if (debuggg)
            //{
            //    Debug.Log("delaunay.Vertices.Count " + voronoi.delaunay.Vertices.Count);
            //    Debug.Log("delaunay.Edges.Count " + voronoi.delaunay.Edges.Count);
            //    Debug.Log("delaunay.Tetrahedra.Count " + voronoi.delaunay.Tetrahedra.Count);
            //}
        }

    }
    

    private void OnDrawGizmos()
    {
        //var l = GetComponent<MeshFilter>().sharedMesh.vertices;
        //Gizmos.color = Color.green;
        //foreach (var v in l)
        //{
        //    Vector3 worldPt = transform.TransformPoint(v);
        //    Gizmos.DrawSphere(v, 0.05f);
        //}



        //Gizmos.color = Color.green;
        //foreach (var v in mesh.sharedMesh.vertices)
        //{
        //    Gizmos.DrawSphere(transform.TransformPoint(v), 0.05f);
        //}
        //Gizmos.color = Color.black;
        //Gizmos.DrawSphere(transform.position, 0.05f);

        Gizmos.color = Color.white;
        if (debuggg)
        {
            // tetrahedralization
            foreach (var t in voronoi.delaunay.Tetrahedra)
            {
                Gizmos.DrawLine(voronoi.vertices[t.A], voronoi.vertices[t.B]);
                Gizmos.DrawLine(voronoi.vertices[t.B], voronoi.vertices[t.C]);
                Gizmos.DrawLine(voronoi.vertices[t.C], voronoi.vertices[t.D]);
                Gizmos.DrawLine(voronoi.vertices[t.D], voronoi.vertices[t.A]);
            }
            Gizmos.color = Color.red;

            // the big tetrahedron

            // IT IS REMOVED!!!!!!!!!!!!!!!!
            //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.A], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.B]);
            //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.A], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.C]);
            //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.A], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.D]);
            //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.B], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.C]);
            //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.B], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.D]);
            //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.C], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.D]);

            // red - centers of the tetrahedra
            // blue - centers of the voronoi cells

            foreach (var t in voronoi.delaunay.Tetrahedra)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(t.Circumcenter, 0.05f);

                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(voronoi.vertices[t.A], 0.02f);
                Gizmos.DrawSphere(voronoi.vertices[t.B], 0.02f);
                Gizmos.DrawSphere(voronoi.vertices[t.C], 0.02f);
                Gizmos.DrawSphere(voronoi.vertices[t.D], 0.02f);
            }

            //Gizmos.color = Color.blue;
            //foreach (var v in delaunay.Tetrahedra[1].GetVertices())
            //    Gizmos.DrawSphere(v, 0.05f);
        }
    }
}
