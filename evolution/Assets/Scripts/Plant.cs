using static System.Array;
using static System.Math;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : Entity {

    static GameObject firstGameObject;
    static Texture2D tex = new Texture2D(10,10);

    static bool initialized = false;

    void makePlant() {
        Color[] colorArray = new Color[tex.GetPixels().Length];
        for (int i = 0; i < colorArray.Length; i++) {
            colorArray[i] = Color.green;
        }
        tex.SetPixels(colorArray);
        tex.Apply();
        Sprite plantSprite = Sprite.Create(tex, new Rect(0,0,tex.width,tex.height),new Vector2(0.5f, 0.5f),20);
        firstGameObject = new GameObject();
        firstGameObject.SetActive(false);
        firstGameObject.name = "plant";
        SpriteRenderer sr = firstGameObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        sr.sortingOrder = 1;
        sr.sprite = plantSprite;
        BoxCollider2D collider = firstGameObject.AddComponent<BoxCollider2D>() as BoxCollider2D;
    }
    
    public override void setBools() {
        canMove = false;
        hasBehaviour = false;
    }
}