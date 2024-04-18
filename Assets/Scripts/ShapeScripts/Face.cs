using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Face : Grabbable
{
    public Line[] lines;
    public Vertex[] vertices;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;

    private Mesh mesh;

    //TRIGGERS to make a face is pinch/grab a line 
    
    // Start is called before the first frame update
    void Start()
    {
        if(lines.count < 3 || vertices.count < 3 || lines.count != vertices.count) {
            Debug.LogError("illegal face");
        }
        SetupMeshrenderer();
        /*gameObject.tag = "Drawable";
        DebugText.log("Setting Tag to drawable in line");
        localHitbox = gameObject.AddComponent<CapsuleCollider>();*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override Grabbable Pinch()
    {
        throw new System.NotImplementedException();
    }
    
    public override Grabbable Grab(Transform grabbingTrans)
    {
        throw new System.NotImplementedException();
    }

    private void setupMeshRenderer(){
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));

        meshFilter = gameObject.AddComponent<MeshFilter>();

        mesh = new Mesh()

        mesh.vertices = vertices;
        meshFilter.mesh = mesh;
    }

    public void reRender(){
        mesh.vertices = vertices
        meshFilter.mesh = mesh;
    }



}
