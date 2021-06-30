using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomComponent : MonoBehaviour
{
    public string dongNum;
    public int DB_ID;
    public float Room_ID;

    public int index;

    public float Height;

    //public MeshFilter meshFilter;
    //public MeshRenderer meshRenderer;

    public Mesh mesh;
    // Start is called before the first frame update
    private void Update()
    {
    }

    private void OnMouseDown()
    {
        GameObject obj = GameObject.Find("MyFrameWork");
        obj.GetComponent<MyFramework>().SelectObject(index);
        //Debug.Log("Click! " + index);
    }

}
