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
            sr.sortingOrder = 10 - (int)(transform.position.y);
        }
    }
    void Update()
    {

    }
}
