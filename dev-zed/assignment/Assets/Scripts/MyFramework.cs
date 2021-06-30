using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UnityEngine.UI;
using System.IO;
using System;


public class MyFramework : MonoBehaviour
{
    public Texture textureDong;
    public GameObject[] Dongs;
    public ZEDData myJson;

    public Dropdown UIDrop;
    public Button UICreateBtn;
    public Button UIDeleteBtn;


    public int selectIndex;

    private void Awake()
    {
        // json 데이터 받아 오기
        string jsonString = File.ReadAllText(Application.dataPath + "/Samples/json/Dong.json");
        myJson = JsonUtility.FromJson<ZEDData>(jsonString);
        Dongs = new GameObject[myJson.data.Length];
    }
    // Start is called before the first frame update
    void Start()
    {
        // UI 설정
        List<string> names = new List<string>();

        foreach (var data in myJson.data)
            names.Add(data.meta.동);

        UIDrop.ClearOptions();
        UIDrop.AddOptions(names);
        SelectButton();
        /////////////////////////
        /// 
    }
    public void DestroyAll()
    {
        foreach(var dong in Dongs)
            Destroy(dong);
        UICreateBtn.interactable = true;
        UIDeleteBtn.interactable = false;
    }
    public void CreateAll()
    {
        for (int i = 0; i < myJson.data.Length; i++)
            CreateCombinedBuildingWithIndex(i);
        SelectButton();
    }
    public void SelectButton()
    {
        selectIndex = UIDrop.value;
        UICreateBtn.interactable = Dongs[selectIndex] == null;
        UIDeleteBtn.interactable = Dongs[selectIndex] != null;
            
    }
    public void OnBtnCreate()
    {
        if(Dongs[selectIndex] == null)
            CreateCombinedBuildingWithIndex(selectIndex);
        SelectButton();
    }
    public void OnBtnDelete()
    {
        if (Dongs[selectIndex] != null)
            Destroy(Dongs[selectIndex]);

        UICreateBtn.interactable = true;
        UIDeleteBtn.interactable = false;
    }
    public void SelectObject(int index)
    {
        UIDrop.value = index;
        SelectButton();
    }

    /** 룸데이터 별로 따로 오프젝트 생셩은 관리가 힘들듯...**/
    void CreateBuildingWithIndex(int index)
    {
        if (Dongs[index] != null) return;

        List<Vector3> poly = new List<Vector3>();

        Data data = myJson.data[index];

        // 정점 데이터 정리
        foreach (Roomtypes roomtypes in data.roomtypes)
        {
            poly.Clear();

            poly = GetPolyData(roomtypes.coordinatesBase64s);

            string roomName = data.meta.동.ToString() + index;

            Mesh mesh = CreateMesh(poly);
            GameObject obj = new GameObject(roomName);
            Dongs[index] = obj;

            RoomComponent newDong = obj.AddComponent<RoomComponent>();
            newDong.Height = mesh.bounds.center.y * 2;
            newDong.DB_ID = data.meta.bd_id;
            newDong.dongNum = data.meta.동;
            newDong.Room_ID = data.roomtypes[0].meta.룸타입id;


            MeshFilter Filter = obj.AddComponent<MeshFilter>();
            MeshRenderer Renderer = obj.AddComponent<MeshRenderer>();

            Filter.mesh = mesh;
            Renderer.material = CreateMat(mesh);
        }

        
    }
    void CreateCombinedBuildingWithIndex(int index)
    {
        if (Dongs[index] != null) return;
        
        List<Vector3> poly = new List<Vector3>();
        
        Data data = myJson.data[index];

        // 정점 데이터 정리
        foreach (Roomtypes roomtypes in data.roomtypes)
        {
            poly.AddRange(GetPolyData(roomtypes.coordinatesBase64s));
        }

        string roomName = data.meta.동.ToString() + index;

        Mesh mesh = CreateMesh(poly);
        GameObject obj = new GameObject(roomName);
        Dongs[index] = obj;

        RoomComponent newDong = obj.AddComponent<RoomComponent>();
        newDong.Height = mesh.bounds.center.y * 2;
        newDong.DB_ID = data.meta.bd_id;
        newDong.dongNum = data.meta.동;
        newDong.Room_ID = data.roomtypes[0].meta.룸타입id;
        newDong.index = index;

        MeshFilter Filter = obj.AddComponent<MeshFilter>();
        MeshRenderer Renderer = obj.AddComponent<MeshRenderer>();

        Filter.mesh = mesh;
        Renderer.material = CreateMat(mesh);
        obj.AddComponent<MeshCollider>();
    }
    /** 스트링 데이터를 Vectex로 변환 **/
    List<Vector3> GetPolyData(string[] stringData)
    {
        List<Vector3> vecList = new List<Vector3>();

        for (int i = 0; i < stringData.Length; i++)
        {
            byte[] arrByte = Convert.FromBase64String(stringData[i]);
            float[] arrFloat = new float[arrByte.Length / 4];
            Buffer.BlockCopy(arrByte, 0, arrFloat, 0, arrByte.Length);

            for (int j = 0; j < (arrFloat.Length / 3); j++)
            {
                float x = arrFloat[j * 3];
                float y = arrFloat[(j * 3) + 2];
                float z = arrFloat[(j * 3) + 1];
                Vector3 vec = new Vector3(x, y, z);
                vecList.Add(vec);
            }
        }
        return vecList;
    }
    /** 메쉬 생성 **/
    Mesh CreateMesh(List<Vector3> PolyList)
    {

        Vector3[] vertices = PolyList.ToArray();
        int[] triangles = new int[vertices.Length];

        // 정점 인덱스 넣기
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        Mesh mesh = new Mesh();

        mesh.vertices   = vertices;
        mesh.triangles  = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.uv = CalculationUVLoc(vertices, mesh);
        
        return mesh;
    }
    
    Vector2[] CalculationUVLoc(Vector3[] vertices, Mesh mesh)
    {
        //uv좌표 계산
        List<Vector3> UpRectList = new List<Vector3>();
        List<Vector3> SideRectList = new List<Vector3>();
        List<Vector3> ForwaradRectList = new List<Vector3>();
        List<Vector3> BackRectList = new List<Vector3>();

        //List<Vector3> UpRectList2 = new List<Vector3>();
        //List<Vector3> ForwaradRectList2 = new List<Vector3>();

        Vector3 ForwardNormal = new Vector3();

        // 특정면 좌표 정리
        for (var i = 0; i < vertices.Length; i++)
        {

            // 위쪽면
            if (Vector3.Angle(mesh.normals[i], Vector3.up) == 0 || Vector3.Angle(mesh.normals[i], Vector3.down) == 0)
            {
                UpRectList.Add(vertices[i]);
                //UpRectList2.Add(new Vector3(vertices[i].x, 0.0f, vertices[i].z));
            }
            // 아파트 정면 점들 정리
            else if (180 <= ContAngle(Vector3.forward, mesh.normals[i]) && ContAngle(Vector3.forward, mesh.normals[i]) <= 220)
            {
                if (ForwardNormal.magnitude == 0) ForwardNormal = mesh.normals[i]; // 건물의 정면 벡터

                ForwaradRectList.Add(vertices[i]);
                //ForwaradRectList2.Add(new Vector3(vertices[i].x, 0.0f, vertices[i].y));

            }/*
            else if (150 <= ContAngle(Vector3.forward, -mesh.normals[i]) && ContAngle(Vector3.forward, -mesh.normals[i]) <= 250)
            {
                BackRectList.Add(vertices[i]);
            }*/
            else
            {
                SideRectList.Add(vertices[i]);
            }
        }

        UpRectList = UpRectList.Distinct().ToList();
        UpRectList.Sort((a, b) => a.x.CompareTo(b.x));
        List<Vector2> UV_UP_List = GetUpUV(UpRectList, ForwardNormal);

        SideRectList = SideRectList.Distinct().ToList();

        //SideRectList.Sort((a, b) => a.z.CompareTo(b.z));
        SideRectList.Sort((a, b) => a.x.CompareTo(b.x));        
        SideRectList.Sort((a, b) => a.y.CompareTo(b.y));
        
        List<Vector2> UV_Side_List = GetSideUV(SideRectList, ForwardNormal);

        
        BackRectList = BackRectList.Distinct().ToList();
        BackRectList.Sort((a, b) => a.x.CompareTo(b.x));
        List<Vector2> UV_Back_List = new List<Vector2>();
        if (BackRectList.Count != 0) UV_Back_List = GetBackUV(BackRectList, ForwardNormal);


        ForwaradRectList = ForwaradRectList.Distinct().ToList();
        ForwaradRectList.Sort((a, b) => a.x.CompareTo(b.x));
        List<Vector2> UV_Forward_List = new List<Vector2>();
        if (ForwaradRectList.Count != 0) UV_Forward_List = GetForwardUV(ForwaradRectList, ForwardNormal);

        //ForwaradRectList2 = ForwaradRectList2.Distinct().ToList();
        // ForwaradRectList2.Sort((a, b) => a.x.CompareTo(b.x));
        // List<Vector2> UV_Forward_List2 = GetUV(ForwaradRectList2, ForwardNormal, new Vector2(0.0f, 0.5f), new Vector2(0.0f, 0.5f));

        //  UpRectList2 = UpRectList2.Distinct().ToList();
        // UpRectList2.Sort((a, b) => a.x.CompareTo(b.x));
        //List<Vector2> UV_UP_List2 = GetUV(UpRectList2, ForwardNormal, new Vector2(0.75f, 1.0f), new Vector2(0.0f, 0.5f));

        

        Vector2[] uvs = new Vector2[vertices.Length];

        for (var i = 0; i < vertices.Length; i++)
        {

            // 건물 바닥과 뚜껑 설정
            if (Vector3.Angle(mesh.normals[i], Vector3.up) == 0 || Vector3.Angle(mesh.normals[i], Vector3.down) == 0)
            {
                int UVIndex = UpRectList.FindIndex(vec => vec.x == vertices[i].x && vec.z == vertices[i].z);
                uvs[i] = UV_UP_List[UVIndex];

            }
            // 건물 정면

            else if (180 <= ContAngle(Vector3.forward, mesh.normals[i]) && ContAngle(Vector3.forward, mesh.normals[i]) <= 220)
            {
                int UVIndex = ForwaradRectList.FindIndex(vec => vec.x == vertices[i].x && vec.y == vertices[i].y);
                uvs[i] = UV_Forward_List[UVIndex];
            }/*
            else if (150 <= ContAngle(Vector3.forward, -mesh.normals[i]) && ContAngle(Vector3.forward, -mesh.normals[i]) <= 250)
            {
                int UVIndex = BackRectList.FindIndex(vec => vec.x == vertices[i].x && vec.y == vertices[i].y);
                uvs[i] = UV_Back_List[UVIndex];
            }*/
            else // 나머지 벽
            {
                int UVIndex = SideRectList.FindIndex(vec => vec.x == vertices[i].x && vec.y == vertices[i].y);
                uvs[i] = UV_Side_List[UVIndex];
            }
        }

        return uvs;
    }
    Material CreateMat(Mesh mesh)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.mainTexture = textureDong;

        float height = mesh.bounds.center.y * 2; // 건물 높이 조절        
        material.SetTextureScale("_BaseMap", new Vector2(1f, (int)height / 3));        
        return material;
    }
    Matrix4x4 GetYRotMatrix(Vector3 vec1, Vector3 vec2)
    {
        float angleV = ContAngle(vec1, -vec2);

        Matrix4x4 VecterRot = Matrix4x4.identity;

        VecterRot.m00 = Mathf.Cos(angleV * Mathf.Deg2Rad);
        VecterRot.m02 = -Mathf.Sin(angleV * Mathf.Deg2Rad);
        VecterRot.m20 = Mathf.Sin(angleV * Mathf.Deg2Rad);
        VecterRot.m22 = Mathf.Cos(angleV * Mathf.Deg2Rad);
        
        return VecterRot;
    }
    /* UV 좌표를 계산하는 함수 */
    List<Vector2> GetUV(List<Vector3> SortList, Vector3 ForwardNormal, Vector2 LerpU, Vector2 LerpV)
    {
        List<Vector2> UVList = new List<Vector2>();

        if (SortList.Count == 0) return UVList;

        Vector3 moveVector = SortList[0] - Vector3.zero;
        List<Vector3> newVec = new List<Vector3>();

        Matrix4x4 VecterRot = GetYRotMatrix(Vector3.forward, ForwardNormal);


        Vector2 minVector = new Vector2();
        Vector2 maxVector = new Vector2();

        // 행렬 이동 및 행렬 회전
        foreach (var v in SortList)
        {
            Vector3 newVector = v - moveVector;
            newVector = VecterRot.MultiplyVector(newVector);

            newVec.Add(newVector);
            // 최소 최대값 탐색
            if (minVector.x > newVector.x)
                minVector.x = newVector.x;
            else if (maxVector.x < newVector.x)
                maxVector.x = newVector.x;

            if (minVector.y > newVector.z)
                minVector.y = newVector.z;
            else if (maxVector.y < newVector.z)
                maxVector.y = newVector.z;

            GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = newVector;
        }


        
        foreach (var vec in newVec)
        {
            float lerpX = vec.x / maxVector.x;
            float lerpY = vec.z / maxVector.y;
            if (lerpX < 0) lerpX = 0;
            if (lerpY < 0) lerpX = 0;
            float u = Mathf.Lerp(LerpU.x, LerpU.y, Mathf.InverseLerp(0.0f, 1f, vec.x / maxVector.x));
            float v = Mathf.Lerp(LerpV.x, LerpV.y, Mathf.InverseLerp(0.0f, 1f, vec.z / maxVector.y));
            UVList.Add(new Vector2(u, v));
        }

        return UVList;
    }
    List<Vector2> GetUpUV(List<Vector3> SortList, Vector3 ForwardNormal)
    {
        Vector3 moveVector = SortList[0] - Vector3.zero;
        List<Vector3> newVec = new List<Vector3>();

        Matrix4x4 VecterRot = GetYRotMatrix(Vector3.forward, ForwardNormal);

        Vector2 maxVector = new Vector2();

        // 행렬 이동 및 행렬 회전
        foreach (var v in SortList)
        {
            Vector3 newVector = v - moveVector;
            
            newVector = VecterRot.MultiplyVector(newVector);
            newVec.Add(newVector);

            // 최대값 탐색
            if (maxVector.x < newVector.x)
                maxVector.x = newVector.x;

            if (maxVector.y < newVector.z)
                maxVector.y = newVector.z;
           // GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = newVector;
        }


        List<Vector2> UVList = new List<Vector2>();
        foreach (var vec in newVec)
        {
            float u = Mathf.Lerp(0.75f, 1.0f, Mathf.InverseLerp(0.0f, 1f, vec.x / maxVector.x));
            float v = Mathf.Lerp(0.0f, 0.5f, Mathf.InverseLerp(0.0f, 1f, vec.z / maxVector.y));
            

            UVList.Add(new Vector2(u, v));
        }
        return UVList;
    }
    List<Vector2> GetForwardUV(List<Vector3> SortList, Vector3 ForwardNormal)
    {
        Vector3 moveVector = SortList[0] - Vector3.zero;
        List<Vector3> newVec = new List<Vector3>();

        Matrix4x4 VecterRot = GetYRotMatrix(Vector3.forward, ForwardNormal);


        Vector2 maxVector = new Vector2();

        // 행렬 이동 및 행렬 회전
        foreach (var v in SortList)
        {
            Vector3 newVector = v - moveVector;
            newVector = VecterRot.MultiplyVector(newVector);

            newVec.Add(newVector);
            // 최소 최대값 탐색
            if (maxVector.x < newVector.x)
                maxVector.x = newVector.x;

            if (maxVector.y < newVector.y)
                maxVector.y = newVector.y;
        }

        List<Vector2> UVList = new List<Vector2>();

        foreach (var vec in newVec)
        {
            float u = Mathf.Lerp(0.0f, 0.5f, Mathf.InverseLerp(0.0f, 1f, vec.x / maxVector.x));
            float v = Mathf.Lerp(0.0f, 0.5f, Mathf.InverseLerp(0.0f, 1f, vec.y / maxVector.y));

            UVList.Add(new Vector2(u, v));
        }

        return UVList;
    }
    List<Vector2> GetBackUV(List<Vector3> SortList, Vector3 ForwardNormal)
    {
        Vector3 moveVector = SortList[0] - Vector3.zero;
        List<Vector3> newVec = new List<Vector3>();

        Matrix4x4 VecterRot = GetYRotMatrix(Vector3.forward, ForwardNormal);


        Vector2 maxVector = new Vector2();

        // 행렬 이동 및 행렬 회전
        foreach (var v in SortList)
        {
            Vector3 newVector = v - moveVector;
            newVector = VecterRot.MultiplyVector(newVector);

            newVec.Add(newVector);
            // 최소 최대값 탐색
            if (maxVector.x < newVector.x)
                maxVector.x = newVector.x;

            if (maxVector.y < newVector.y)
                maxVector.y = newVector.y;

            //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = newVector;
        }


        List<Vector2> UVList = new List<Vector2>();

        foreach (var vec in newVec)
        {
            float u = Mathf.Lerp(0.5f, 0.75f, Mathf.InverseLerp(0.0f, 1f, vec.x / maxVector.x));
            float v = Mathf.Lerp(0.0f, 0.5f, Mathf.InverseLerp(0.0f, 1f, vec.y / maxVector.y));

            UVList.Add(new Vector2(u, v));
        }

        return UVList;
    }
    List<Vector2> GetSideUV(List<Vector3> SortList, Vector3 ForwardNormal)
    {
        Vector3 moveVector = SortList[0] - Vector3.zero;
        List<Vector3> newVec = new List<Vector3>();

        Matrix4x4 Y_Rot = GetYRotMatrix(Vector3.forward, ForwardNormal);

        Vector2 maxVector = new Vector2();

        // 행렬 이동 및 행렬 회전
        foreach (var v in SortList)
        {
            Vector3 newVector = v - moveVector;
            newVec.Add(newVector);
            newVector = Y_Rot.MultiplyVector(newVector);

            // 최소 최대값 탐색

            if (maxVector.x < newVector.x)
                maxVector.x = newVector.x;

            if (maxVector.y < newVector.y)
                maxVector.y = newVector.y;
            //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = newVector;
        }


        List<Vector2> UVList = new List<Vector2>();

        foreach (var vec in newVec)
        {   
            
            float u = Mathf.Lerp(0.5f, 0.75f, Mathf.InverseLerp(0.0f, 1f, Math.Abs(vec.x) / maxVector.x));
            float v = Mathf.Lerp(0.0f, 0.5f, Mathf.InverseLerp(0.0f, 1f, Math.Abs(vec.y) / maxVector.y));
            //Debug.Log(vec + " : " + u + "," + v);
            UVList.Add(new Vector2(u, v));
        }

        return UVList;
    }

    public float ContAngle(Vector3 fwd, Vector3 targetDir)
    {
        float angle = Vector3.Angle(fwd, targetDir);

        if (AngleDir(fwd, targetDir, Vector3.up) == -1)
        {
            angle = 360.0f - angle;
            if (angle > 359.9999f)
                angle -= 360.0f;
            return angle;
        }
        else
            return angle;
    }

    public int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0)
            return 1;
        else if (dir < 0.0)
            return -1;
        else
            return 0;
    }
}


[System.Serializable]
public class Meta2
{
    public int 룸타입id;
}

[System.Serializable]
public class Roomtypes
{
    public string[] coordinatesBase64s;
    public Meta2 meta;
}

[System.Serializable]
public class Meta1
{
    public int bd_id;
    public string 동;
    public int 지면높이;
}

[System.Serializable]
public class Data
{
    public Roomtypes[] roomtypes;
    public Meta1 meta;
}

[System.Serializable]
public class ZEDData
{
    public bool success;
    public int code;
    public Data[] data;
}
