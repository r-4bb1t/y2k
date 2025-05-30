using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    public bool isWatching = false;
    public Sprite defaultSprite;
    public Sprite watchingSprite;
    private SpriteRenderer spriteRenderer;
    private float nextChangeTime;
    private float changeInterval;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetNextChangeTime();
    }

    void Update()
    {
        if (Time.time >= nextChangeTime)
        {
            isWatching = true;
            spriteRenderer.sprite = watchingSprite;
            StartCoroutine(ResetWatching());
            SetNextChangeTime();
        }
    }

    void SetNextChangeTime()
    {
        changeInterval = Random.Range(1f, 5f);
        nextChangeTime = Time.time + changeInterval;
    }

    IEnumerator ResetWatching()
    {
        changeInterval = Random.Range(0.5f, 3f);
        yield return new WaitForSeconds(changeInterval);
        isWatching = false;
        spriteRenderer.sprite = defaultSprite;
    }
}
