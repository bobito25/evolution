using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Behaviourable : Entity {
    public Net behaviour;
    public Actions nextAction;

    public int stomachFullness;
    public int hungerRate;

    public Behaviourable() {
        behaviour = new Net(5,1);
    }

    public void showStomachFullness() {
        Debug.Log("stomach fullness: " + stomachFullness);
    }

    public void eat() {
        if (stomachFullness <= 500) stomachFullness += Plant.nutrition;
    }

    public void hunger () {
        if (stomachFullness > 0) stomachFullness -= hungerRate;
    }

    public abstract void mutate();
    public abstract void highlight();
    public abstract void unhighlight();
    public abstract void showBehaviourNet();
    public abstract void setInput();
    public abstract void calcBehaviour();
    public abstract void move();
}