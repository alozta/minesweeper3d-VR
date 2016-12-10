using UnityEngine;
using System.Collections;

public class cubeController : MonoBehaviour {
    
    // Use this for initialization
    void Start()
    {
        generateSpaceField();
    }

    // Update is called once per frame
    void Update()
    {
        //android tuning, quit when pressing back button, auto quit when manual kill
        if (Input.GetKey(KeyCode.Escape))
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                Application.Quit();
            }
        }
    }

    //#####
    //class variables
    static int SPACE_SIZE = 5;
    int NUM_OF_MINES = 0;

    public class Cube
    {
        private GameObject cube;
        private Point3D point;
        private bool alive;
        private bool mine;

        //set x,y,z of a cube, make it alive and mine free
        public Cube(int x, int y, int z)
        {
            point = new Point3D(x, y, z);
            alive = true;
            mine = false;
            cube = Instantiate(Resources.Load("Prefabs/prefab_cube"), new Vector3(x, y, z), Quaternion.identity) as GameObject;
        }

        public bool getStatus() { return alive; }
        public Point3D getPoint() { return point; }
        public GameObject getGameObject() { return cube; }
        public bool getMine() { return mine; }
        public void setMine(bool status) { mine = status; }
    }

    public Cube[,,] space = new Cube[SPACE_SIZE, SPACE_SIZE, SPACE_SIZE];
    //class variables
    //#####

    //#####
    //functions

    //generates SPACE_SIZExSPACE_SIZExSPACE_SIZE 3D playground with their coordinates
    private void generateSpaceField()
    {
        for(int i=0; i<SPACE_SIZE; ++i)
        {
            for(int j=0; j<SPACE_SIZE; ++j)
            {
                for(int k=0; k<SPACE_SIZE; ++k)
                {
                    space[i, j, k] = new Cube(i, j, k);
                }
            }
        }
    }

    //places n mines in 3d space
    private void generateRandomMines(int n)
    {
        System.Random rnd = new System.Random();
        while (NUM_OF_MINES < n)
        {
            Point3D randomPoint = new Point3D(rnd.Next(SPACE_SIZE), rnd.Next(SPACE_SIZE), rnd.Next(SPACE_SIZE));
            if (!isAlive(randomPoint))
            {
                space[randomPoint.getX(), randomPoint.getY(), randomPoint.getZ()].setMine(true);        //set new Cube constructor to that x,y,z space???
                ++NUM_OF_MINES;
            }
        }
    }

    //returns space(point) cube is alive or not
    private bool isAlive(Point3D point)
    {
        return space[point.getX(), point.getY(), point.getY()].getStatus();
    }
    //functions
    //#####
}

public class Point3D
{
    private int x;
    private int y;
    private int z;

    public Point3D(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public int getX() { return x; }
    public int getY() { return y; }
    public int getZ() { return z; }
}
