using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Actions {
    public float forwardM;
    public float rotation;

    public Actions(float m, float r) {
        forwardM = m;
        rotation = r;
    }
}