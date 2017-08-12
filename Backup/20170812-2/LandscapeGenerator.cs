using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle {
    public readonly List<Vector3> Points;

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Points = new List<Vector3> { p1, p2, p3 };
    }
}

public class Face
{
    public readonly List<Triangle> Triangles;

    public Face(Triangle t1, Triangle t2)
    {
        Triangles = new List<Triangle> { t1, t2 };
    }

    public List<Vector3> ToPoints()
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var t in Triangles)
        {
            points.AddRange(t.Points);
        }
        return points;
    }
}

public class Faces
{
    private List<Face> _Faces;

    public Faces(List<Face> faces)
    {
        _Faces = faces;
    }

    public List<Vector3> ToPoints()
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var f in _Faces)
        {
            points.AddRange(f.ToPoints());
        }
        return points;
    }
}

public class Grid
{
    private Vector3[,] _Grid;
    private int ElementCountX; //elementCount = pointCount - 1
    private int ElementCountZ;

    public Grid(int elementCountX, int elementCountZ, float elementSizeX, float elementSizeZ)
    {
        ElementCountX = elementCountX;
        ElementCountZ = elementCountZ;
        float translateX = elementCountX * elementSizeX * 0.5f;
        float translateZ = elementCountZ * elementSizeZ * 0.5f;
        _Grid = new Vector3[elementCountX+1, elementCountZ+1];

        for (int i = 0; i <= elementCountX; i++)
        {
            for (int j = 0; j <= elementCountZ; j++)
            {
                float x = (i * elementSizeX) - translateX;
                float z = (j * elementSizeZ) - translateZ;
                _Grid[i, j] = new Vector3(x, 0f, z);
            }
        }
    }

    public Faces ToFaces()
    {
        List<Face> faces = new List<Face>();

        for (int i = 0; i < ElementCountX; i++)
        {
            for (int j = 0; j < ElementCountZ; j++)
            {
                var t1 = new Triangle(_Grid[i, j + 1], _Grid[i + 1, j], _Grid[i, j]);
                var t2 = new Triangle(_Grid[i + 1, j + 1], _Grid[i + 1, j], _Grid[i, j + 1]);
                faces.Add(new Face(t1, t2));
            }
        }

        return new Faces(faces);
    }

    public List<Vector3> ToPoints()
    {
        return ToFaces().ToPoints();
    }

    public Mesh ToMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = ToPoints().ToArray();

        int[] triangles = new int[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            triangles[i] = i;
        }
        mesh.triangles = triangles;

        return mesh;
    }
}

public class LandscapeGenerator : MonoBehaviour {

    public int count_x = 4;
    public int count_y = 4;

    public float size_x = 10;
    public float size_y = 10;

    void Start () {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Grid(count_x, count_y, size_x, size_y).ToMesh();
        meshFilter.mesh = mesh;
    }

}
