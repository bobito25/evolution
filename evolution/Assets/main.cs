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
    int numEntities;

    public int time;

    void Awake()
    {
        makeBackground();
        time = 0;
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
        doBehaviour();
        if (time % 10 == 0) {
            mutate();
        }
        time++;
    }

    void spawnEntities() {
        entities = new List<Entity>();
        numEntities = 0;
        for (int i = 0; i < 5; i++) {
            entities.Add(new Creature1());
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
        int r = Random.Range(0,numEntities);
        entities[r].mutate();
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
    int possibleLinks;

    string[] possibleInputs;
    string[] possibleHiddens;
    string[] possibleOutputs;

    bool[,] adj;

    List<Link> links;

    Node[] input;
    int inputNum;
    Node[] hidden;
    int hiddenNum;
    Node[] output;
    int outputNum;

    bool first;

    public Net(int tL, int nH) { //max links and num hidden
        linkNum = 0;
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
        int r = Random.Range(0,possibleLinks);
        if (r < inputNum*outputNum) {
            int r2 = r / inputNum;
            int r3 = r % inputNum;
            if (adj[r3,r2]) return;
            if (input[r3] == null) {
                Node n1 = new Node(possibleInputs[r3],r3,r3);
                input[r3] = n1;
            }
            if (output[r2] == null) {
                Node n2 = new Node(possibleOutputs[r2],r2,r2);
                output[r2] = n2;
            }
            Link l = input[r3].addLink(output[r2]);
            links.Add(l);
            adj[r3,r2] = true;
            linkNum++;
        } else if (r < inputNum*(outputNum+hiddenNum)) {
            r = r-(inputNum*outputNum);
            int r2 = r / inputNum;
            int r3 = r % inputNum;
            int i2 = outputNum + r2;
            int i3 = r3;
            if (adj[i3,i2]) return;
            if (input[r3] == null) {
                Node n1 = new Node(possibleInputs[r3],i3,r3);
                input[r3] = n1;
            }
            if (hidden[r2] == null) {
                Node n2 = new Node(possibleHiddens[r2],i2,r2);
                hidden[r2] = n2;
            }
            Link l = input[r3].addLink(hidden[r2]);
            links.Add(l);
            adj[i3,i2] = true;
            linkNum++;
        } else {
            r = r-(inputNum*(outputNum+hiddenNum));
            int r2 = r / hiddenNum;
            int r3 = r % hiddenNum;
            int i2 = r2;
            int i3 = inputNum + r3;
            if (adj[i3,i2]) return;
            if (hidden[r3] == null) {
                Node n1 = new Node(possibleHiddens[r3],i3,r3);
                hidden[r3] = n1;
            }
            if (output[r2] == null) {
                Node n2 = new Node(possibleOutputs[r2],i2,r2);
                output[r2] = n2;
            }
            Link l = hidden[r3].addLink(output[r2]);
            links.Add(l);
            adj[i3,i2] = true;
            linkNum++;
        }
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
    public string type;
    public List<Link> links;
    public int index; //adj
    public int index2; //bool array

    public Node(string t, List<Link> l) {
        if (t == "constant") {
            value = 1;
        } else {
            value = 0;
        }
        type = t;
        links = l;
    }

    public Node(string t) {
        if (t == "constant") {
            value = 1;
        } else {
            value = 0;
        }
        type = t;
        links = new List<Link>();
    }

    public Node(string t, int i1, int i2) {
        if (t == "constant") {
            value = 1;
        } else {
            value = 0;
        }
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