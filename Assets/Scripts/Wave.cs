using UnityEngine;
using System.Collections;

public class Wave: MonoBehaviour
{
    public float MeshSize;
    public int GridCount;

    public Texture2D NoiseTex;

    public float WaveSpeed;
    public float WaveAmp;

    private Material m_mat;

    private Mesh m_mesh;

    // Use this for initialization
    void Awake()
    {
        GetComponent<MeshFilter>().mesh = Gen();
        m_mat = GetComponent<MeshRenderer>().sharedMaterial;
        float uvScale = 1 / MeshSize;
        m_mat.SetFloat("_UVScale", uvScale);
        float gridSize = MeshSize / (float)GridCount;
        m_mat.SetFloat("_GridSize", gridSize);
        Debug.Log(uvScale);
        Debug.Log(gridSize);
    }

    // Update is called once per frame
    void Update()
    {
        m_mat.SetFloat("_MyTime", Time.realtimeSinceStartup);
        //m_mesh.vertices = GenVertices();

        if(Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            var wavePos = GetWavePos(hit.point);
        }
    }

    public Vector3 GetWavePos(Vector3 worldPos, float yAmp = 1)
    {
        var localPox = transform.worldToLocalMatrix.MultiplyPoint3x4(worldPos);

        Color noise = NoiseTex.GetPixelBilinear(localPox.x / (float)MeshSize , localPox.z / (float)MeshSize);
        float co = noise.r * Time.realtimeSinceStartup * WaveSpeed;
        float p1 = Mathf.Sin(2*co) * WaveAmp;
        float x = localPox.x + p1;
        float y = Mathf.Cos(co) * WaveAmp * yAmp;
        float z = localPox.z + p1;

        return new Vector3(x,y,z) + transform.position;
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
        for (int z = 0; z < verticeCount; z++)
        {
            for (int x = 0; x < verticeCount; x++)
            {
                var localX = (x / (float)GridCount - 0.5f) * MeshSize;
                var localZ = (z / (float)GridCount - 0.5f) * MeshSize;

                /*
                Color noise = NoiseTex.GetPixelBilinear( x / (float)GridCount, z / (float)GridCount);
                float co = noise.r * Time.realtimeSinceStartup * WaveSpeed;
                float p1 = Mathf.Sin(2*co) * WaveAmp;
                vecs[z * verticeCount + x] = new Vector3(
                    localX + p1,
                    Mathf.Cos(co) * WaveAmp,
                    localZ 
                    );
                */
                vecs[z * verticeCount + x] = new Vector3( localX, 0, localZ);
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

    private Mesh Gen()
    {
        m_mesh = new Mesh();
        m_mesh.vertices = GenVertices();
        m_mesh.triangles = GenTriangles();
        m_mesh.uv = GenUV();

        return m_mesh;
    }
}
