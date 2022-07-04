using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behaviourable : Entity {
    public Net behaviour;
    public Actions nextAction;

    public Behaviourable() {
        behaviour = new Net(5,1);
    }

    public abstract void mutate();
    public abstract void highlight();
    public abstract void unhighlight();
    public abstract void showBehaviourNet();
    public abstract void setInput();
    public abstract void calcBehaviour();
    public abstract void move();
}