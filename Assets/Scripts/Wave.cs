using UnityEngine;
using System.Collections;

public class Wave: MonoBehaviour
{
    public float MeshSize;
    public int GridCount;

    private Material m_mat;

    // Use this for initialization
    void Start()
    {
        GetComponent<MeshFilter>().mesh = Gen();
        m_mat = GetComponent<MeshRenderer>().sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        m_mat.SetFloat("_MyTime", Time.realtimeSinceStartup);
    }
    private Mesh m_mesh;

    private int GetNoiseIndex(int x)
    {
        return x * 1 + 1;
    }

    private int[] GenTriangles()
    {
        int verticeCount = GridCount + 1;
        int triCoe = 6;
        int[] tris = new int[verticeCount * verticeCount * triCoe];

        for (int i = 0; i < verticeCount - 1; i++)
        {
            for (int j = 0; j < verticeCount - 1; j++)
            {
                int lbIndex = i * verticeCount + j;
                int rbIndex = i * verticeCount + j + 1;
                int ltIndex = (i + 1) * verticeCount + j;
                int rtIndex = (i + 1) * verticeCount + j + 1;

                tris[(i * verticeCount + j) * triCoe] = lbIndex;
                tris[(i * verticeCount + j) * triCoe + 1] = ltIndex;
                tris[(i * verticeCount + j) * triCoe + 2] = rbIndex;
                tris[(i * verticeCount + j) * triCoe + 3] = rbIndex;
                tris[(i * verticeCount + j) * triCoe + 4] = ltIndex;
                tris[(i * verticeCount + j) * triCoe + 5] = rtIndex;
            }
        }
        return tris;
    }

    private Vector3[] GenVertices()
    {
        int verticeCount = GridCount + 1;
        if (verticeCount <= 0)
            verticeCount = 1;

        int vn = verticeCount * verticeCount;
        var vecs = new Vector3[vn];
        for (int y = 0; y < verticeCount; y++)
        {
            for (int x = 0; x < verticeCount; x++)
            {
                vecs[y * verticeCount + x] = new Vector3(
                    (x / (float)GridCount - 0.5f) * MeshSize,
                    0,
                    (y / (float)GridCount - 0.5f) * MeshSize);
            }
        }
        return vecs;
    }

    private Vector2[] GenUV()
    {
        int verticeCount = GridCount + 1;

        if (verticeCount <= 0)
            verticeCount = 1;

        int vn = verticeCount * verticeCount;

        var uv = new Vector2[vn];

        for (int y = 0; y < verticeCount; y++)
        {
            for (int x = 0; x < verticeCount; x++)
            {
                uv[y * verticeCount + x] = new Vector2(x / (float)GridCount, y / (float)GridCount);
            }
        }
        return uv;
    }

    public Mesh Gen()
    {
        m_mesh = new Mesh();
        m_mesh.vertices = GenVertices();
        m_mesh.triangles = GenTriangles();
        m_mesh.uv = GenUV();

        return m_mesh;
    }
}
