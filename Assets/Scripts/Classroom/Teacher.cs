using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teacher : MonoBehaviour
{
    public bool isWatching = false;
    public Sprite defaultSprite;
    public Sprite watchingSprite;
    public Sprite surprisedSprite;
    private SpriteRenderer spriteRenderer;
    private float nextChangeTime;
    private float changeInterval;

    [SerializeField] private float warningTime = 1f;
    [SerializeField] private float minChangeTime = 2f;
    [SerializeField] private float maxChangeTime = 5f;
    [SerializeField] private float minWatchTime = 0.5f;
    [SerializeField] private float maxWatchTime = 3f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetNextChangeTime();
    }

    void Update()
    {
        if (Time.time >= nextChangeTime - warningTime && spriteRenderer.sprite != surprisedSprite)
        {
            spriteRenderer.sprite = surprisedSprite;
        }
        else if (Time.time >= nextChangeTime)
        {
            isWatching = true;
            spriteRenderer.sprite = watchingSprite;
            StartCoroutine(ResetWatching());
            SetNextChangeTime();
        }
    }

    void SetNextChangeTime()
    {
        changeInterval = Random.Range(minChangeTime, maxChangeTime);
        nextChangeTime = Time.time + changeInterval;
    }

    IEnumerator ResetWatching()
    {
        changeInterval = Random.Range(minWatchTime, maxWatchTime);
        yield return new WaitForSeconds(changeInterval);
        isWatching = false;
        spriteRenderer.sprite = defaultSprite;
    }
}
