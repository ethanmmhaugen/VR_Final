using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Face : Grabbable
{
    public List<Line> lines = new List<Line>();
    public List<Vertex> vertices = new List<Vertex>();
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Vector3 midpoint;

    private List<int> triangles;

    private Mesh mesh;

    //TRIGGERS to make a face is pinch/grab a line 
    
    // Start is called before the first frame update
    void Start()
    {
        if(lines.Count < 3 || vertices.Count < 3 || lines.Count != vertices.Count) {
            Debug.LogError("illegal face");
        }
        SetupMeshRenderer();
        /*gameObject.tag = "Drawable";
        DebugText.log("Setting Tag to drawable in line");
        localHitbox = gameObject.AddComponent<CapsuleCollider>();*/
    }

    // Update is called once per frame
    void Update()
    {
        reRender();
    }
    public override Grabbable Pinch()
    {
        throw new System.NotImplementedException();
    }
    
    public override Grabbable Grab(Transform grabbingTrans)
    {
        throw new System.NotImplementedException();
    }

    private void SetupMeshRenderer(){
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();
        reRender();
    }

    public void reRender(){
        recalcMidpoint();
        drawTriangles();
    }

    public void recalcMidpoint(){
        float x = 0;
        float y = 0;
        float z = 0;
        foreach(Vertex vertex in vertices){
            x += vertex.transform.position.x;
            y += vertex.transform.position.y;
            z += vertex.transform.position.z;
        }

        midpoint = new Vector3(x/vertices.Count, y/vertices.Count, z/vertices.Count);

    }

    public int findIndex(Vector3[] verts, Vertex vertex){
        for(int i = 0; i<verts.Length; i++){
            if(vertex.transform.position == verts[i]){
                return i;
            }
        }
        DebugText.log("Hitting error Face ln 88");
        return -1;
    }
    public void drawTriangles(){
        triangles = new List<int>();
        Vector3[] vertpos = new Vector3[vertices.Count+1];
        vertpos[0] = midpoint;
        for(int i = 0; i<vertices.Count; i++){
            vertpos[i+1] = vertices[i].transform.position;
        }

        for(int i = 0; i<lines.Count; i++){
            triangles.Add(findIndex(vertpos, lines[i].vert1.GetComponent<Vertex>()));
            triangles.Add(findIndex(vertpos, lines[i].vert2.GetComponent<Vertex>()));
            triangles.Add(0);

            triangles.Add(0);
            triangles.Add(findIndex(vertpos, lines[i].vert2.GetComponent<Vertex>()));
            triangles.Add(findIndex(vertpos, lines[i].vert1.GetComponent<Vertex>()));
            
        }
        mesh = new Mesh();
        mesh.vertices = vertpos;
        mesh.triangles = triangles.ToArray();
        meshFilter.mesh = mesh;
    }

}
