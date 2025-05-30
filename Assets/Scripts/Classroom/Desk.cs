using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk : MonoBehaviour
{
    private SpriteRenderer sr;
    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            string[] deskSprites = { "Sprites/Classroom/Desk", "Sprites/Classroom/Desk2", "Sprites/Classroom/Desk3" };
            string selectedSprite = deskSprites[Random.Range(0, deskSprites.Length)];
            sr.sprite = Resources.Load<Sprite>(selectedSprite);
        }
    }
    void Update()
    {

    }
}
