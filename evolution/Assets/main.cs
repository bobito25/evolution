using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODOs:

//  implement way to see bahaviour nets while running / clickable nodes

// make game states saveable

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

    List<Plant> plants;
    int numPlants;
    int maxPlants;

    Entity highlighted;

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
        spawnPlants();
        run = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            checkHighlight();
        }
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

    void spawnPlants() {
        plants = new List<Entity>();
        numPlants = 0;
        for (int i = 0; i < maxPlant; i++) {
            Plant p = new Plant();
            plants.Add(p);
            p.setRandPos();
            numPlants++;
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

    void checkHighlight() {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0,0,-1), 2);
        if (hit.collider != null)
        {
            Entity e = entities.Find(x => x.entityGameObject == hit.collider.gameObject);
            if (e == highlighted) {
                e.unhighlight();
                return;
            }
            e.showBehaviourNet();
            if (highlighted != null) highlighted.unhighlight();
            highlighted = e;
            e.highlight();
        }
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
        if (highlighted == e) highlighted = null;
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

    public abstract void highlight();
    public abstract void unhighlight();
    public abstract void showBehaviourNet();
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
        entityGameObject.transform.Rotate(new Vector3(0,0,nextAction.rotation));
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

    public override void showBehaviourNet() {
        behaviour.show();
    }

    public override void highlight() {
        entityGameObject.GetComponent<SpriteRenderer>().color = Color.blue;
    }

    public override void unhighlight() {
        entityGameObject.GetComponent<SpriteRenderer>().color = Color.red;
    }
}

public struct Actions {
    public float forwardM;
    public float rotation;

    public Actions(float m, float r) {
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

    static float maxSpeed = 10;
    static float maxRotation = 11f/7f;

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
        possibleOutputs = new string[] {"moveForward","rotateLeft"};
        
        inputNum = possibleInputs.Length;
        hiddenNum = possibleHiddens.Length;
        outputNum = possibleOutputs.Length;

        possibleLinks = inputNum*(outputNum+hiddenNum)+(hiddenNum*outputNum);
        if (tL > possibleLinks) {
            Debug.Log("error: links per net higher than possible link amount (funct Net)");
        } 

        adj = new bool[inputNum+hiddenNum,outputNum+hiddenNum];

        input = new Node[inputNum];
        hidden = new Node[hiddenNum];
        output = new Node[outputNum];

        for (int i = 0; i < tL; i++) {
            addRandomLink();
        }
    }

    public void show() {
        Debug.Log("Links:");
        foreach (Link l in links) {
            l.print();
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
        float m = Max(Min(o[0],maxSpeed),-maxSpeed);
        float rot = Max(Min(o[1],maxRotation),-maxRotation);
        return new Actions(m,rot);
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
            int rx = r % inputNum;
            int ry = r / inputNum;
            if (adj[rx,ry]) return null;
            if (input[rx] == null) {
                Node n1 = new Node(possibleInputs[rx],new int[] {rx,-1},rx,"input");
                input[rx] = n1;
            }
            if (output[ry] == null) {
                Node n2 = new Node(possibleOutputs[ry],new int[] {-1,ry},ry,"output");
                output[ry] = n2;
            }
            l = input[rx].addLink(output[ry]);
            l.index = index;
            links.Add(l);
            adj[rx,ry] = true;
            linkNum++;
        } else if (r < inputNum*(outputNum+hiddenNum)) {
            r = r-(inputNum*outputNum);
            int rx = r % inputNum;
            int ry = r / inputNum;
            int ix = rx;
            int iy = outputNum + ry;
            if (adj[ix,iy]) return null;
            if (input[rx] == null) {
                Node n1 = new Node(possibleInputs[rx],new int[] {ix,-1},rx,"input");
                input[rx] = n1;
            }
            if (hidden[ry] == null) {
                Node n2 = new Node(possibleHiddens[ry],new int[] {inputNum+ry,iy},ry,"hidden");
                hidden[ry] = n2;
            }
            l = input[rx].addLink(hidden[ry]);
            l.index = index;
            links.Add(l);
            adj[ix,iy] = true;
            linkNum++;
        } else {
            r = r-(inputNum*(outputNum+hiddenNum));
            int rx = r % hiddenNum;
            int ry = r / hiddenNum;
            int ix = inputNum + rx;
            int iy = ry;
            if (adj[ix,iy]) return null;
            if (hidden[rx] == null) {
                Node n1 = new Node(possibleHiddens[rx],new int[] {ix,outputNum+rx},rx,"hidden");
                hidden[rx] = n1;
            }
            if (output[ry] == null) {
                Node n2 = new Node(possibleOutputs[ry],new int[] {-1,iy},ry,"output");
                output[ry] = n2;
            }
            l = hidden[rx].addLink(output[ry]);
            l.index = index;
            links.Add(l);
            adj[ix,iy] = true;
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
        //Debug.Log(w.source.index + " " + w.dest.index);
        adj[w.source.index[0],w.dest.index[1]] = false;
        cleanS(w.source);
        cleanD(w.dest);
        links.Remove(w);
        linkNum--;
    }

    public void cleanS(Node n) {
        if (n.index[0] < inputNum) {
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
        if (n.index[1] < outputNum) {
            if (!checkOutputNodeLinked(n.index[1])) {
                output[n.index2] = null;
            }
        } else {
            if (!checkHiddenNodeLinked(n.index)) {
                hidden[n.index2] = null;
            }
        }
    }

    public bool checkHiddenNodeLinked(int[] index) {
        bool r = false;
        for (int i = 0; i < inputNum; i++) {
            if (adj[i,index[1]]) r = true;
        }
        for (int i = 0; i < outputNum; i++) {
            if (adj[index[0],i]) r = true;
        }
        return r;
    }

    public bool checkOutputNodeLinked(int index) {
        bool r = false;
        for (int i = 0; i < outputNum+hiddenNum; i++) {
            if (adj[i,index]) r = true;
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
    public int[] index; //adj
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

    public Node(string n, int[] i1, int i2, string t) {
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
    public static bool showZeroWeightLinks = false;

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

    public void print() {
        if (!showZeroWeightLinks && weight == 0) return;
        if (source.name != "random") {
            Debug.Log(source.name + " - " + weight + " - " + dest.name);
        } else {
            Debug.Log(source.name + ": " + source.value + " - " + weight + " - " + dest.name);
        }
    }
}

public class Plant() {
    public int id;
    static int nextId = 0;

    public gameObject plantGameObject;
    static gameObject firstGameObject;
    static Texture2D tex = new Texture2D(10,10);

    public Plant() {
        id = nextId++;
        if (!initialized) {
            makePlant();
            initialized = !initialized;
        }
        plantGameObject = Object.Instantiate(firstGameobject,new Vector3(0, 0, 0), Quaternion.identity);
        plantGameObject.SetActive(true);
    }

    makePlant() {
        Color[] colorArray = new Color[tex.GetPixels().Length];
        for (int i = 0; i < colorArray.Length; i++) {
            colorArray[i] = Color.green;
        }
        tex.SetPixels(colorArray);
        tex.Apply();
        Sprite plantSprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),new Vector2(0.5f, 0.5f),20);
        firstGameobject = new GameObject();
        firstGameobject.SetActive(false);
        firstGameobject.name = "plant";
        SpriteRenderer sr = firstGameobject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.sortingOrder = 1;
        sr.sprite = creature1Sprite;
        BoxCollider2D collider = firstGameobject.AddComponent<BoxCollider2D>() as BoxCollider2D;
    }

    public void setRandPos() {
        plantGameObject.transform.position = new Vector3(Random.Range(-GlobalVars.backgroundWidth/2,GlobalVars.backgroundWidth/2),Random.Range(-GlobalVars.backgroundHeight/2,GlobalVars.backgroundHeight/2),0);
    }
}