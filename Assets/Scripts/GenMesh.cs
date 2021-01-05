using UnityEngine;
using System.Collections;

public class GenMesh : MonoBehaviour
{
    public float MeshSize;
    public int GridCount;

    // Use this for initialization
    void Start()
    {
        var mf = GetComponent<MeshFilter>();
        BuildMeshData(MeshSize, GridCount);
        mf.mesh = Gen();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public Mesh Mesh { get; private set; }

    private Vector3[] Vertices;
    private int[] Triangles;
    private Vector2[] UV;
    private Vector2[] UV2;

    private bool m_enableExtra;

    private int GetNoiseIndex(int x)
    {
        return x * (m_enableExtra ? 2 : 1) + (m_enableExtra ? 2 : 1);
    }

    public void BuildMeshData(float meshSize, int gridCount, bool enableExtra = false)
    {

        int verticeCount = gridCount + 1;

        if (verticeCount <= 0)
            verticeCount = 1;

        m_enableExtra = enableExtra;

        int vn = !enableExtra ? (verticeCount * verticeCount) : (verticeCount * verticeCount + gridCount * gridCount);
        Vertices = new Vector3[vn];
        int triCoe = enableExtra ? 12 : 6;
        Triangles = new int[verticeCount * verticeCount * triCoe];
        UV = new Vector2[vn];
        UV2 = new Vector2[vn];

        float uu = 0, vv = 0;
        int extraBegin = verticeCount * verticeCount;

        for (int y = 0; y < verticeCount; y++)
        {
            vv = 0;
            for (int x = 0; x < verticeCount; x++)
            {
                Vertices[y * verticeCount + x] = new Vector3(
                    (x / (float)gridCount - 0.5f) * meshSize,
                    0,
                    (y / (float)gridCount - 0.5f) * meshSize);


                UV[y * verticeCount + x] = new Vector2(uu, vv);
                vv += 1f / verticeCount;
                UV2[y * verticeCount + x] = new Vector2(x / (float)gridCount, y / (float)gridCount);

                if (enableExtra)
                {
                    if (x < (verticeCount - 1) && y < (verticeCount - 1))
                    {
                        int extraIndex = extraBegin + y * (verticeCount - 1) + x;
                        Vertices[extraIndex] = new Vector3(
                            ((x + 0.5f) / (float)gridCount - 0.5f) * meshSize,
                            0,
                            ((y + 0.5f) / (float)gridCount - 0.5f) * meshSize
                            );

                        UV[extraIndex] = new Vector2(uu + 0.5f, vv - 0.5f);
                        UV2[extraIndex] = new Vector2((x + 0.5f) / (float)verticeCount, (y + 0.5f) / (float)verticeCount);
                    }
                }
            }
            uu += 1f / verticeCount;
        }

        for (int i = 0; i < verticeCount - 1; i++)
        {
            for (int j = 0; j < verticeCount - 1; j++)
            {
                int lbIndex = i * verticeCount + j;
                int rbIndex = i * verticeCount + j + 1;
                int ltIndex = (i + 1) * verticeCount + j;
                int rtIndex = (i + 1) * verticeCount + j + 1;
                if (enableExtra)
                {
                    int midIndex = extraBegin + i * (verticeCount - 1) + j;

                    Triangles[(i * verticeCount + j) * triCoe] = lbIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 1] = midIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 2] = rbIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 3] = rbIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 4] = midIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 5] = rtIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 6] = rtIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 7] = midIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 8] = ltIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 9] = ltIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 10] = midIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 11] = lbIndex;
                }
                else
                {
                    Triangles[(i * verticeCount + j) * triCoe] = lbIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 1] = ltIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 2] = rbIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 3] = rbIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 4] = ltIndex;
                    Triangles[(i * verticeCount + j) * triCoe + 5] = rtIndex;
                }
            }
        }
    }

    public Mesh Gen()
    {
        if (Mesh != null)
            return Mesh;

        Mesh m = new Mesh();
        m.vertices = Vertices;
        m.triangles = Triangles;
        m.uv = UV;
        m.uv2 = UV2;
        Mesh = m;
        /*
        UnityEditor.AssetDatabase.CreateAsset(m, Application.dataPath + "/test.obj");
        UnityEditor.AssetDatabase.SaveAssets();
        */
        return m;
    }
}
