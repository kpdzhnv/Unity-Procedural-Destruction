using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Destructible : MonoBehaviour
{
    public Voronoi voronoi;
    public int count = 100;
    public float impactForce = 0;

    MeshFilter mf;
    Collider mc;
    Material mat_out;
    public  Material mat_in;
    public GameObject VFXPrefab;

    // Start is called before the first frame update  
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<Collider>();
        mat_out = GetComponent<MeshRenderer>().sharedMaterial;
        //mat_in = GetComponent<MeshRenderer>().sharedMaterials[1];
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            if (mc.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 9999f))
            {
                if (VFXPrefab!= null)
                    Instantiate(VFXPrefab, hit.point, Quaternion.identity);
                Break(hit.point);
            }
        }

        if (Input.GetButtonDown("Jump"))
            Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Break();
    }

    public void Explode()
    {
        if (VFXPrefab != null)
            Instantiate(VFXPrefab, mc.bounds.center, Quaternion.identity);
        // actual center of an object, not depending on the pivot
        Break(mc.bounds.center);
    }
    public void Break(Vector3 point)
    {
        voronoi = new Voronoi(mf.sharedMesh, mc.bounds, count, point);
        voronoi.Generate();

        foreach (VoronoiCell cell in voronoi.cells)
        {
            // instantiate part
            // transform
            var part = new GameObject("part");
            part.transform.position = this.transform.position;
            part.transform.rotation = this.transform.rotation;
            part.transform.localScale = this.transform.localScale;

            part.transform.SetParent(transform, false);
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
            part.transform.localScale = Vector3.one;

            // mesh
            var mesh = new Mesh();
            mesh.subMeshCount = 2;
            mesh.SetVertices(cell.vertices);
            mesh.SetTriangles(cell.triangles_out, 0);
            mesh.SetTriangles(cell.triangles_in, 1);
            mesh.SetNormals(cell.normals);

            part.AddComponent<MeshRenderer>().sharedMaterials = new Material[2] { mat_out, mat_in };
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshCollider pmc = part.AddComponent<MeshCollider>();
            pmc.convex = true;
            pmc.sharedMesh = mesh;

            Rigidbody prb = part.AddComponent<Rigidbody>();
            prb.mass = 0.5f;
            prb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            
            Matrix4x4 localToWorld = transform.localToWorldMatrix;
            
            prb.velocity = (localToWorld.MultiplyPoint3x4(cell.seed) - point) * impactForce;
            
        }

        var d = this.GetComponent<Destructible>();
        Destroy(d);
        Destroy(mc);
        Destroy(mf);
        var rb = this.GetComponent<Rigidbody>();
        if (rb)
            Destroy(rb);
    }

}
