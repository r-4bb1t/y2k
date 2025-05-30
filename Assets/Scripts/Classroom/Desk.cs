using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Desk : MonoBehaviour
{
    void Start()
    {

    }

    void LateUpdate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            string[] deskSprites = { "Desk", "Desk2", "Desk3" };
            string selectedSprite = deskSprites[Random.Range(0, deskSprites.Length)];
            sr.sprite = Resources.Load<Sprite>(selectedSprite);
            sr.sortingOrder = 10 - (int)(transform.position.y);
        }
    }
    void Update()
    {

    }
}
