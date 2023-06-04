using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public Voronoi voronoi;
    //public int count = 1;
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
        }

    }
}
