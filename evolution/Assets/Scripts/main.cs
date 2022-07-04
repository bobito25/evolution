using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODOs:

//  implement way to see bahaviour nets while running / clickable nodes

//  make game states saveable

//  change evo pressure: add food, hunger, seeFood input

//  change weights to be less random values? -> example: only mutiples of 0.5 between -2 and 2

//  gamespeed slider

//  fix camera (dynammically)

/////////////////////////////////////////////////////////////////////////////////////////////////////////

public class main : MonoBehaviour
{
    List<Entity> entities;
    int numEntities;
    int maxEntities;

    List<Entity> behaviourables;
    int numCreatures;
    int maxCreatures;

    int numPlants;
    int maxPlants;

    Entity highlighted;

    public int time;
    public bool run;

    void Awake()
    {
        time = 1;
        run = false;
        maxCreatures = 50;
        maxPlants = 30;
        maxEntities = maxCreatures + maxPlants;
        makeBackground();
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnEntities();
        spawnPlants();
        run = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            checkHighlight();
        }
    }

    void FixedUpdate()
    {
        if (run) {
            doBehaviour();
            if (time % 20 == 0) {
                mutate();
            }
            if (time % 1000 == 0) {
                nextGen();
            }
            checkAllDead();
            time++;
        }
    }

    void spawnEntities() {
        entities = new List<Entity>();
        numEntities = 0;
        behaviourables = new List<Entity>();
        spawnCreatures();
        spawnPlants();
    }

    void spawnCreatures() {
        numCreatures = 0;
        for (int i = 0; i < maxCreatures; i++) {
            Entity e = new Creature1();
            entities.Add(e);
            bahaviourables.Add(e);
            e.setRandPos();
            numCreatures++;
            numEntities++;
        }
    }

    void spawnPlants() {
        numPlants = 0;
        for (int i = 0; i < maxPlants; i++) {
            Plant p = new Plant();
            plants.Add(p);
            p.setRandPos();
            numPlants++;
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

    void checkHighlight() {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0,0,-1), 2);
        if (hit.collider != null)
        {
            Entity e = entities.Find(x => x.entityGameObject == hit.collider.gameObject);
            if (!e.hasBehaviour) return;
            if (e == highlighted) {
                e.unhighlight();
                return;
            }
            e.showBehaviourNet();
            if (highlighted != null) highlighted.unhighlight();
            highlighted = e;
            e.highlight();
        }
    }

    void doBehaviour() {
        setInputs();
        calcBehaviours();
        moveEntities();
    }

    void setInputs() {
        foreach(Entity e in behaviourables) {
            e.setInput();
        }
    }

    void calcBehaviours() {
        foreach(Entity e in behaviourables) {
            e.calcBehaviour();
        }
    }

    void moveEntities() {
        foreach(Entity e in entities) {
            if (e.canMove) e.move();
            e.checkBoundaries();
        }
    }

    void mutate() {
        if (numCreatures == 0) return;
        int r = Random.Range(0,numCreatures);
        behaviourables[r].mutate();
    }

    void nextGen() {
        deleteTouching();
        multiply();
    }

    void deleteTouching() {
        List<Entity> toDie = new List<Entity>();
        foreach(Entity e in creatures) {
            if (e.isTouching()) toDie.Add(e);
        }
        foreach(Entity e in toDie) {
            kill(e);
        }
    }

    void checkAllDead() {
        if (numCreatures == 0) {
            Debug.Log("all creatures are dead, time: " + time);
            run = false;
        }
    }

    void multiply() {
        if (numCreatures == 0) return;
        while (numEntities < maxCreatures) {
            int r = Random.Range(0,numCreatures);
            Entity e = behaviourables[r];
            Entity newE = new Creature1();
            newE.behaviour = e.behaviour.clone();
            entities.Add(newE);
            behaviourables.Add(newE);
            newE.setRandPos();
            numEntities++;
            numCreatures++;
        }
    }

    void kill(Entity e) {
        Destroy(e.entityGameObject);
        entities.Remove(e);
        if (highlighted == e) highlighted = null;
        numEntities--;
        numCreatures--;
    }
}




