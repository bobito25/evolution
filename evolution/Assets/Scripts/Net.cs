using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (linksAtStart > possibleLinks) {
            Debug.Log("error: links per net higher than possible link amount (funct Net)");
        } 

        adj = new bool[inputNum+hiddenNum,outputNum+hiddenNum];

        input = new Node[inputNum];
        hidden = new Node[hiddenNum];
        output = new Node[outputNum];

        for (int i = 0; i < linksAtStart; i++) {
            addRandomLink();
        }
    }

    public Net(int nH, int[] ls) { //max links and num hidden
        linkNum = 0;
        linksAtStart = ls.Length;
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
        if (linksAtStart > possibleLinks) {
            Debug.Log("error: links per net higher than possible link amount (funct Net)");
        } 

        adj = new bool[inputNum+hiddenNum,outputNum+hiddenNum];

        input = new Node[inputNum];
        hidden = new Node[hiddenNum];
        output = new Node[outputNum];

        for (int i = 0; i < linksAtStart; i++) {
            addLink(ls[i]);
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

    public override string ToString() {
        string r = "";
        r += hiddenNum.ToString() + "\n";
        foreach (Link l in links) {
            r += l.index.ToString() + " ";
        }
        return r;
    }
}



