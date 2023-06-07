using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{

    MeshFilter mf;
    Collider mc;
    // vertices that are used for the algorithms
    public List<Vector3> vertices;

    VoronoiCell cell;
    private void Start()
    {
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<Collider>();

        GenerateVertices();
        cell = new VoronoiCell(Vector3.zero, vertices);
    }

    // Update is called once per frame
    void Update()
    {
        var tris = mf.mesh.triangles;
        var verts = mf.mesh.vertices;
        for (int i = 0; i < tris.Length; i+=3)
        {
            Debug.DrawLine(verts[tris[i]], verts[tris[i + 1]], Color.black);
            Debug.DrawLine(verts[tris[i + 1]], verts[tris[i + 2]], Color.black);
            Debug.DrawLine(verts[tris[i]], verts[tris[i + 2]], Color.black);
        }


        if (cell != null)
        {
            for (int i = 0; i < cell.triangles.Count; i += 3)
            {
                Debug.DrawLine(cell.vertices[cell.triangles[i]], cell.vertices[cell.triangles[i + 1]], Color.cyan);
                Debug.DrawLine(cell.vertices[cell.triangles[i + 1]], cell.vertices[cell.triangles[i + 2]], Color.cyan);
                Debug.DrawLine(cell.vertices[cell.triangles[i]], cell.vertices[cell.triangles[i + 2]], Color.cyan);
            }
        }
        if (Input.GetButtonDown("Jump"))
        {


            cell.CutWithPlane(new Vector3(0.5f, 0.5f, 0),
                new Vector3(0, 0.5f, 0.5f),
                new Vector3(0.5f, -0.5f, 0.5f));

            //cell.CutWithPlane(new Vector3(0, 1, 0),
            //    new Vector3(0, -1, 0.5f),
            //    new Vector3(0, -1, -1));



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

    public void GenerateVertices()
    {
        // add initial mesh vertices 
        foreach (var p in mf.mesh.vertices)
        {
            if (!vertices.Contains(p))
                vertices.Add(p);
        }

        //int pointcount = 0;
        //while (pointcount != insidePointsCount)
        //{
        //    float x = Random.Range(minBounds.x, maxBounds.x);
        //    float y = Random.Range(minBounds.y, maxBounds.y);
        //    float z = Random.Range(minBounds.z, maxBounds.z);
        //    var v = new Vector3(x, y, z);
        //    if (IsInsideMesh(v))
        //    {
        //        vertices.Add(v);
        //        pointcount++;
        //    }
        //}
    }
}
