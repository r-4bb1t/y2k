using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    public static FadeManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeOutAndReload()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    private IEnumerator FadeOutAndLoadScene()
    {
        float t = 0f;
        Color color = fadeImage.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
