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
        behaviour = new Net(5,1);
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
        entityGameObject.transform.Rotate(0,0,Random.value*2 *Random.Range(-1,2));
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
    int linkNum;
    int possibleLinks;

    string[] possibleInputs;
    string[] possibleHiddens;
    string[] possibleOutputs;

    bool[,] adj;

    Node[] input;
    int inputNum;
    Node[] hidden;
    int hiddenNum;
    Node[] output;
    int outputNum;

    bool first;

    public Net(int tL, int nH) {
        linkNum = 0;
        possibleInputs = new string[] {"constant","random","seeEntity"};
        possibleHiddens = new string[nH];
        for (int i = 0; i < nH; i++) {
            possibleHiddens[i] = "hidden";
        }
        possibleOutputs = new string[] {"moveForward","rotateLeft","rotateRight"};
        
        inputNum = possibleInputs.Length;
        hiddenNum = possibleHiddens.Length;
        outputNum = possibleOutputs.Length;

        possibleLinks = inputNum*(outputNum+hiddenNum)+(hiddenNum*outputNum);
        if (tL > possibleLinks) {
            Debug.Log("error: links per net higher than possible link amount (funct Net)");
        } 

        adj = new bool[possibleInputs.Length+possibleHiddens.Length,possibleOutputs.Length+possibleHiddens.Length];

        input = new Node[possibleInputs.Length];
        hidden = new Node[possibleHiddens.Length];
        output = new Node[possibleOutputs.Length];

        for (int i = 0; i < tL; i++) {
            addRandomLink();
        }
    }

    public void addRandomLink() {
        int r = Random.Range(0,possibleLinks);
        if (r < inputNum*outputNum) {
            int r2 = r / inputNum;
            int r3 = r % inputNum;
            if (adj[r3,r2]) return;
            if (input[r3] == null) {
                Node n1 = new Node(possibleInputs[r3]);
                input[r3] = n1;
                inputNodeBools[r3] = true;
            }
            if (output[r2] == null) {
                Node n2 = new Node(possibleOutputs[r2]);
                output[r2] = n2;
                outputNodeBools[r2] = true;
            }
            input[r3].addLink(hidden[r2]);
            adj[r3,r2] = true;
            linkNum++;
        } else if (r < inputNum*(outputNum+hiddenNum)) {
            r = r-(inputNum*outputNum);
            int r2 = r / inputNum;
            int r3 = r % inputNum;
            if (adj[r3,r2]) return;
            if (input[r3] == null) {
                Node n1 = new Node(possibleInputs[r3]);
                input[r3] = n1;
            }
            if (hidden[r2] == null) {
                Node n2 = new Node(possibleHiddens[r2]);
                hidden[r2] = n1;
            }
            input[r3].addLink(hidden[r2]);
            adj[r3,r2] = true;
            linkNum++;
        } else {
            r = r-(inputNum*(outputNum+hiddenNum));
            int r2 = r / hiddenNum;
            int r3 = r % hiddenNum;
            if (adj[r3,r2]) return;
            if (hidden[r3] == null) {
                Node n1 = new Node(possibleHiddens[r3]);
                hidden[r3] = n1;
            }
            if (output[r2] == null) {
                Node n2 = new Node(possibleOutputs[r2]);
                output[r2] = n2;
            }
            hidden[r3].addLink(output[r2]);
            adj[r3,r2] = true;
            linkNum++;
        }
    }
}

public class Node
{
    float value;
    string type;
    List<Link> links;

    public Node(string t, List<Link> l) {
        setRandomValue();
        type = t;
        links = l;
    }

    public Node(string t) {
        setRandomValue();
        type = t;
        links = new List<Link>();
    }

    public void setRandomValue() {
        if (Random.value > 0.5) {
            value = Random.value;
        } else {
            value = -Random.value;
        }
    }

    public void addLink(Node n) {
        links.Add(new Link(this,n));
    }
}

public class Link
{
    public float weight;
    public Node source;
    public Node dest;

    public Link(Node s, Node d) {
        if (Random.value > 0.5) {
            weight = Random.value*2;
        } else {
            weight = -Random.value*2;
        }
        source = s;
        dest = d;
    }
}