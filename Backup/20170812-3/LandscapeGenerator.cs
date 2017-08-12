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
    public readonly int GridSplitCount;
    public readonly int PointCount; 

    public Grid(int gridSplitCount, float size)
    {
        GridSplitCount = gridSplitCount;
        PointCount = (int)(Mathf.Pow(2,gridSplitCount) + 1.0f);
        float translateX = PointCount * size * 0.5f;
        float translateZ = PointCount * size * 0.5f;
        _Grid = new Vector3[PointCount, PointCount];

        for (int i = 0; i < PointCount; i++)
        {
            for (int j = 0; j < PointCount; j++)
            {
                float x = (i * size) - translateX;
                float z = (j * size) - translateZ;
                _Grid[i, j] = new Vector3(x, 0f, z);
            }
        }
    }

    public Faces ToFaces()
    {
        List<Face> faces = new List<Face>();

        for (int i = 0; i < PointCount-1; i++)
        {
            for (int j = 0; j < PointCount-1; j++)
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

    public void SetHeight(int indexX, int indexZ, float height)
    {
        var grid_point = _Grid[indexX, indexZ];
        _Grid[indexX, indexZ] = new Vector3(grid_point.x, height, grid_point.z);
    }

    public float GetHeight(int indexX, int indexZ)
    {
        return _Grid[indexX, indexZ].y;
    }
}

public class FractalGrid {

    public Grid Grid;
    private float RandomSize = 1;
    private System.Random Random = new System.Random();

    public FractalGrid(int gridSplitCount, float size, float randomSize, float seed)
    {
        Grid = new Grid(gridSplitCount, size);
        RandomSize = randomSize;
        Diamond(2);
    }

    private void Diamond(int gridSplitDepth)
    {
        int spacing =  Grid.PointCount / gridSplitDepth;
        int half_spacing = spacing / 2;
       
        for(int i=0; i < Grid.PointCount-1; i += spacing)
        {
            for (int j=0; j < Grid.PointCount-1; j += spacing)
            {
                int i_diamond = i + half_spacing;
                int j_diamond = j + half_spacing;
                float height_nw = Grid.GetHeight(i, j);
                float height_ne = Grid.GetHeight(i, j + spacing);
                float height_se = Grid.GetHeight(i + spacing, j + spacing);
                float height_sw = Grid.GetHeight(i + spacing, j);
                float height = ((height_nw + height_ne + height_se + height_sw) / 4) + (float)(Random.NextDouble() * RandomSize);
                Grid.SetHeight(i_diamond, j_diamond, height);
            }
        }
    }

    public Mesh ToMesh()
    {
        return Grid.ToMesh();
    }
}

public class LandscapeGenerator : MonoBehaviour {

    public int gridSplitCount = 2;
    public float gridSize = 10;
    public float randomSize = 10;
    public float randomSeed = 1;

    void Start () {
        var meshFilter = GetComponent<MeshFilter>();
        var fractalGrid = new FractalGrid(gridSplitCount, gridSize, randomSize, randomSeed);
        var mesh = fractalGrid.ToMesh();
        meshFilter.mesh = mesh;
    }

}
