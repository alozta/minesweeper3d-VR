using UnityEngine;
using System.Collections;

public class cubeController : MonoBehaviour {

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //#####
    //class variables
    static int SPACE_SIZE = 10;
    int NUM_OF_MINES = 0;

    private class Cube
    {
        private GameObject obj;
        private Point3D point;
        private bool alive;
        private bool mine;

        Cube(int x, int y, int z)
        {
            point = new Point3D(x, y, z);
        }

        public bool getStatus() { return alive; }
        public Point3D getPoint() { return point; }
        public GameObject getGameObject() { return obj; }
        public bool getMine() { return mine; }
        public void setMine(bool status) { mine = status; }
    }

    private Cube[,,] space = new Cube[SPACE_SIZE, SPACE_SIZE, SPACE_SIZE];
    //class variables
    //#####

    //#####
    //functions
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
