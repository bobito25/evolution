using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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