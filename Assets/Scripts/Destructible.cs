using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public Voronoi voronoi;
    public int count = 100;
    //public int size = 1;

    MeshFilter mf;
    Collider mc;

    // Start is called before the first frame update  
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        Break();
    }

    public void Break()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<Collider>();
        voronoi = new Voronoi(mf.sharedMesh, mc.bounds, count);
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
            MeshCollider pmc = part.AddComponent<MeshCollider>();
            pmc.convex = true;
            pmc.sharedMesh = mesh;

            Rigidbody prb = part.AddComponent<Rigidbody>();
            prb.mass = 0.5f;
            prb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        Destroy(this.gameObject);
    }
}
