using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using System.IO;
using System;


public class MyFramework : MonoBehaviour
{
    public Texture textureDong;
    public GameObject[] Dongs;
    public ZEDData myJson;

    // Start is called before the first frame update
    void Start()
    {
        // json 데이터 받아 오기
        string jsonString = File.ReadAllText(Application.dataPath + "/Samples/json/Dong.json");
        myJson = JsonUtility.FromJson<ZEDData>(jsonString);

        Dongs = new GameObject[myJson.data.Length];


        /*
        int i = 0;
        foreach (Data data in myJson.data)
        {
            foreach(Roomtypes roomtypes in data.roomtypes)
            {
                
                string roomName = data.meta.동.ToString() + roomtypes.meta.룸타입.ToString();

                List<Vector3> poly = GetPolyData(roomtypes.coordinatesBase64s);
                Mesh mesh = CreateMesh(poly);


                GameObject obj = new GameObject(roomName);
                Dongs[i++] = obj;
                //i++;
                RoomComponent newDong = obj.AddComponent<RoomComponent>();

                MeshFilter Filter = obj.AddComponent<MeshFilter>();
                MeshRenderer Renderer = obj.AddComponent<MeshRenderer>();

                Filter.mesh = mesh;
                Renderer.material = CreateMat(mesh);
            }
        }*/
        
        int i = 0;
        List<Vector3> poly = new List<Vector3>();

        foreach (Data data in myJson.data)
        {
            poly.Clear();
            foreach (Roomtypes roomtypes in data.roomtypes)
            {
                poly.AddRange(GetPolyData(roomtypes.coordinatesBase64s));

            }

            string roomName = data.meta.동.ToString() + i;

            Mesh mesh = CreateMesh(poly);
            GameObject obj = new GameObject(roomName);
            Dongs[i++] = obj;

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
        
        /*
        for( int i = 0; i < myJson.data.Length; i++)
        {
            testIndex(i);
        }
        */
        



    }
    void testIndex(int index)
    {
        // 정점 데이터 정리
        foreach (var data in myJson.data[index].roomtypes)
        {

            List<Vector3> poly = GetPolyData(data.coordinatesBase64s);
            Mesh mesh = CreateMesh(poly);

            GameObject obj = new GameObject();
            MeshFilter Filter = obj.AddComponent<MeshFilter>();
            MeshRenderer Renderer = obj.AddComponent<MeshRenderer>();

            Filter.mesh = mesh;
            Renderer.material = CreateMat(mesh);

        }
    }
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


        //uv좌표 계산
        List<Vector3> UpRectList = new List<Vector3>();
        List<Vector3> BotRectList = new List<Vector3>();

        List<Vector3> SideRectList = new List<Vector3>();

        List<Vector3> ForwaradRectList = new List<Vector3>();
        List<Vector3> BackRectList = new List<Vector3>();

        Vector3 ForwardNormal = new Vector3();

        // 특정면 좌표 정리
        for (var i = 0; i < vertices.Length; i++)
        {

            // 위쪽면
            if (Vector3.Angle(mesh.normals[i], Vector3.up) == 0 || Vector3.Angle(mesh.normals[i], Vector3.down) == 0)
            {
                UpRectList.Add(vertices[i]);
            }
            /*
            // 아래쪽 면
            else if (Vector3.Angle(mesh.normals[i], Vector3.down) == 0)
            {
                BotRectList.Add(vertices[i]);
            }*/
            // 아파트 정면 점들 정리
            else if (180 <= ContAngle(Vector3.forward, mesh.normals[i]) && ContAngle(Vector3.forward, mesh.normals[i]) <= 220)
            {
                if (ForwardNormal.magnitude == 0) ForwardNormal = mesh.normals[i]; // 건물의 정면 벡터

                ForwaradRectList.Add(vertices[i]);
            }
            else if (180 <= ContAngle(Vector3.forward, -mesh.normals[i]) && ContAngle(Vector3.forward, -mesh.normals[i]) <= 220)
            {
                BackRectList.Add(vertices[i]);
            }
            else
            {
                SideRectList.Add(vertices[i]);
            }
        }

        UpRectList          = UpRectList.Distinct().ToList();
        BotRectList         = BotRectList.Distinct().ToList();
        SideRectList        = SideRectList.Distinct().ToList();
        ForwaradRectList    = ForwaradRectList.Distinct().ToList();
        BackRectList        = BackRectList.Distinct().ToList();

        UpRectList.Sort((a, b)          => a.x.CompareTo(b.x));
        BotRectList.Sort((a, b)         => a.x.CompareTo(b.x));
        SideRectList.Sort((a, b)        => a.x.CompareTo(b.x));
        ForwaradRectList.Sort((a, b)    => a.x.CompareTo(b.x));
        BackRectList.Sort((a, b)        => a.x.CompareTo(b.x));

        List<Vector2> UV_UP_List = GetUpUV(UpRectList, ForwardNormal);
        //List<Vector2> UV_Bot_List = GetUpUV(BotRectList, ForwardNormal);

        List<Vector2> UV_Forward_List = new List<Vector2>();
        List<Vector2> UV_Back_List = new List<Vector2>(); ;

        if (ForwaradRectList.Count != 0) UV_Forward_List = GetForwardUV(ForwaradRectList, ForwardNormal);
        if (BackRectList.Count != 0) UV_Back_List = GetBackUV(BackRectList, ForwardNormal);

        List<Vector2> UV_Side_List = GetSideUV(SideRectList, ForwardNormal);

        Vector2[] uvs = new Vector2[vertices.Length];
        for (var i = 0; i < vertices.Length; i++)
        {

            // 건물 바닥과 뚜껑 설정
            if (Vector3.Angle(mesh.normals[i], Vector3.up) == 0 || Vector3.Angle(mesh.normals[i], Vector3.down) == 0)
            {
                int UVIndex = UpRectList.FindIndex(vec => vec.x == vertices[i].x && vec.z == vertices[i].z);
                uvs[i] = UV_UP_List[UVIndex];

            }/*
            else if (Vector3.Angle(mesh.normals[i], Vector3.down) == 0)
            {
                int UVIndex = BotRectList.FindIndex(vec => vec.x == vertices[i].x && vec.z == vertices[i].z);
                uvs[i] = UV_Bot_List[UVIndex];

            }*/
            // 건물 정면
            
            else if (180 <= ContAngle(Vector3.forward, mesh.normals[i]) && ContAngle(Vector3.forward, mesh.normals[i]) <= 220)
            {
                int UVIndex = ForwaradRectList.FindIndex(vec => vec.x == vertices[i].x && vec.y == vertices[i].y);
                uvs[i] = UV_Forward_List[UVIndex];
            }
            else if (180 <= ContAngle(Vector3.forward, -mesh.normals[i]) && ContAngle(Vector3.forward, -mesh.normals[i]) <= 220)
            {
                int UVIndex = BackRectList.FindIndex(vec => vec.x == vertices[i].x && vec.y == vertices[i].y);
                uvs[i] = UV_Back_List[UVIndex];
            }
            else // 나머지 벽
            {
                int UVIndex = SideRectList.FindIndex(vec => vec.x == vertices[i].x && vec.y == vertices[i].y);
                uvs[i] = UV_Side_List[UVIndex];
            }       
            

        }
        
        mesh.uv = uvs;
        
        return mesh;
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
    List<Vector2> GetUpUV(List<Vector3> SortList, Vector3 ForwardNormal)
    {
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

            if (minVector.y > newVector.y)
                minVector.y = newVector.y;
            else if (maxVector.y < newVector.y)
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

            if (minVector.y > newVector.y)
                minVector.y = newVector.y;
            else if (maxVector.y < newVector.y)
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

        Vector2 minVector = new Vector2();
        Vector2 maxVector = new Vector2();

        // 행렬 이동 및 행렬 회전
        foreach (var v in SortList)
        {
            Vector3 newVector = v - moveVector;
            newVec.Add(newVector);
            newVector = Y_Rot.MultiplyVector(newVector);

            // 최소 최대값 탐색
            if (minVector.x > newVector.y)
                minVector.x = newVector.y;
            else if (maxVector.x < newVector.y)
                maxVector.x = newVector.y;

            if (minVector.y > newVector.x)
                minVector.y = newVector.x;
            else if (maxVector.y < newVector.x)
                maxVector.y = newVector.x;

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
