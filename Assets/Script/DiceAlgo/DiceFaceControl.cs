using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DiceFaceControl : MonoBehaviour
{
    [SerializeField]
    int desiredFaceCount = 20;

    private static readonly float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

    private static readonly Vector3[] tetrahedronVertices = {
        new Vector3(1, 1, 1).normalized,
        new Vector3(-1, -1, 1).normalized,
        new Vector3(-1, 1, -1).normalized,
        new Vector3(1, -1, -1).normalized
    };

    private static readonly int[][] tetrahedronFaces = {
        new int[] {0, 1, 2},
        new int[] {0, 3, 1},
        new int[] {0, 2, 3},
        new int[] {1, 3, 2}
    };

    private static readonly Vector3[] octahedronVertices = {
        new Vector3(1, 0, 0).normalized,
        new Vector3(-1, 0, 0).normalized,
        new Vector3(0, 1, 0).normalized,
        new Vector3(0, -1, 0).normalized,
        new Vector3(0, 0, 1).normalized,
        new Vector3(0, 0, -1).normalized
    };

    private static readonly int[][] octahedronFaces = {
        new int[] {0, 2, 4},
        new int[] {0, 4, 3},
        new int[] {0, 3, 5},
        new int[] {0, 5, 2},
        new int[] {1, 4, 2},
        new int[] {1, 3, 4},
        new int[] {1, 5, 3},
        new int[] {1, 2, 5}
    };

    private static readonly Vector3[] icosahedronVertices = {
        new Vector3(-1,  t,  0).normalized,
        new Vector3( 1,  t,  0).normalized,
        new Vector3(-1, -t,  0).normalized,
        new Vector3( 1, -t,  0).normalized,
        new Vector3( 0, -1,  t).normalized,
        new Vector3( 0,  1,  t).normalized,
        new Vector3( 0, -1, -t).normalized,
        new Vector3( 0,  1, -t).normalized,
        new Vector3( t,  0, -1).normalized,
        new Vector3( t,  0,  1).normalized,
        new Vector3(-t,  0, -1).normalized,
        new Vector3(-t,  0,  1).normalized
    };

    private static readonly int[][] icosahedronFaces = {
        new int[] {0, 11, 5},
        new int[] {0, 5, 1},
        new int[] {0, 1, 7},
        new int[] {0, 7, 10},
        new int[] {0, 10, 11},
        new int[] {1, 5, 9},
        new int[] {5, 11, 4},
        new int[] {11, 10, 2},
        new int[] {10, 7, 6},
        new int[] {7, 1, 8},
        new int[] {3, 9, 4},
        new int[] {3, 4, 2},
        new int[] {3, 2, 6},
        new int[] {3, 6, 8},
        new int[] {3, 8, 9},
        new int[] {4, 9, 5},
        new int[] {2, 4, 11},
        new int[] {6, 2, 10},
        new int[] {8, 6, 7},
        new int[] {9, 8, 1}
    };

    private List<Vector3> vertices;
    private List<int[]> faces;

    private void Subdivide()
    {
        List<int[]> faces2 = new List<int[]>();
        Dictionary<long, Vector3> middlePointCache = new Dictionary<long, Vector3>();

        foreach (var tri in faces)
        {
            int v1 = tri[0];
            int v2 = tri[1];
            int v3 = tri[2];

            int a = AddVertex(GetMiddlePoint(v1, v2, middlePointCache));
            int b = AddVertex(GetMiddlePoint(v2, v3, middlePointCache));
            int c = AddVertex(GetMiddlePoint(v3, v1, middlePointCache));

            faces2.Add(new int[] { v1, a, c });
            faces2.Add(new int[] { v2, b, a });
            faces2.Add(new int[] { v3, c, b });
            faces2.Add(new int[] { a, b, c });
        }

        faces = faces2;
    }

    private Vector3 GetMiddlePoint(int p1, int p2, Dictionary<long, Vector3> cache)
    {
        long key = ((long)Mathf.Min(p1, p2) << 32) + Mathf.Max(p1, p2);
        if (!cache.TryGetValue(key, out Vector3 middle))
        {
            Vector3 point1 = vertices[p1];
            Vector3 point2 = vertices[p2];
            middle = (point1 + point2) / 2.0f;
            cache[key] = middle;
        }
        return middle;
    }

    private int AddVertex(Vector3 vertex)
    {
        vertex.Normalize();
        vertices.Add(vertex);
        return vertices.Count - 1;
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        List<Vector3> meshNormals = new List<Vector3>();

        foreach (var tri in faces)
        {
            Vector3 v1 = vertices[tri[0]];
            Vector3 v2 = vertices[tri[1]];
            Vector3 v3 = vertices[tri[2]];

            int idx1 = meshVertices.Count;
            meshVertices.Add(v1);
            meshNormals.Add(Vector3.Cross(v2 - v1, v3 - v1).normalized);
            int idx2 = meshVertices.Count;
            meshVertices.Add(v2);
            meshNormals.Add(Vector3.Cross(v3 - v2, v1 - v2).normalized);
            int idx3 = meshVertices.Count;
            meshVertices.Add(v3);
            meshNormals.Add(Vector3.Cross(v1 - v3, v2 - v3).normalized);

            meshTriangles.Add(idx1);
            meshTriangles.Add(idx2);
            meshTriangles.Add(idx3);
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.normals = meshNormals.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Start()
    {
        // Choose the base polyhedron based on the closest match to the desired face count
        if (desiredFaceCount <= 4)
        {
            vertices = new List<Vector3>(tetrahedronVertices);
            faces = new List<int[]>(tetrahedronFaces);
        }
        else if (desiredFaceCount <= 8)
        {
            vertices = new List<Vector3>(octahedronVertices);
            faces = new List<int[]>(octahedronFaces);
        }
        else
        {
            vertices = new List<Vector3>(icosahedronVertices);
            faces = new List<int[]>(icosahedronFaces);
        }

        int subdivisions = 0;
        while (faces.Count < desiredFaceCount)
        {
            Subdivide();
            subdivisions++;
        }

        CreateMesh();
    }
}
