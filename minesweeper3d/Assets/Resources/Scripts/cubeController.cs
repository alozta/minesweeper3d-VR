using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class cubeController : MonoBehaviour {
    
    // Use this for initialization
    void Start()
    {
        initializeGame(0);
    }

    // Update is called once per frame
    void Update()
    {
        objectClickListener();                          //click listener attached to the camera
        
        if (Input.GetKey(KeyCode.Escape))               //android tuning, quit when pressing back button, auto quit on manual kill
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                Application.Quit();
            }
        }
    }

    //#############################################################################################################################
    //class variables
    static int SPACE_SIZE = 5;                                      //default mode is easy
    static int MINE_COUNT = 10;
    public Cube[,,] space;
    List<GameObject> textObjects = new List<GameObject>();          //cube replacement 3d text GameObjects, holding reference to rotate to face the camera on each update

    public class Cube
    {
        private GameObject cube;
        private Point3D point;
        private bool alive;
        private bool mine;
        private string textOnClick;                     //text to show when the cube is clicked

        //set x,y,z of a cube, make it alive and mine free
        public Cube(int x, int y, int z)
        {
            point = new Point3D(x, y, z);
            alive = true;
            mine = false;
            textOnClick = "";
            cube = Instantiate(Resources.Load("Prefabs/prefab_cube"), new Vector3(x, y, z), Quaternion.identity) as GameObject;
        }

        public bool getStatus() { return alive; }
        public Point3D getPoint() { return point; }
        public GameObject getGameObject() { return cube; }
        public bool getMine() { return mine; }
        public void setMine(bool status) { mine = status; }
        public void setText(int neighborMineCount) { textOnClick = neighborMineCount.ToString(); }
    }
    
    //class variables
    //#############################################################################################################################

    //#############################################################################################################################
    //functions

    //MUTATOR FUNCTIONS BELOW######################################################################################################

    //set initial requirements for the game
    public void initializeGame(int mode)
    {
        selectGameMode(mode);
        space = new Cube[SPACE_SIZE, SPACE_SIZE, SPACE_SIZE];
        generateSpace();
        generateRandomMines(MINE_COUNT);
    }
    
    //generates SPACE_SIZExSPACE_SIZExSPACE_SIZE 3D playground with their coordinates
    private void generateSpace()
    {
        for(int x=0; x<SPACE_SIZE; ++x)
        {
            for(int y=0; y<SPACE_SIZE; ++y)
            {
                for(int z=0; z<SPACE_SIZE; ++z)
                {
                    space[x, y, z] = new Cube(x, y, z);
                }
            }
        }
    }

    //places n mines in 3d space, expects n==MINE_COUNT but accepts n<MINE_COUNT situations too, TODO: this should be optimized to offer better gameplay
    private void generateRandomMines(int n)
    {
        System.Random rnd = new System.Random();
        while (n > 0 && n <= MINE_COUNT)
        {
            Point3D randomPoint = new Point3D(rnd.Next(SPACE_SIZE), rnd.Next(SPACE_SIZE), rnd.Next(SPACE_SIZE));
            if (!hasMine(randomPoint))
            {
                space[randomPoint.getX(), randomPoint.getY(), randomPoint.getZ()].setMine(true);
                --n;
            }
        }
    }

    //check neighbor mine status for each cube and store sum of it into that cube as text, 26 neighbor for each edge (cubes in the outer surfaces have less than 26)
    private void setAllCubeText()
    {
        for (int x = 0; x < SPACE_SIZE; ++x)
        {
            for (int y = 0; y < SPACE_SIZE; ++y)
            {
                for (int z = 0; z < SPACE_SIZE; ++z)            //for each cube, invalid points are eliminated automatically
                {
                    int mineCount = 0;
                    if (!(new Point3D(x + 1, y + 1, z + 1).isInvalidPoint()) && hasMine(new Point3D(x + 1, y + 1, z + 1))) ++mineCount;
                    if (!(new Point3D(x + 1, y + 1, z + 0).isInvalidPoint()) && hasMine(new Point3D(x + 1, y + 1, z + 0))) ++mineCount;
                    if (!(new Point3D(x + 1, y + 1, z - 1).isInvalidPoint()) && hasMine(new Point3D(x + 1, y + 1, z - 1))) ++mineCount;
                    if (!(new Point3D(x + 1, y + 0, z + 1).isInvalidPoint()) && hasMine(new Point3D(x + 1, y + 0, z + 1))) ++mineCount;
                    if (!(new Point3D(x + 1, y + 0, z + 0).isInvalidPoint()) && hasMine(new Point3D(x + 1, y + 0, z + 0))) ++mineCount;
                    if (!(new Point3D(x + 1, y + 0, z - 1).isInvalidPoint()) && hasMine(new Point3D(x + 1, y + 0, z - 1))) ++mineCount;
                    if (!(new Point3D(x + 1, y - 1, z + 1).isInvalidPoint()) && hasMine(new Point3D(x + 1, y - 1, z + 1))) ++mineCount;
                    if (!(new Point3D(x + 1, y - 1, z + 0).isInvalidPoint()) && hasMine(new Point3D(x + 1, y - 1, z + 0))) ++mineCount;
                    if (!(new Point3D(x + 1, y - 1, z - 1).isInvalidPoint()) && hasMine(new Point3D(x + 1, y - 1, z - 1))) ++mineCount;
                    if (!(new Point3D(x + 0, y + 1, z + 1).isInvalidPoint()) && hasMine(new Point3D(x + 0, y + 1, z + 1))) ++mineCount;
                    if (!(new Point3D(x + 0, y + 1, z + 0).isInvalidPoint()) && hasMine(new Point3D(x + 0, y + 1, z + 0))) ++mineCount;
                    if (!(new Point3D(x + 0, y + 1, z - 1).isInvalidPoint()) && hasMine(new Point3D(x + 0, y + 1, z - 1))) ++mineCount;
                    if (!(new Point3D(x + 0, y + 0, z + 1).isInvalidPoint()) && hasMine(new Point3D(x + 0, y + 0, z + 1))) ++mineCount;
                    if (!(new Point3D(x + 0, y + 0, z - 1).isInvalidPoint()) && hasMine(new Point3D(x + 0, y + 0, z - 1))) ++mineCount;
                    if (!(new Point3D(x + 0, y - 1, z + 1).isInvalidPoint()) && hasMine(new Point3D(x + 0, y - 1, z + 1))) ++mineCount;
                    if (!(new Point3D(x + 0, y - 1, z + 0).isInvalidPoint()) && hasMine(new Point3D(x + 0, y - 1, z + 0))) ++mineCount;
                    if (!(new Point3D(x + 0, y - 1, z - 1).isInvalidPoint()) && hasMine(new Point3D(x + 0, y - 1, z - 1))) ++mineCount;
                    if (!(new Point3D(x - 1, y + 1, z + 1).isInvalidPoint()) && hasMine(new Point3D(x - 1, y + 1, z + 1))) ++mineCount;
                    if (!(new Point3D(x - 1, y + 1, z + 0).isInvalidPoint()) && hasMine(new Point3D(x - 1, y + 1, z + 0))) ++mineCount;
                    if (!(new Point3D(x - 1, y + 1, z - 1).isInvalidPoint()) && hasMine(new Point3D(x - 1, y + 1, z - 1))) ++mineCount;
                    if (!(new Point3D(x - 1, y + 0, z + 1).isInvalidPoint()) && hasMine(new Point3D(x - 1, y + 0, z + 1))) ++mineCount;
                    if (!(new Point3D(x - 1, y + 0, z + 0).isInvalidPoint()) && hasMine(new Point3D(x - 1, y + 0, z + 0))) ++mineCount;
                    if (!(new Point3D(x - 1, y + 0, z - 1).isInvalidPoint()) && hasMine(new Point3D(x - 1, y + 0, z - 1))) ++mineCount;
                    if (!(new Point3D(x - 1, y - 1, z + 1).isInvalidPoint()) && hasMine(new Point3D(x - 1, y - 1, z + 1))) ++mineCount;
                    if (!(new Point3D(x - 1, y - 1, z + 0).isInvalidPoint()) && hasMine(new Point3D(x - 1, y - 1, z + 0))) ++mineCount;
                    if (!(new Point3D(x - 1, y - 1, z - 1).isInvalidPoint()) && hasMine(new Point3D(x - 1, y - 1, z - 1))) ++mineCount;         //could not handle the caos,  <+ 0> is cured my cancer

                    if (mineCount != 0) space[x, y, z].setText(mineCount);
                }
            }
        }
    }
    
    //sets the game diffuculty. 0 is easy, 1 is medium, 2 represents hard mode
    private static void selectGameMode(int mode)
    {
        if(mode == 0)
        {
            SPACE_SIZE = 5;
            MINE_COUNT = 10;
        }
        else if(mode == 1)
        {
            SPACE_SIZE = 7;
            MINE_COUNT = 33;
        }
        else if (mode == 2)
        {
            SPACE_SIZE = 10;
            MINE_COUNT = 99;
        }
    }

    //blast clicked cube & rotate 3d text GameObjects to the camera
    private void objectClickListener()
    {
        if (Input.GetMouseButtonDown(0))                    // if left button pressed
        {
            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // the object identified by hit.transform was clicked
                // do whatever you want
                Destroy(hit.transform.gameObject);                              //destroy the object located at x,y,z
                
                GameObject text = Instantiate(Resources.Load("Prefabs/prefab_text"), hit.transform.gameObject.transform.position, Quaternion.identity) as GameObject;       //place a 3d text object at x,y,z
                text.GetComponent<TextMesh>().text = "4";
                textObjects.Add(text);
            }
        }

        Transform t = GetComponent<Camera>().transform;                     //rotate all 3d text objects to this angle
        for (int i = 0; i < textObjects.Count; ++i)
        {
            textObjects[i].transform.LookAt(t.InverseTransformDirection(t.position.x,0,t.position.z));
        }
    }

    //ACCESSOR FUNCTIONS BELOW#####################################################################################################

    //returns space(point) cube has mine or not
    private bool hasMine(Point3D point)
    {
        return space[point.getX(), point.getY(), point.getZ()].getMine();
    }

    //returns space(point) cube is alive or not
    private bool isAlive(Point3D point)
    {
        return space[point.getX(), point.getY(), point.getZ()].getStatus();
    }

    //functions
    //#############################################################################################################################

    public class Point3D
    {
        private int x;
        private int y;
        private int z;

        public Point3D(int x, int y, int z)
        {
            if (x < 0 || x >= SPACE_SIZE || y < 0 || y >= SPACE_SIZE || z < 0 || z >= SPACE_SIZE)        //invalid points
            {
                this.x = -1; this.y = -1; this.z = -1;
            }
            else
            {
                this.x = x; this.y = y; this.z = z;
            }
        }

        public int getX() { return x; }
        public int getY() { return y; }
        public int getZ() { return z; }

        //returns true if at least one of the axis coordinate marked as invalid (-1)
        public bool isInvalidPoint()
        {
            if (x == -1 || y == -1 || z == -1)
                return true;
            return false;
        }
    }
}

