using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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