using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity
{
    static int nextId = 0;

    public static GameObject firstGameobject;

    public int id;
    public Net behaviour;
    public GameObject entityGameObject;
    public int speed = 10;

    public Actions nextAction;

    public bool canMove;
    public bool hasBehaviour;
    
    public Entity() {
        id = nextId++;
        if (hasBehaviour) behaviour = new Net(5,1);
        setBools();
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
    
    public void checkBoundaries() {
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

    public abstract void setBools();
    public abstract void highlight();
    public abstract void unhighlight();
    public abstract void showBehaviourNet();
    public abstract void setInput();
    public abstract void calcBehaviour();
    public abstract void move();
    public abstract void makeEntity();
    public abstract void mutate();
}
