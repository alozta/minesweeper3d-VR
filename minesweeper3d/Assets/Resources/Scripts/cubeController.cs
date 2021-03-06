﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class cubeController : MonoBehaviour {
    
    // Use this for initialization
    void Start()
    {
        textObjects = new List<GameObject>();          //cube replacement 3d text GameObjects, holding reference to rotate to face the camera on each update
        COLOR_INITIAL = new Color(0.1f, 0.1f, 0.1f, COLOR_VISIBILITY);
        COLOR_MARKED = new Color(0.7f, 0.2f, 0.7f, COLOR_VISIBILITY);
        COLOR_MINE = new Color(0.8f, 0.0f, 0.2f, COLOR_VISIBILITY);
        COLOR_MINE_BLOWN = new Color(0.0f, 0.0f, 0.0f, COLOR_VISIBILITY);
        FIXED_TIME = Time.fixedDeltaTime;
        MOUSE_DOWN_TO_MARK_TIME = FIXED_TIME * 10;
        initializeGame(0);
        MODIFY_SPACE = true;
    }

    IEnumerator showTextFuntion()
    {
        if (informativeText == null)
        {
            string s;
            if (WIN) s = ":)";
            else s = "***";
            informativeText = Instantiate(Resources.Load("Prefabs/prefab_text"), new Vector3(-10, -5, -10), Quaternion.identity) as GameObject;
            informativeText.transform.Rotate(0.0f, -Input.GetAxis("Horizontal"), 0.0f);
            informativeText.GetComponent<TextMesh>().text = s;
            informativeText.transform.localScale = new Vector3(informativeText.transform.localScale.x*10, informativeText.transform.localScale.y * 10, informativeText.transform.localScale.z*10);
        }
        yield return new WaitForSeconds(10f);
        //Destroy(informativeText);
        //SceneManager.LoadScene("initial_scene");
    }

    // Update is called once per frame
    void Update()
    {
        clickListener();
        rotateTextToCamera();
        if (!MODIFY_SPACE)                                              //the game has ended
        {
            StartCoroutine(showTextFuntion());
            //SceneManager.LoadScene("initial_scene");
        }
        else
        {
            updateScoreBoard();
        }

        if (Input.GetKey(KeyCode.Escape))                           //android tuning, quit when pressing back button, auto quit on manual kill
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
    static bool MODIFY_SPACE = true;                                //becomes false if user hits the mine, chance to observe the space without further actions
    static float COLOR_VISIBILITY = 0.01f;
    static Color COLOR_INITIAL;
    static Color COLOR_MARKED;
    static Color COLOR_MINE;
    static Color COLOR_MINE_BLOWN;
    static float FIXED_TIME;
    static float MOUSE_DOWN_TO_MARK_TIME;
    static int SCORE = 0;
    static int REMAINING_CUBES = 0;
    static bool WIN = false;
    static GameObject scoreObject;
    static GameObject informativeText = null;
    static Point3D interactedPoint;                                 //clicked cube point
    static float timeMouseDown = 0;
    public Cube[,,] space;
    List<GameObject> textObjects;

    public class Cube
    {
        private GameObject cube;
        private Point3D point;
        private bool blasted;
        private bool mine;
        private string neigborMineCount;                     //text to show when the cube is clicked

        //set x,y,z of a cube
        public Cube(int x, int y, int z)
        {
            point = new Point3D(x, y, z);
            blasted = false;
            mine = false;
            neigborMineCount = "";
            cube = Instantiate(Resources.Load("Prefabs/prefab_cube"), new Vector3(x, y, z), Quaternion.identity) as GameObject;
            cube.GetComponent<Renderer>().material.color = COLOR_INITIAL;
        }

        public bool getStatus() { return blasted; }
        public Point3D getPoint() { return point; }
        public GameObject getCube() { return cube; }
        public bool hasMine() { return mine; }
        public int getNeigborMineCountI() { if (neigborMineCount.Equals("")) return 0; else return int.Parse(neigborMineCount); }
        public string getNeigborMineCountS() { return neigborMineCount; }
        public void setMine(bool status) { mine = status; }
        public void setNeigborMineCount(int neighborMineCount) { this.neigborMineCount = neighborMineCount.ToString(); }
        public void setCube(GameObject o) { cube = o; }
        public void setStatus(bool blasted) { this.blasted = blasted; }
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
        enableGodMode();
    }
    
    //generates SPACE_SIZExSPACE_SIZExSPACE_SIZE 3D playground with their coordinates
    private void generateSpace()
    {
        //generate space
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

        //generate scoreboard
        scoreObject = Instantiate(Resources.Load("Prefabs/prefab_text"), new Vector3(-5,-5,-5), Quaternion.identity) as GameObject;
        scoreObject.transform.Rotate(0.0f, -Input.GetAxis("Horizontal"), 0.0f);
        updateScoreBoard();
        textObjects.Add(scoreObject);
    }

    private void updateScoreBoard()
    {
        if(scoreObject != null)
            scoreObject.GetComponent<TextMesh>().text = "REMAINS: " + SCORE.ToString();
    }

    //places n mines in 3d space, expects n==MINE_COUNT but also accepts n<MINE_COUNT situations, TODO: this should be optimized to offer better gameplay
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
        setAllCubeText();
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

                    if (mineCount != 0) space[x, y, z].setNeigborMineCount(mineCount);
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
        SCORE = MINE_COUNT;
        REMAINING_CUBES = SPACE_SIZE * SPACE_SIZE * SPACE_SIZE;
    }

    //returns clicked object location in Point3D
    private Point3D objectClickListener()
    {
        timeMouseDown += FIXED_TIME;
        Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            return convertToPoint3D(hit.transform.gameObject.transform.position);
        }
        return null;
    }

    //click listener
    private void clickListener()
    {
        if (Input.GetMouseButton(0))                        //if left button is held down
        {
            if (MODIFY_SPACE)                               //if the game continues
            {
                interactedPoint = objectClickListener();    //click listener attached to the camera
            }
        }

        if (Input.GetMouseButtonUp(0))                      // if left button released
        {
            if (MODIFY_SPACE == true && interactedPoint != null && timeMouseDown <= MOUSE_DOWN_TO_MARK_TIME && SCORE > 0) blastCube(interactedPoint);       //click to blast
            else if (MODIFY_SPACE == true && interactedPoint != null && SCORE > 0) markCube(interactedPoint);                                               //hold to mark

            timeMouseDown = 0;
            interactedPoint = null;
        }
    }

    //does the expected action when clicking a cube
    private void blastCube(Point3D cube)
    {
        if (cube.isInvalidPoint()) return;                                  //dont go out of the territory

        if(space[cube.getX(), cube.getY(), cube.getZ()].hasMine())          //user clicked on the mine, game over
        {
            space[cube.getX(), cube.getY(), cube.getZ()].getCube().GetComponent<Renderer>().material.color = COLOR_MINE_BLOWN;
            space[cube.getX(), cube.getY(), cube.getZ()].setStatus(true);                               //mark as blasted
            WIN = false;
            MODIFY_SPACE = false;
        }
        else                                                                //no mine
        {
            space[cube.getX(), cube.getY(), cube.getZ()].setStatus(true);                           //mark as blasted
            --REMAINING_CUBES;
            if (REMAINING_CUBES <= SCORE)
            {
                WIN = true;
                MODIFY_SPACE = false;
            }

            if (space[cube.getX(), cube.getY(), cube.getZ()].getCube() != null)
            {
                Destroy(space[cube.getX(), cube.getY(), cube.getZ()].getCube());                        //pop out current cube
                space[cube.getX(), cube.getY(), cube.getZ()].setCube(null);
            }

            if (space[cube.getX(), cube.getY(), cube.getZ()].getNeigborMineCountI() == 0)               //spread the blast to the neighbor cubes
            {
                //spread to the neighbors
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() + 1, cube.getZ() + 1))) blastCube(new Point3D(cube.getX() + 0, cube.getY() + 1, cube.getZ() + 1));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() + 1, cube.getZ() + 0))) blastCube(new Point3D(cube.getX() + 0, cube.getY() + 1, cube.getZ() + 0));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() + 1, cube.getZ() - 1))) blastCube(new Point3D(cube.getX() + 0, cube.getY() + 1, cube.getZ() - 1));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() + 0, cube.getZ() + 1))) blastCube(new Point3D(cube.getX() + 0, cube.getY() + 0, cube.getZ() + 1));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() + 0, cube.getZ() - 1))) blastCube(new Point3D(cube.getX() + 0, cube.getY() + 0, cube.getZ() - 1));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() - 1, cube.getZ() + 1))) blastCube(new Point3D(cube.getX() + 0, cube.getY() - 1, cube.getZ() + 1));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() - 1, cube.getZ() + 0))) blastCube(new Point3D(cube.getX() + 0, cube.getY() - 1, cube.getZ() + 0));
                if (!isBlasted(new Point3D(cube.getX() + 0, cube.getY() - 1, cube.getZ() - 1))) blastCube(new Point3D(cube.getX() + 0, cube.getY() - 1, cube.getZ() - 1));
            }
            else                                            //cube has no mine but has neighbor to at least one mined one
            {
                GameObject text = Instantiate(Resources.Load("Prefabs/prefab_text"), new Vector3(cube.getX(), cube.getY(), cube.getZ()), Quaternion.identity) as GameObject;    //create the text object
                if(space[cube.getX(), cube.getY(), cube.getZ()].getNeigborMineCountI() == 1) text.GetComponent<Renderer>().material.color = Color.grey;                         //select color scheme
                else if (space[cube.getX(), cube.getY(), cube.getZ()].getNeigborMineCountI() == 2) text.GetComponent<Renderer>().material.color = Color.cyan;
                else if (space[cube.getX(), cube.getY(), cube.getZ()].getNeigborMineCountI() == 3) text.GetComponent<Renderer>().material.color = Color.blue;
                else text.GetComponent<Renderer>().material.color = Color.white;
                text.GetComponent<TextMesh>().text = space[cube.getX(), cube.getY(), cube.getZ()].getNeigborMineCountS();                                                       //display the text
                textObjects.Add(text);                      //add text object to the list to rotate to the camera on each update
            }
        }
    }

    //marks/unmarks selected cube
    private void markCube(Point3D cube)
    {
        if(space[cube.getX(), cube.getY(), cube.getZ()].getCube().GetComponent<Renderer>().material.color.Equals(COLOR_INITIAL))
        {
            space[cube.getX(), cube.getY(), cube.getZ()].getCube().GetComponent<Renderer>().material.color = COLOR_MARKED;
            if(SCORE > 0) --SCORE;
        }
        else                            //COLOR_MARKED
        {
            space[cube.getX(), cube.getY(), cube.getZ()].getCube().GetComponent<Renderer>().material.color = COLOR_INITIAL;
            ++SCORE;
        }
    }

    //mark mined cubes as red, debug purposes
    private void enableGodMode()
    {
        for (int x = 0; x < SPACE_SIZE; ++x)
        {
            for (int y = 0; y < SPACE_SIZE; ++y)
            {
                for (int z = 0; z < SPACE_SIZE; ++z)
                {
                    if(space[x, y, z].hasMine())
                    {
                        space[x, y, z].getCube().GetComponent<Renderer>().material.color = COLOR_MINE;
                    }
                }
            }
        }
    }

    //rotates all 3d text objects to the camera
    private void rotateTextToCamera()
    {
        Vector3 fwd = Camera.main.transform.forward;
        fwd.y = (float)-0.8;
        fwd.z = (float)-0.1;
        //Transform t = GetComponent<Camera>().transform;
        for (int i = 0; i < textObjects.Count; ++i)
        {
            //textObjects[i].transform.LookAt(t.InverseTransformDirection(t.position.x, 0, t.position.z));
            if(textObjects[i] != null)
                textObjects[i].transform.rotation = Quaternion.LookRotation(fwd);
        }
    }

    //ACCESSOR FUNCTIONS BELOW#####################################################################################################

    //returns space(point) cube has mine or not
    private bool hasMine(Point3D point)
    {
        return space[point.getX(), point.getY(), point.getZ()].hasMine();
    }

    //returns space(point) cube is blasted or not
    private bool isBlasted(Point3D point)
    {
        if (!point.isInvalidPoint())
            return space[point.getX(), point.getY(), point.getZ()].getStatus();
        else
            return true;
    }

    //converts Vector3 to Point3D class
    private Point3D convertToPoint3D(Vector3 v)
    {
        return new Point3D((int)v.x, (int)v.y, (int)v.z);
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

