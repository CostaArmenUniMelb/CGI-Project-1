using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A triangle is a ordered sequence of three points (clockwise)
 */
public class Triangle {
    public readonly List<Vector3> Points;

    public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Points = new List<Vector3> { p1, p2, p3 };
    }
}

/* A face is composed of two triangles where their hypotemuses
 * are coincident, forming a square shape. Both triangles have the 
 * same winding order (clockwise).
 */
public class Face
{
    public readonly List<Triangle> Triangles;

    public Face(Triangle t1, Triangle t2)
    {
        Triangles = new List<Triangle> { t1, t2 };
    }

    /* Creates a sequence of ordered points going from triangle 1
     * to triangle 2 in a clockwise winding order.  
     */
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

/* Wraps operations on a collection of faces
 */
public class Faces
{
    private List<Face> _Faces;

    public Faces(List<Face> faces)
    {
        _Faces = faces;
    }

    /* Flattens the set of points within each face
     * to a single collection of points
     */
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

/* A grid is a 2D square set (by index) of 3D points.
 * Alone each axis (X,Z) there are 2^n + 1 points
 * where n denotes the 'grid resolution' (number of halvings). 
 */
public class Grid
{
    private Vector3[,] _Grid;
    public readonly int GridResolution;
    public readonly int PointCount; 

    /* Creates a grid of size 'gridSize' in the X and Z directions
     * with 'gridResolution' number of halvings. The grid is centred so
     * the Y-axis cuts through the centre point. 
     */
    public Grid(int gridResolution, float gridSize)
    {
        GridResolution = gridResolution;
        PointCount = (int)(Mathf.Pow(2,gridResolution) + 1.0f);
        float gridSpacing = gridSize / PointCount;
        float translateX = gridSize * 0.5f;
        float translateZ = gridSize * 0.5f;
        _Grid = new Vector3[PointCount, PointCount];

        for (int i = 0; i < PointCount; i++)
        {
            for (int j = 0; j < PointCount; j++)
            {
                float x = (i * gridSpacing) - translateX;
                float z = (j * gridSpacing) - translateZ;
                _Grid[i, j] = new Vector3(x, 0f, z);
            }
        }
    }

    /* Traverses the grid and constructs a face (2 triangles) for each square
     */
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

    /* Creates a Unity Mesh by flattening the grid into 
     * a collection of points (vector3's) in triangle winding order
     */
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

    /* Helper method to set the height of a point at a particular
     * grid location
     */
    public void SetHeight(int indexX, int indexZ, float height)
    {
        var grid_point = _Grid[indexX, indexZ];
        _Grid[indexX, indexZ] = new Vector3(grid_point.x, height, grid_point.z);
    }

    /* Gets the height of a point at a particular grid lcoatrion.
     * This function returns zero for indexes out of bounds
     */
    public float GetHeight(int indexX, int indexZ)
    {
        try
        {
            return _Grid[indexX, indexZ].y;
        }
        catch
        {
            return 0;
        }
        
    }
}

/* A fractal grid is a grid with a fractal heightmap using the diamond square algorithm
 * The heights of the points at each diamond-square step will vary randomly within the
 * range of 'randomHeightSize'. At each diamond-square step the randomHeightSize will 
 * decay by the amount 'randomDecay' (exponential decay).  
 */
public class FractalGrid {

    public Grid Grid;
    private float RandomHeightSize = 1;
    private float RandomDecay;
    private float RandomSeed;
    private System.Random Random;

    /* Creates a flat square grid of size 'gridSize' and splits the grid using the 'gridResolution'.
     * The corder points are intialised to a height of 'randomHeightSize' and then the first diamond
     * step follows. The diamond step then invokes the square step and a mutual recursion takes place
     * between these two steps until the grid resolution is reached.
     */
    public FractalGrid(int gridResolution, float gridSize, float randomHeightSize, int randomSeed, float randomDecay)
    {
        Grid = new Grid(gridResolution, gridSize);
        Random =  new System.Random(randomSeed);
        RandomHeightSize = randomHeightSize;
        RandomDecay = randomDecay;
        RandomSeed = randomSeed;
        InitialiseCorners();
        Diamond(1, RandomHeightSize);
    }

    /* Sets the corner heights to randomHeightSize.
     */
    private void InitialiseCorners()
    {
        float cornerHeight = RandomHeightSize; 
        int lastPoint = Grid.PointCount-1;
        Grid.SetHeight(0, 0, cornerHeight);
        Grid.SetHeight(lastPoint, 0, cornerHeight);
        Grid.SetHeight(0, lastPoint, cornerHeight);
        Grid.SetHeight(lastPoint, lastPoint, cornerHeight);
    }

    /* Diamond step in diamond-square algorithm.
     * The approach taken here is firstly determining the location of all
     * the diamond points based on the resolution level of the grid. i.e. for
     * a resolution level of 2, the diamond points are at points, (1,1), (1,3),
     * (3,1) and (3,3). The diamond points are obtained by taking a constant offset
     * (of half the resolution spacing) for the first four points closest to the 
     * north west corner of the grid. Using the diamond points, the neighbouring 
     * points are queried for their heights, averaged and a random amount added.
     * The square step is then invoked.  
     * 'gridSplitDepth' is the grid resolution level in the current iteration
     */
    private void Diamond(int gridSplitDepth, float randomHeightSize)
    {
        if (gridSplitDepth > Grid.GridResolution)
        {
            return;
        }
        //spacing is the number of gridpoints that will be skipped whilst traversing
        //the grid for this level of resolution (gridSplitDepth).
        int spacing =  (Grid.PointCount - 1) / ((int)Mathf.Pow(2,gridSplitDepth-1));
        int half_spacing = spacing / 2;
       
        for(int i=0; i < Grid.PointCount-1; i += spacing)
        {
            for (int j=0; j < Grid.PointCount-1; j += spacing)
            {
                //the diamond points are a constant offset of half spacing away from the grid
                //points being traversed. 
                int i_diamond = i + half_spacing;
                int j_diamond = j + half_spacing;

                //neighbouring heights (north-west, north-east, south-east and south-west)
                float height_nw = Grid.GetHeight(i, j);
                float height_ne = Grid.GetHeight(i, j + spacing);
                float height_se = Grid.GetHeight(i + spacing, j + spacing);
                float height_sw = Grid.GetHeight(i + spacing, j);

                //average the neighbouring heights and add a random amount
                float height = ((height_nw + height_ne + height_se + height_sw) / 4) + NextRandom(randomHeightSize);
                Grid.SetHeight(i_diamond, j_diamond, height);
            }
        }
        Square(gridSplitDepth, randomHeightSize);
    }

    /* Square step in diamond-square algorithm
     * The approach taken here is determining the location of all
     * the square points based on the resolution level of the grid. i.e. for 
     * a resolution level of 2, the square points are at points, (0,1), (3,1),
     * (1,0), (1,2), (1,4),... A pattern appears where the points stagger whilst
     * travelling down the grid. Each square point in the horizontal direction is
     * a constant spacing away from the previous. Each square point in the vertical
     * direction is also a constant spacing away from the previous (row). After determing
     * the square points, the neighbours (north, east, south and west) heights are averaged
     * and a random amount is applied (same as the diamond step). After this, the square step
     * is invoked. 
     */
    private void Square(int gridSplitDepth, float randomHeightSize)
    {
        if (gridSplitDepth > Grid.GridResolution)
        {
            return;
        }
        int spacing = (Grid.PointCount - 1) / ((int)Mathf.Pow(2, gridSplitDepth - 1));
        int half_spacing = spacing / 2;

        int i_count = 0;

        for (int i = 0; i < Grid.PointCount; i += half_spacing)
        {
            //Staggering of the horizontal start position of the square point rows
            int initial_j = isEven(i_count) * half_spacing;

            for (int j = initial_j; j < Grid.PointCount; j += spacing)
            {
                //heights of the square points neighbours (north, east, south and west)
                float height_n = Grid.GetHeight(i - half_spacing, j);
                float height_e = Grid.GetHeight(i , j + half_spacing);
                float height_s = Grid.GetHeight(i + half_spacing, j);
                float height_w = Grid.GetHeight(i, j - half_spacing);

                //average the neighbouring heights and add a random amount
                float height = ((height_n + height_e + height_s + height_w) / 4) + NextRandom(randomHeightSize);
                Grid.SetHeight(i, j, height);
            }
            i_count++;
        }
        Diamond(gridSplitDepth + 1, randomHeightSize * RandomDecay);
    }

    private int isEven(int num)
    {
        if (num % 2 == 0)
        {
            return 1; 
        }
        else if (num % 2 == 1)
        {
            return 0;
        }
        return -1;
    }

    /* Creates a random number.
     * If the seed is zero, then a constant of 0 is returned.
     */
    private float NextRandom(float randomHeightSize)
    {
        if (RandomSeed == 0f)
        {
            return 0;
        }
        else
        {
            return (float)(Random.NextDouble() * randomHeightSize) - (randomHeightSize/2);
        }
        
    }

    public Mesh ToMesh()
    {
        return Grid.ToMesh();
    }
}

/* Interfaces with Unity to obtain parameters for
 * creating a fractal landscape and then generates 
 * a unity mesh that is to be rendered in unity.
 * 'gridResolution' is the number of times thegrid is refined by halving it
 * 'gridSize' is the total length of the grid in the X and Z directions
 * 'randomHeightSize' is the range to which the heightmap is varied in the 
 * diamond-square iterations
 * 'randomSeed' is used to ensure reproducible random results
 * 'randomdecay' is the degree to which the randomHeightSize is reduced in 
 * each iteration of the diamon-square algorithm
 */
public class LandscapeGenerator : MonoBehaviour {

    public int gridResolution = 5;
    public float gridSize = 50;
    public float randomHeightSize = 10;
    public int randomSeed = 1;
    public float randomDecay = 1f;

    void Start () {
        var meshFilter = GetComponent<MeshFilter>();
        var fractalGrid = new FractalGrid(gridResolution, gridSize, randomHeightSize, randomSeed, randomDecay);
        var mesh = fractalGrid.ToMesh();
        meshFilter.mesh = mesh;
    }

}
