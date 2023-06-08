using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Destructible : MonoBehaviour
{
    public Voronoi voronoi;
    public int count = 100;
    //public int size = 1;

    MeshFilter mf;
    Collider mc;
    public GameObject VFXPrefab;

    // Start is called before the first frame update  
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (mc.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 9999f))
            {
                Instantiate(VFXPrefab, hit.point, Quaternion.identity);
                Break(hit.point);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Break();
    }

    public void Break(Vector3 point)
    {
        voronoi = new Voronoi(mf.sharedMesh, mc.bounds, count, point);
        voronoi.Generate();

        foreach (VoronoiCell cell in voronoi.cells)
        {
            // instantiate part
            var part = new GameObject("part");

            part.transform.SetParent(transform.parent, false);
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = Vector3.one;

            var mesh = new Mesh();
            mesh.SetVertices(cell.vertices);
            mesh.SetTriangles(cell.triangles, 0);
            mesh.SetNormals(cell.normals);

            part.AddComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().material;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            //MeshCollider pmc = part.AddComponent<MeshCollider>();
            //pmc.convex = true;
            //pmc.sharedMesh = mesh;

            //Rigidbody prb = part.AddComponent<Rigidbody>();
            //prb.mass = 0.5f;
            //prb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        //Destroy(this.gameObject);
    }

    private void OnDrawGizmos()
    {
        // tetrahedralization
        foreach (var t in voronoi.delaunay.Tetrahedra)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(voronoi.vertices[t.A], voronoi.vertices[t.B]);
            Gizmos.DrawLine(voronoi.vertices[t.A], voronoi.vertices[t.C]);
            Gizmos.DrawLine(voronoi.vertices[t.A], voronoi.vertices[t.D]);
            Gizmos.DrawLine(voronoi.vertices[t.B], voronoi.vertices[t.C]);
            Gizmos.DrawLine(voronoi.vertices[t.B], voronoi.vertices[t.D]);
            Gizmos.DrawLine(voronoi.vertices[t.C], voronoi.vertices[t.D]);


            Gizmos.color = Color.red;
            Gizmos.DrawSphere(t.Circumcenter, 0.05f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(voronoi.vertices[t.A], 0.02f);
            Gizmos.DrawSphere(voronoi.vertices[t.B], 0.02f);
            Gizmos.DrawSphere(voronoi.vertices[t.C], 0.02f);
            Gizmos.DrawSphere(voronoi.vertices[t.D], 0.02f);

        }

        //var c = voronoi.cells[0];
        //Gizmos.color = Color.magenta;
        //foreach (var p in c.vertices)
        //    Gizmos.DrawSphere(p, 0.02f);

        //foreach (var c in voronoi.cells)
        //{
        //    Gizmos.DrawSphere(c.seed, 0.02f);

        //}

        // the big tetrahedron

       //Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.A], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.B]);
       // Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.A], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.C]);
       // Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.A], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.D]);
       // Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.B], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.C]);
       // Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.B], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.D]);
       // Gizmos.DrawLine(voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.C], voronoi.vertices[voronoi.delaunay.THETETRAHEDRON.D]);
    }
}
