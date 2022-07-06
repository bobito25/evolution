using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Globalization;
using UnityEngine;

/////////////////////////////////////////////////////////////////////////////////////////////////////////

// TODOs:

//  gamespeed slider

//  visualize behaviour nets

/////////////////////////////////////////////////////////////////////////////////////////////////////////

public class main : MonoBehaviour
{
    int sessionID;

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
    public float multiplyChance;

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
        mutChance = 0.02f;
        multiplyChance = 0.01f;
        sessionID = UnityEngine.Random.Range(1000,9999);
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnEntities();
        run = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) checkHighlight();
        if (Input.GetKeyDown("space")) run = !run;
        if (Input.GetKeyDown("s")) save();
    }

    void FixedUpdate()
    {
        if (run) {
            List<Entity> toDie = new List<Entity>();
            List<Entity> toClone = new List<Entity>();

            foreach(Behaviourable b in behaviourables) {
                doBehaviour(b);
                killStarved(b, toDie);
                if (time % 10 == 0) {
                    b.hunger();
                }
                if (time % 20 == 0) {
                    if (UnityEngine.Random.value < mutChance) b.mutate();
                    if (UnityEngine.Random.value < multiplyChance && b.stomachFullness > 100) toClone.Add(b);
                }
            }

            foreach(Plant p in plants) checkEatPlant(p, toDie);
            
            if (time % 100 == 0) grow();

            foreach(Entity e in toDie) kill(e);
            foreach(Entity e in toClone) clone((Behaviourable)e);

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
            Entity p = new Plant();
            p.setRandPos();
            entities.Add(p);
            plants.Add((Plant)p);
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

    void checkEatPlant(Plant p, List<Entity> toDie) {
        Collider2D c = p.getTouchingBehaviourable();
        if (c != null) {
            toDie.Add(p);
            Behaviourable b = behaviourables.Find(x => x.entityGameObject == c.gameObject);
            b.eat();
        }
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

    void doBehaviour(Behaviourable b) {
        b.setInput();
        b.calcBehaviour();
        b.move();
        b.checkBoundaries();
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
            if (UnityEngine.Random.value < mutChance) b.mutate();
        }
    }

    void nextGen() {
        
    }

    void killStarved(Behaviourable b, List<Entity> toDie) {
        if (b.stomachFullness <= 0) toDie.Add(b);
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
            int r = UnityEngine.Random.Range(0,numCreatures);
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

    void clone(Behaviourable b) {
        Behaviourable newE = new Creature1();
        newE.behaviour = (b).behaviour.clone();
        entities.Add(newE);
        behaviourables.Add(newE);
        newE.setRandPos();
        numEntities++;
        numCreatures++;
    }

    void multiplyEach(List<Behaviourable> bs) {
        if (numCreatures == 0) return;
        foreach (Behaviourable b in bs) {
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

    void save() {
        string contents = "";
        List<string> toAdd = new List<string>();
        toAdd.Add(sessionID.ToString());
        toAdd.Add(time.ToString());
        toAdd.Add(mutChance.ToString("0.00"));
        toAdd.Add(multiplyChance.ToString("0.00"));
        toAdd.Add(maxEntities.ToString());
        toAdd.Add(maxCreatures.ToString());
        toAdd.Add(maxPlants.ToString());
        toAdd.Add("\nb:");
        foreach (Behaviourable b in behaviourables) toAdd.Add(b.ToString());
        toAdd.Add("p:");
        foreach (Plant p in plants) toAdd.Add(p.ToString());
        foreach (string s in toAdd) contents += s + "\n";
        var culture = new CultureInfo("de-DE");
        DateTime localDate = DateTime.Now;
        string date = localDate.ToString(culture);
        string noColons = date.Replace(':',';');
        string name = sessionID.ToString() + " - " + noColons;
        WriteString(name, contents);
        Debug.Log("saved game state at time: " + time);
    }

    void load(string contents) {
        string[] c = contents.Split('\n');
        sessionID = Int32.Parse(c[0]);
        time = Int32.Parse(c[1]);
        mutChance = Int32.Parse(c[2]);
        multiplyChance = Int32.Parse(c[3]);
        maxEntities = Int32.Parse(c[4]);
        maxCreatures = Int32.Parse(c[5]);
        maxPlants = Int32.Parse(c[6]);
        int i = 9;
        while (c[i] != "p:") {
            string[] b = new string[7];
            Array.Copy(c,i,b,0,7);
            loadBehaviourable(b);
            i += 2;
        }
        i++;
        while (i < c.Length) {
            string[] p = new string[3];
            Array.Copy(c,i,p,0,3);
            loadPlant(p);
            i += 2;
        }
    }

    void loadBehaviourable(string[] b) {
        Entity e = new Creature1();
        e.id = Int32.Parse(b[0]);
        e.setPos(float.Parse(b[1],CultureInfo.InvariantCulture.NumberFormat),float.Parse(b[2],CultureInfo.InvariantCulture.NumberFormat));
        ((Behaviourable)e).stomachFullness = Int32.Parse(b[3]);
        ((Behaviourable)e).hungerRate = Int32.Parse(b[4]);
        string[] linkStrings = b[6].Remove(b[6].Length-1).Split(" ");
        int[] links = new int[linkStrings.Length];
        for (int i = 0; i < linkStrings.Length; i++) links[i] = Int32.Parse(linkStrings[i]);
        ((Behaviourable)e).behaviour = new Net(Int32.Parse(b[5]),links);
        entities.Add(e);
        behaviourables.Add((Behaviourable)e);
        numCreatures++;
        numEntities++;
    }

    void loadPlant(string[] p) {
        Entity e = new Plant();
        e.id = Int32.Parse(p[0]);
        e.setPos(float.Parse(p[1],CultureInfo.InvariantCulture.NumberFormat),float.Parse(p[2],CultureInfo.InvariantCulture.NumberFormat));
        entities.Add(e);
        plants.Add((Plant)e);
        numPlants++;
        numEntities++;
    }

    public static void WriteString(string name, string contents) {
        string path = Application.persistentDataPath + "/" + name + ".txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(contents);
        writer.Close();
    }

    public static string ReadString(string name) {
        string path = Application.persistentDataPath + "/" + name + ".txt";
        StreamReader reader = new StreamReader(path);
        string r =  reader.ReadToEnd();
        reader.Close();
        Debug.Log("loaded game state");
        return r;
    }
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////

// path to saves: C:/Users/Leon/AppData/LocalLow/DefaultCompany/evolution

//path laptop: C:\Users\Huawei\AppData\LocalLow\DefaultCompany\evolution

///////////////////////////////////////////////////////////////////////////////////////////////////////////

//longest time before extinction: 616761 (equal to: ~6167 seconds or ~102 minutes or ~1.7 hours)