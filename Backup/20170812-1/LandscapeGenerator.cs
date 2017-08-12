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
    private int ElementCountX;
    private int ElementCountY;

    public Grid(int elementCountX, int elementCountY, float elementSizeX, float elementSizeY)
    {
        ElementCountX = elementCountX;
        ElementCountY = elementCountY;
        _Grid = new Vector3[elementCountX+1, elementCountY+1];

        for (int i = 0; i <= elementCountX; i++)
        {
            for (int j = 0; j <= elementCountY; j++)
            {
                _Grid[i, j] = new Vector3(i * elementSizeX, 0f, -j * elementSizeY);
            }
        }
    }

    public Faces ToFaces()
    {
        List<Face> faces = new List<Face>();

        for (int i = 0; i < ElementCountX; i++)
        {
            for (int j = 0; j < ElementCountY; j++)
            {
                var t1 = new Triangle(_Grid[i, j], _Grid[i + 1, j], _Grid[i, j + 1]);
                var t2 = new Triangle(_Grid[i, j + 1], _Grid[i + 1, j], _Grid[i + 1, j + 1]);
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

    public float size_x = 1;
    public float size_y = 1;

    void Start () {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = new Grid(count_x, count_y, size_x, size_y).ToMesh();
        meshFilter.mesh = mesh;
    }

}
