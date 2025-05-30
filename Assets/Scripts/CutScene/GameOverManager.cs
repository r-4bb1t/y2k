using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOverManager : MonoBehaviour
{
    public GameObject graphic;
    public GameObject title;
    public float fadeDuration = 1.0f;
    public float delayBetweenFades = 0.5f;

    void Start()
    {
        InitializeAlpha(graphic);
        InitializeAlpha(title);
        StartCoroutine(PlayGameOver());
    }

    IEnumerator PlayGameOver()
    {
        yield return StartCoroutine(FadeIn(graphic));
        yield return new WaitForSeconds(delayBetweenFades);
        yield return StartCoroutine(FadeInAndMoveUp(title));
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("Classroom");
    }
    void InitializeAlpha(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color color = sr.color;
            color.a = 0f;
            sr.color = color;
        }
    }
    IEnumerator FadeIn(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color color = sr.color;
        color.a = 0;
        sr.color = color;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            sr.color = color;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        sr.color = color;
    }


    IEnumerator FadeInAndMoveUp(GameObject obj)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color color = sr.color;
        color.a = 0;
        sr.color = color;

        Vector3 startPosition = obj.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 1.0f;

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            color.a = Mathf.Lerp(0, 1, t);
            sr.color = color;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = 1;
        sr.color = color;
        obj.transform.position = endPosition;
    }
}
