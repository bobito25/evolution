using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODOs:

//  make game states saveable

//  change evo pressure: add food, hunger, seeFood input

//  change weights to be less random values? -> example: only mutiples of 0.5 between -2 and 2

//  gamespeed slider

//  visualize behaviour nets

/////////////////////////////////////////////////////////////////////////////////////////////////////////

public class main : MonoBehaviour
{
    List<Entity> entities;
    int numEntities;
    int maxEntities;

    List<Behaviourable> behaviourables;
    int numCreatures;
    int maxCreatures;

    List<Plant> plants;
    int numPlants;
    int maxPlants;

    Behaviourable highlighted;

    public int time;
    public bool run;

    public float mutChance;

    void Awake()
    {
        time = 1;
        run = false;
        maxCreatures = 50;
        maxPlants = 50;
        maxEntities = maxCreatures + maxPlants;
        makeBackground();
        initEntities();
        fixCameraSize();
        mutChance = 0.05f;
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnEntities();
        run = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            checkHighlight();
        }
        eatPlants();
    }

    void FixedUpdate()
    {
        if (run) {
            doBehaviour();
            killStarved();
            if (time % 10 == 0) {
                hunger();
            }
            if (time % 20 == 0) {
                mutate();
            }
            if (time % 100 == 0) {
                grow();
            }
            if (time % 1000 == 0) {
                nextGen();
            }
            checkAllDead();
            time++;
        }
    }

    void fixCameraSize() {
        Camera.main.orthographicSize = GlobalVars.backgroundWidth/2;
    }

    void spawnEntities() {
        entities = new List<Entity>();
        numEntities = 0;
        behaviourables = new List<Behaviourable>();
        plants = new List<Plant>();
        spawnCreatures();
        spawnPlants();
    }

    void spawnCreatures() {
        numCreatures = 0;
        for (int i = 0; i < maxCreatures; i++) {
            Entity e = new Creature1();
            entities.Add(e);
            behaviourables.Add((Behaviourable)e);
            e.setRandPos();
            numCreatures++;
            numEntities++;
        }
    }

    void spawnPlants() {
        numPlants = 0;
        for (int i = 0; i < maxPlants; i++) {
            Plant p = new Plant();
            entities.Add(p);
            plants.Add(p);
            p.setRandPos();
            numPlants++;
            numEntities++;
        }
    }

    void initEntities() {
        Entity.init();
    }

    void makeBackground() {
        Texture2D backgroundTex = new Texture2D(GlobalVars.backgroundWidth,GlobalVars.backgroundHeight);
        Color[] colorArray = new Color[backgroundTex.GetPixels().Length];
        for (int i = 0; i < colorArray.Length; i++) {
            colorArray[i] = Color.white;
        }
        backgroundTex.SetPixels(colorArray);
        backgroundTex.Apply();
        Sprite backgroundSprite = Sprite.Create(backgroundTex, new Rect(0,0,backgroundTex.width,backgroundTex.height),new Vector2(0.5f, 0.5f),1);
        GameObject background = new GameObject();
        background.name = "background";
        SpriteRenderer sr = background.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.sprite = backgroundSprite;
    }

    void checkHighlight() {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), new Vector3(0,0,-1), 2);
        if (hit.collider != null)
        {
            Behaviourable e = behaviourables.Find(x => x.entityGameObject == hit.collider.gameObject);
            if (e == null) return;
            if (e == highlighted) {
                e.unhighlight();
                return;
            }
            Debug.Log("Entity: " + e.id);
            e.showStomachFullness();
            e.showBehaviourNet();
            if (highlighted != null) highlighted.unhighlight();
            highlighted = e;
            e.highlight();
        }
    }

    void hunger() {
        foreach(Behaviourable e in behaviourables) {
            e.hunger();
        }
    }

    void grow() {
        Plant p = new Plant();
        entities.Add(p);
        plants.Add(p);
        p.setRandPos();
        numPlants++;
        numEntities++;
    }

    void eatPlants() {
        List<Entity> toDie = new List<Entity>();
        foreach(Plant p in plants) {
            Collider2D c = p.getTouchingBehaviourable();
            if (c != null) {
                toDie.Add(p);
                Behaviourable b = behaviourables.Find(x => x.entityGameObject == c.gameObject);
                b.eat();
            }
        }
        foreach(Entity e in toDie) {
            kill(e);
        }
    }

    void doBehaviour() {
        setInputs();
        calcBehaviours();
        moveEntities();
    }

    void setInputs() {
        foreach(Behaviourable e in behaviourables) {
            e.setInput();
        }
    }

    void calcBehaviours() {
        foreach(Behaviourable e in behaviourables) {
            e.calcBehaviour();
        }
    }

    void moveEntities() {
        foreach(Behaviourable e in behaviourables) {
            e.move();
            e.checkBoundaries();
        }
    }

    void mutate() {
        foreach (Behaviourable b in behaviourables) {
            if (Random.value < mutChance) b.mutate();
        }
    }

    void nextGen() {
        multiplyEach();
    }

    void killStarved() {
        List<Entity> toDie = new List<Entity>();
        foreach(Behaviourable e in behaviourables) {
            if (e.stomachFullness <= 0) toDie.Add(e);
        }
        foreach(Entity e in toDie) {
            kill(e);
        }
    }

    void deleteTouching() {
        List<Entity> toDie = new List<Entity>();
        foreach(Behaviourable e in behaviourables) {
            if (e.isTouchingBehaviourable()) toDie.Add(e);
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

    void multiplyAny() {
        if (numCreatures == 0) return;
        while (numCreatures < maxCreatures) {
            int r = Random.Range(0,numCreatures);
            Entity e = behaviourables[r];
            Entity newE = new Creature1();
            ((Behaviourable)newE).behaviour = ((Behaviourable)e).behaviour.clone();
            entities.Add(newE);
            behaviourables.Add((Behaviourable)newE);
            newE.setRandPos();
            numEntities++;
            numCreatures++;
        }
    }

    void multiplyEach() {
        if (numCreatures == 0) return;
        List<Behaviourable>
        foreach (Behaviourable b in behaviourables) {
            Behaviourable newE = new Creature1();
            newE.behaviour = (b).behaviour.clone();
            entities.Add(newE);
            behaviourables.Add(newE);
            newE.setRandPos();
            numEntities++;
            numCreatures++;
        }
    }

    void kill(Entity e) {
        Destroy(e.entityGameObject);
        if (e is Behaviourable) {
            behaviourables.Remove((Behaviourable)e);
            numCreatures--;
        } else if (e is Plant) {
            plants.Remove((Plant)e);
            numPlants--;
        }
        entities.Remove(e);
        if (highlighted == e) highlighted = null;
        numEntities--;
    }
}




