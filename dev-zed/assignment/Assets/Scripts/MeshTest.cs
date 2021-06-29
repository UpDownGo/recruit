using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour
{
    // Start is called before the first frame update
    public Material m_test;
    void Start()
    {

        Vector3[] vertices = new Vector3[] {    new Vector3(-1f, -1f, -1f), new Vector3(1f, -1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(-1f, 1f, -1f),
                                                new Vector3(-1f, -1f, 1f), new Vector3(1f, -1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f),
                                                new Vector3(1f, -1f, -1f), new Vector3(1f, -1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(1f, 1f, -1f),
                                                new Vector3(-1f, -1f, 1f), new Vector3(-1f, 1f, 1f), new Vector3(-1f, 1f, -1f), new Vector3(-1f, -1f, -1f),
                                                new Vector3(-1f, 1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f),
                                                new Vector3(1f, -1f, 1f), new Vector3(-1f, -1f, 1f), new Vector3(-1f, -1f, -1f), new Vector3(1f, -1f, -1f) };

        int[] triangles = new int[] {
            0, 3, 2, 0, 2, 1, 
            4, 5, 6, 4, 6, 7, 
            8, 10, 9, 8, 11, 10, 
            12, 13, 14, 12, 14, 15, 
            16, 18, 17, 16, 19, 18, 
            20, 21, 23, 23, 21, 22 };
        Mesh mesh = new Mesh();
        Vector2[] uvs = new Vector2[] { new Vector2(0.0f, 0.0f), new Vector2(0.5f, 0.0f), new Vector2(0.5f, 0.5f), new Vector2(0.0f, 0.5f),
                                        new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
                                        new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
                                        new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f),
                                        new Vector2(0.5f, 0.5f), new Vector2(1.0f, 0.5f), new Vector2(1.0f, 1.0f), new Vector2(0.5f, 1.0f),
                                        new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(0.0f, 1.0f)};

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
        //Material material = new Material(Shader.Find("Unlit/Texture"));
        //material.SetTexture("_MainTex", m_texture);
        //Material.SetTextureScale("_BaseMap", new Vector2(1f, 3));
        GetComponent<MeshRenderer>().material = m_test;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
