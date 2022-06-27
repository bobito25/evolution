using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalVars
{
    public static int backgroundWidth = 256;
    public static int backgroundHeight = 144;
}

public class main : MonoBehaviour
{
    List<Entity> entities;

    void Awake()
    {
        makeBackground();
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnEntities();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        moveEntities();
    }

    void spawnEntities() {
        entities = new List<Entity>();
        Creature1 c1 = new Creature1();
        entities.Add(c1);
    }

    void makeBackground() {
        Texture2D backgroundTex = new Texture2D(GlobalVars.backgroundWidth,GlobalVars.backgroundHeight);
        Color[] colorArray = new Color[backgroundTex.GetPixels().Length];
        for (int i = 0; i < colorArray.Length; i++) {
            colorArray[i] = Color.white;
        }
        backgroundTex.SetPixels(colorArray);
        backgroundTex.Apply();
        Sprite backgroundSprite = Sprite.Create(backgroundTex, new Rect(0,0,backgroundTex.width,backgroundTex.height),new Vector2(0.5f, 0.5f),10);
        GameObject background = new GameObject();
        background.name = "background";
        SpriteRenderer sr = background.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.sprite = backgroundSprite;
    }

    void moveEntities() {
        if (entities.Count == 0) {
            return;
        }
        foreach(Entity e in entities) {
            e.move();
        }
        foreach(Entity e in entities) {
            e.checkBoundaries();
        }
    }
}


public abstract class Entity
{
    static int nextId = 0;

    public static bool initialized = false;
    public static GameObject firstGameobject;

    public int id;
    public Net behaviour;
    public GameObject entityGameObject;
    public int speed = 10;
    
    public Entity() {
        id = nextId++;
        if (!initialized) {
            makeEntity();
            initialized = !initialized;
        }
        entityGameObject = Object.Instantiate(firstGameobject,new Vector3(0, 0, 0), Quaternion.identity);
    }

    public abstract void move();
    public abstract void checkBoundaries();
    public abstract void makeEntity();
}

public class Creature1 : Entity
{
    static Texture2D tex = new Texture2D(10,10);

    public override void makeEntity() {
        Color[] colorArray = new Color[tex.GetPixels().Length];
        for (int i = 0; i < colorArray.Length; i++) {
            colorArray[i] = Color.red;
        }
        tex.SetPixels(colorArray);
        tex.Apply();
        Sprite creature1Sprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),new Vector2(0.5f, 0.5f),20);
        firstGameobject = new GameObject();
        firstGameobject.name = "creature1";
        SpriteRenderer sr = firstGameobject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.sortingOrder = 1;
        sr.sprite = creature1Sprite;
    }

    public override void move() {
        entityGameObject.transform.Rotate(0,0,Random.value*2 *Random.Range(-1,2)); //eigentlich nicht y sondern z
        entityGameObject.transform.Translate(new Vector3(0,0.005f*speed,0));
    }

    public override void checkBoundaries() {
        Rect b = new Rect(-GlobalVars.backgroundWidth/20,-GlobalVars.backgroundHeight/20,GlobalVars.backgroundWidth/10,GlobalVars.backgroundHeight/10);
        Vector3 p = new Vector3(entityGameObject.transform.position.x,entityGameObject.transform.position.y,0);
        if (!b.Contains(p)) {
            if (entityGameObject.transform.position.x > GlobalVars.backgroundWidth/20) {
                entityGameObject.transform.position += new Vector3(-GlobalVars.backgroundWidth/10,0,0);
            } else if (entityGameObject.transform.position.x < -GlobalVars.backgroundWidth/20) {
                entityGameObject.transform.position += new Vector3(GlobalVars.backgroundWidth/10,0,0);
            } else if (entityGameObject.transform.position.y > GlobalVars.backgroundHeight/20) {
                entityGameObject.transform.position += new Vector3(0,-GlobalVars.backgroundHeight/10,0);
            } else if (entityGameObject.transform.position.y < -GlobalVars.backgroundHeight/20) { 
                entityGameObject.transform.position += new Vector3(0,GlobalVars.backgroundHeight/10,0);
            }
            //entityGameObject.transform.position = new Vector3(0,0,0);
        }
    }
}

public class Net
{
    int totalLinks;

    List<string> possibleInputs;
    List<string> possibleHiddens;
    List<string> possibleOutputs;

    List<Node> input;
    int inputNum;
    List<Node> hidden;
    int hiddenNum;
    List<Node> output; //needed?
    int outputNum;

    public Net(int tL, int nH) {
        possibleInputs = new List<string>{"constant","random","seeEntity"};
        possibleHiddens = new List<string>();
        for (int i = 0; i < nH; i++) {
            possibleHiddens.Add("hidden");
        }
        possibleOutputs = new List<string>{"moveForward","rotateLeft","rotateRight"};

        //choose links
        for (int i = 0; i < tL; i++) {
            
        }
    }

    public Net(int iN, int hN, int oN) {
        possibleInputs = new List<string>{"constant","random","seeEntity"};
        possibleOutputs = new List<string>{"moveForward","rotateLeft","rotateRight"};
        if (iN > possibleInputs.Count) iN = possibleInputs.Count;
        if (oN > possibleOutputs.Count) oN = possibleOutputs.Count;
        inputNum = iN;
        hiddenNum = hN;
        outputNum = oN;
        //make nodes
        for (int i = 0; i < iN; i++) {
            int r = Random.Range(0,possibleInputs.Count);
            string type = possibleInputs[r];
            possibleInputs.RemoveAt(r);
            input.Add(new Node(type));
        }
        for (int i = 0; i < hN; i++) {
            string type = "hidden";
            input.Add(new Node(type));
        }
        for (int i = 0; i < oN; i++) {
            int r = Random.Range(0,possibleOutputs.Count);
            string type = possibleOutputs[r];
            possibleOutputs.RemoveAt(r);
            input.Add(new Node(type));
        }
        //make links

    }
}

public class Node
{
    float value;
    string type;
    List<Node> links;

    public Node(string t, List<Node> l) {
        setRandomValue();
        type = t;
        links = l;
    }

    public Node(string t) {
        setRandomValue();
        type = t;
        links = new List<Node>();
    }

    public void setRandomValue() {
        if (Random.value > 0.5) {
            value = Random.value;
        } else {
            value = -Random.value;
        }
    }

    public void addLink(Node n) {
        links.Add(n);
    }
}