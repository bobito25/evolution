using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity
{
    static int nextId = 0;

    public int id;
    public GameObject entityGameObject;
    public int speed = 10;
    
    public Entity() {
        id = nextId++;
    }

    public static void init() {
        Plant.initEntity();
        Creature1.initEntity();
    }

    public bool isTouchingBehaviourable() {
        Collider2D collider = entityGameObject.GetComponent<Collider2D>();
        if (collider == null) Debug.Log("collider is null (funct isTouching)");
        Collider2D[] r = new Collider2D[1];
        ContactFilter2D cf = new ContactFilter2D();
        cf.useLayerMask = true;
        cf.SetLayerMask(1 << 10);
        return !(collider.OverlapCollider(cf, r) == 0);
    }

    public Collider2D getTouchingBehaviourable() {
        Collider2D collider = entityGameObject.GetComponent<Collider2D>();
        if (collider == null) Debug.Log("collider is null (funct isTouching)");
        Collider2D[] r = new Collider2D[1];
        ContactFilter2D cf = new ContactFilter2D();
        cf.useLayerMask = true;
        cf.SetLayerMask(1 << 10);
        if (collider.OverlapCollider(cf, r) != 0) {
            return r[0];
        } else {
            return null;
        }
    }

    public void setRandPos() {
        entityGameObject.transform.position = new Vector3(Random.Range(-GlobalVars.backgroundWidth/2,GlobalVars.backgroundWidth/2),Random.Range(-GlobalVars.backgroundHeight/2,GlobalVars.backgroundHeight/2),0);
    }
    
    public void checkBoundaries() {
        Rect b = new Rect(-GlobalVars.backgroundWidth/2,-GlobalVars.backgroundHeight/2,GlobalVars.backgroundWidth,GlobalVars.backgroundHeight);
        Vector3 p = new Vector3(entityGameObject.transform.position.x,entityGameObject.transform.position.y,0);
        if (!b.Contains(p)) {
            if (entityGameObject.transform.position.x > GlobalVars.backgroundWidth/2) {
                entityGameObject.transform.position += new Vector3(-GlobalVars.backgroundWidth,0,0);
            } else if (entityGameObject.transform.position.x < -GlobalVars.backgroundWidth/2) {
                entityGameObject.transform.position += new Vector3(GlobalVars.backgroundWidth,0,0);
            } else if (entityGameObject.transform.position.y > GlobalVars.backgroundHeight/2) {
                entityGameObject.transform.position += new Vector3(0,-GlobalVars.backgroundHeight,0);
            } else if (entityGameObject.transform.position.y < -GlobalVars.backgroundHeight/2) { 
                entityGameObject.transform.position += new Vector3(0,GlobalVars.backgroundHeight,0);
            }
        }
    }
}
