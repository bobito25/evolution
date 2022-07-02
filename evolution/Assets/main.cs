using static System.Array;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODOs:

//  change rotation to match euler angles

//  set min and max for behaviour net outputs

//  implement way to see bahaviour nets while running / clickable nodes

//  change evo pressure: add food, hunger, seeFood input

//  change weights to be less random values? -> example: only mutiples of 0.5 between -2 and 2

//  gamespeed slider

//  fix camera (dynammically)

/////////////////////////////////////////////////////////////////////////////////////////////////////////

public static class GlobalVars
{
    public static int backgroundWidth = 256;
    public static int backgroundHeight = 144;
}

public class main : MonoBehaviour
{
    List<Entity> entities;
    int numEntities;
    int maxEntities;

    public int time;
    public bool run;

    void Awake()
    {
        time = 1;
        run = false;
        maxEntities = 50;
        makeBackground();
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnEntities();
        run = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (run) {
            doBehaviour();
            if (time % 20 == 0) {
                mutate();
            }
            if (time % 1000 == 0) {
                nextGen();
            }
            checkAllDead();
            time++;
        }
    }

    void spawnEntities() {
        entities = new List<Entity>();
        numEntities = 0;
        for (int i = 0; i < maxEntities; i++) {
            Entity e = new Creature1();
            entities.Add(e);
            e.setRandPos();
            numEntities++;
        }
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

    void doBehaviour() {
        setInputs();
        calcBehaviours();
        moveEntities();
    }

    void setInputs() {
        foreach(Entity e in entities) {
            e.setInput();
        }
    }

    void calcBehaviours() {
        foreach(Entity e in entities) {
            e.calcBehaviour();
        }
    }

    void moveEntities() {
        foreach(Entity e in entities) {
            e.move();
            e.checkBoundaries();
        }
    }

    void mutate() {
        if (numEntities == 0) return;
        int r = Random.Range(0,numEntities);
        entities[r].mutate();
    }

    void nextGen() {
        deleteTouching();
        multiply();
    }

    void deleteTouching() {
        List<Entity> toDie = new List<Entity>();
        foreach(Entity e in entities) {
            if (e.isTouching()) toDie.Add(e);
        }
        foreach(Entity e in toDie) {
            //Debug.Log("killing " + e.id);
            kill(e);
        }
    }

    void checkAllDead() {
        if (numEntities == 0) {
            Debug.Log("all entities are dead, time: " + time);
            run = false;
        }
    }

    void multiply() {
        if (numEntities == 0) return;
        while (numEntities < maxEntities) {
            int r = Random.Range(0,numEntities);
            Entity e = entities[r];
            Entity newE = new Creature1();
            newE.behaviour = e.behaviour.clone();
            entities.Add(newE);
            newE.setRandPos();
            numEntities++;
        }
    }

    void kill(Entity e) {
        Destroy(e.entityGameObject);
        entities.Remove(e);
        numEntities--;
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

    public Actions nextAction;
    
    public Entity() {
        id = nextId++;
        behaviour = new Net(5,1);
        if (!initialized) {
            makeEntity();
            initialized = !initialized;
        }
        entityGameObject = Object.Instantiate(firstGameobject,new Vector3(0, 0, 0), Quaternion.identity);
        entityGameObject.SetActive(true);
    }

    public bool isTouching() {
        Collider2D collider = entityGameObject.GetComponent<Collider2D>();
        if (collider == null) Debug.Log("collider is null (funct isTouching)");
        Collider2D[] r = new Collider2D[1];
        ContactFilter2D cf = new ContactFilter2D();
        return !(collider.OverlapCollider(cf.NoFilter(), r) == 0);
    }

    public void setRandPos() {
        entityGameObject.transform.position = new Vector3(Random.Range(-GlobalVars.backgroundWidth/2,GlobalVars.backgroundWidth/2),Random.Range(-GlobalVars.backgroundHeight/2,GlobalVars.backgroundHeight/2),0);
    }

    public abstract void setInput();
    public abstract void calcBehaviour();
    public abstract void move();
    public abstract void checkBoundaries();
    public abstract void makeEntity();
    public abstract void mutate();
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
        firstGameobject.SetActive(false);
        firstGameobject.name = "creature1";
        SpriteRenderer sr = firstGameobject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.sortingOrder = 1;
        sr.sprite = creature1Sprite;
        BoxCollider2D collider = firstGameobject.AddComponent<BoxCollider2D>() as BoxCollider2D;
    }

    public override void setInput() {
        behaviour.setInput(checkSeeEntity());
    }

    public bool checkSeeEntity() {
        Vector3 fwd = entityGameObject.transform.up;
        Vector3 origin = entityGameObject.transform.position+(entityGameObject.transform.up/2);
        //Debug.DrawRay(origin, fwd*3, Color.yellow);
        RaycastHit2D hit = Physics2D.Raycast(origin, fwd, 3);
        //if (hit.collider != null) Debug.Log(hit.distance);
        return hit.collider != null;
    }

    public override void calcBehaviour() {
        nextAction = behaviour.calcOutput();
    }

    public override void move() {
        entityGameObject.transform.Rotate(nextAction.rotation);
        entityGameObject.transform.Translate(new Vector3(0,0.01f*nextAction.forwardM,0));
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
        }
    }

    public override void mutate() {
        behaviour.mutate();
    }
}

public struct Actions {
    public float forwardM;
    public Vector3 rotation;

    public Actions(float m, Vector3 r) {
        forwardM = m;
        rotation = r;
    }
}

public class Net
{
    int linkNum;

    static int possibleLinks;
    static int linksAtStart;

    static string[] possibleInputs;
    static int inputNum;
    static string[] possibleHiddens;
    static int hiddenNum;
    static string[] possibleOutputs;
    static int outputNum;

    bool[,] adj;

    List<Link> links;

    Node[] input;
    Node[] hidden;
    Node[] output;

    public Net(int tL, int nH) { //max links and num hidden
        linkNum = 0;
        linksAtStart = tL;
        links = new List<Link>();
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

    public Net clone() {
        Net r = new Net(linksAtStart,hiddenNum);
        foreach (Link l in links) {
            r.addLink(l.index);
        }
        return r;
    }

    public void setInput(bool seeEntity) {
        if (input[1] != null) {
            input[1].setRandomValue();
        }
        if (input[2] != null) {
            if (seeEntity) {
                input[2].value = 1;
            } else {
                input[2].value = 0;
            }
        }
    }

    public Actions calcOutput() {
        initNet();
        for (int i = 0; i < inputNum; i++) {
            if (input[i] != null) input[i].calc();
        }
        for (int i = 0; i < hiddenNum; i++) {
            if (hidden[i] != null) hidden[i].calc();
        }
        float[] o = new float[outputNum];
        for (int i = 0; i < outputNum; i++) {
            if (output[i] != null) {
                o[i] = output[i].value;
            } else {
                o[i] = 0;
            }
        }
        float m = o[0];
        Vector3 rL = new Vector3(0,0,o[1]);
        Vector3 rR = new Vector3(0,0,o[2]);
        return new Actions(m,rL-rR);
    }

    public void initNet() {
        for (int i = 0; i < hiddenNum; i++) {
            if (hidden[i] != null) hidden[i].value = 0;
        }
        for (int i = 0; i < outputNum; i++) {
            if (output[i] != null) output[i].value = 0;
        }
    }

    public void addRandomLink() {
        addLink(Random.Range(0,possibleLinks));
    }

    public Link addLink(int r) { //returns added link
        Link l;
        int index = r;
        if (r < inputNum*outputNum) {
            int r2 = r / inputNum;
            int r3 = r % inputNum;
            if (adj[r3,r2]) return null;
            if (input[r3] == null) {
                Node n1 = new Node(possibleInputs[r3],r3,r3,"input");
                input[r3] = n1;
            }
            if (output[r2] == null) {
                Node n2 = new Node(possibleOutputs[r2],r2,r2,"output");
                output[r2] = n2;
            }
            l = input[r3].addLink(output[r2]);
            l.index = index;
            links.Add(l);
            adj[r3,r2] = true;
            linkNum++;
        } else if (r < inputNum*(outputNum+hiddenNum)) {
            r = r-(inputNum*outputNum);
            int r2 = r / inputNum;
            int r3 = r % inputNum;
            int i2 = outputNum + r2;
            int i3 = r3;
            if (adj[i3,i2]) return null;
            if (input[r3] == null) {
                Node n1 = new Node(possibleInputs[r3],i3,r3,"input");
                input[r3] = n1;
            }
            if (hidden[r2] == null) {
                Node n2 = new Node(possibleHiddens[r2],i2,r2,"hidden");
                hidden[r2] = n2;
            }
            l = input[r3].addLink(hidden[r2]);
            l.index = index;
            links.Add(l);
            adj[i3,i2] = true;
            linkNum++;
        } else {
            r = r-(inputNum*(outputNum+hiddenNum));
            int r2 = r / hiddenNum;
            int r3 = r % hiddenNum;
            int i2 = r2;
            int i3 = inputNum + r3;
            if (adj[i3,i2]) return null;
            if (hidden[r3] == null) {
                Node n1 = new Node(possibleHiddens[r3],i3,r3,"hidden");
                hidden[r3] = n1;
            }
            if (output[r2] == null) {
                Node n2 = new Node(possibleOutputs[r2],i2,r2,"output");
                output[r2] = n2;
            }
            l = hidden[r3].addLink(output[r2]);
            l.index = index;
            links.Add(l);
            adj[i3,i2] = true;
            linkNum++;
        }
        return l;
    }

    public void mutate() {
        if (Random.value > 0.2 && linkNum != 0) {
            mutateWeight();
        } else {
            mutateLink();
        }
    }

    public void mutateWeight() {
        int r = Random.Range(0,linkNum);
        Link w = links[r];
        w.mutateWeight();
    }

    public void mutateLink() {
        if (Random.value > 0.7 && linkNum != 0) {
            removeRandomLink();
        } else {
            addRandomLink();
        }
    }

    public void removeRandomLink() {
        int r = Random.Range(0,linkNum);
        Link w = links[r];
        w.source.links.Remove(w);
        adj[w.source.index,w.dest.index] = false;
        cleanS(w.source);
        cleanD(w.dest);
        links.Remove(w);
        linkNum--;
    }

    public void cleanS(Node n) {
        if (n.index < inputNum) {
            if (!(n.links.Count == 0)) {
                input[n.index2] = null;
            }
        } else {
            if (!checkHiddenNodeLinked(n.index)) {
                hidden[n.index2] = null;
            }
        }
    }

    public void cleanD(Node n) {
        if (n.index < outputNum) {
            if (!checkOutputNodeLinked(n.index)) {
                output[n.index2] = null;
            }
        } else {
            if (!checkHiddenNodeLinked(n.index)) {
                hidden[n.index2] = null;
            }
        }
    }

    public bool checkHiddenNodeLinked(int index) {
        bool r = false;
        for (int i = 0; i < inputNum; i++) {
            if (adj[i,index]) r = true;
        }
        for (int i = 0; i < outputNum; i++) {
            if (adj[index,i]) r = true;
        }
        return r;
    }

    public bool checkOutputNodeLinked(int index) {
        bool r = false;
        for (int i = 0; i < outputNum+hiddenNum; i++) {
            if (adj[index,i]) r = true;
        }
        return r;
    }
}

public class Node
{
    public float value;
    public string name;
    public string type;
    public List<Link> links;
    public int index; //adj
    public int index2; //array

    public Node(string n, List<Link> l, string t) {
        if (n == "constant") {
            value = 1;
        } else {
            value = 0;
        }
        name = n;
        type = t;
        links = l;
    }

    public Node(string n, string t) {
        if (n == "constant") {
            value = 1;
        } else {
            value = 0;
        }
        name = n;
        type = t;
        links = new List<Link>();
    }

    public Node(string n, int i1, int i2, string t) {
        if (n == "constant") {
            value = 1;
        } else {
            value = 0;
        }
        name = n;
        type = t;
        index = i1;
        index2 = i2;
        links = new List<Link>();
    }

    public void calc() {
        foreach (Link l in links) {
            l.dest.value += value * l.weight;
        }
    }

    public void setRandomValue() {
        if (Random.value > 0.5) {
            value = Random.value;
        } else {
            value = -Random.value;
        }
    }

    public Link addLink(Node n) {
        Link r = new Link(this,n);
        links.Add(r);
        return r;
    }
}

public class Link
{
    public float weight;
    public Node source;
    public Node dest;
    public int index; // number used to generate link in addLink method

    public Link(Node s, Node d) {
        weight = Random.Range(-2,2);
        source = s;
        dest = d;
    }

    public void mutateWeight() {
        float m = Random.Range(-1,1);
        weight += m;
    }
}