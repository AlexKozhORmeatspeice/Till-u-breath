using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Script;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

//blya eto polni pizdec
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private Mesh hexMesh;

    [NonSerialized] private List<Vector3> verticies;
    [NonSerialized] private List<int> indBuffer;
    [NonSerialized] private List<Color> colors;
    [NonSerialized] private List<Vector2> uvs;

    private MeshCollider meshCollider;

    public bool useCollider, useColors, useUV;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "Hex mesh";

        if (useCollider)
            meshCollider = gameObject.AddComponent<MeshCollider>();
    }


    public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = verticies.Count;
        verticies.Add(HexMetrics.Perturb(v1));
        verticies.Add(HexMetrics.Perturb(v2));
        verticies.Add(HexMetrics.Perturb(v3));

        indBuffer.Add(vertexIndex);
        indBuffer.Add(vertexIndex + 1);
        indBuffer.Add(vertexIndex + 2);
    }

    public void AddTriangleUnpertubed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = verticies.Count;
        verticies.Add(v1);
        verticies.Add(v2);
        verticies.Add(v3);

        indBuffer.Add(vertexIndex);
        indBuffer.Add(vertexIndex + 1);
        indBuffer.Add(vertexIndex + 2);
    }

    public void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    public void AddTriangleColors(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector2 uv3)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
    }


    public void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = verticies.Count;
        verticies.Add(HexMetrics.Perturb(v1));
        verticies.Add(HexMetrics.Perturb(v2));
        verticies.Add(HexMetrics.Perturb(v3));
        verticies.Add(HexMetrics.Perturb(v4));
        
        indBuffer.Add(vertexIndex);
        indBuffer.Add(vertexIndex + 2);
        indBuffer.Add(vertexIndex + 1);
        indBuffer.Add(vertexIndex + 1);
        indBuffer.Add(vertexIndex + 2);
        indBuffer.Add(vertexIndex + 3);
    }
    public void AddQuadUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = verticies.Count;
        verticies.Add((v1));
        verticies.Add((v2));
        verticies.Add((v3));
        verticies.Add((v4));
        
        indBuffer.Add(vertexIndex);
        indBuffer.Add(vertexIndex + 2);
        indBuffer.Add(vertexIndex + 1);
        indBuffer.Add(vertexIndex + 1);
        indBuffer.Add(vertexIndex + 2);
        indBuffer.Add(vertexIndex + 3);
    }

    public void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    public void AddQuadColors(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
    
    public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector2 uv3, Vector2 uv4)
    {
        uvs.Add(uv1);
        uvs.Add(uv2);
        uvs.Add(uv3);
        uvs.Add(uv4);
    }
    public void AddQuadUV(float uMin, float uMax, float vMin, float vMax)
    {
        uvs.Add(new Vector2(uMin, vMin));
        uvs.Add(new Vector2(uMax, vMin));
        uvs.Add(new Vector2(uMin, vMax));
        uvs.Add(new Vector2(uMax, vMax));
    }

    

    public void Clear()
    {
        hexMesh.Clear();
        verticies = ListPool<Vector3>.Get();
        indBuffer = ListPool<int>.Get();
        
        if(useColors)
            colors = ListPool<Color>.Get();

        if (useUV)
            uvs = ListPool<Vector2>.Get();
    }

    public void Apply()
    {
        hexMesh.SetVertices(verticies);
        ListPool<Vector3>.Add(verticies);

        if (useColors)
        {
            hexMesh.SetColors(colors);
            ListPool<Color>.Add(colors);
        }

        hexMesh.SetTriangles(indBuffer, 0);
        ListPool<int>.Add(indBuffer);
        
        hexMesh.RecalculateNormals();
        
        if(useCollider)
            meshCollider.sharedMesh = hexMesh;

        if (useUV)
        {
            hexMesh.SetUVs(0, uvs);
            ListPool<Vector2>.Add(uvs);
        }
    }
}
