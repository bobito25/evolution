using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature1 : Behaviourable
{
    public static GameObject firstGameobject;
    static Texture2D tex = new Texture2D(10,10);

    public Creature1() {
        entityGameObject = Object.Instantiate(firstGameobject,new Vector3(0, 0, 0), Quaternion.identity);
        entityGameObject.SetActive(true);
        stomachFullness = 300;
        entityGameObject.layer = 10;
    }
    
    public static void initEntity() {
        hungerRate = 1;
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
        behaviour.setInput(checkSeePlant());
    }

    public bool checkSeeEntity() {
        Vector3 fwd = entityGameObject.transform.up;
        Vector3 origin = entityGameObject.transform.position+(entityGameObject.transform.up/2);
        //Debug.DrawRay(origin, fwd*3, Color.yellow);
        RaycastHit2D hit = Physics2D.Raycast(origin, fwd, 3);
        return hit.collider != null;
    }

    public bool checkSeePlant() {
        Vector3 fwd = entityGameObject.transform.up;
        Vector3 origin = entityGameObject.transform.position+(entityGameObject.transform.up/2);
        RaycastHit2D hit = Physics2D.Raycast(origin, fwd, 3);
        //if (hit.collider != null) Debug.Log(hit.collider.name);
        return hit.collider != null && hit.collider.name == "plant(Clone)";
    }

    public override void calcBehaviour() {
        nextAction = behaviour.calcOutput();
    }

    public override void move() {
        entityGameObject.transform.Rotate(new Vector3(0,0,nextAction.rotation));
        entityGameObject.transform.Translate(new Vector3(0,0.01f*nextAction.forwardM,0));
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