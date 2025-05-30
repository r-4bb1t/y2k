using UnityEngine;

public class ZombieSpawnEffect : MonoBehaviour
{
    public float fadeDuration = 0.5f; // 실체화에 걸리는 시간
    private SpriteRenderer spriteRenderer;
    private float startTime;
    private Color startColor;
    private Color endColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        endColor = new Color(startColor.r, startColor.g, startColor.b, 255f); // 완전히 불투명한 색상
        startTime = Time.time;
    }

    void Update()
    {
        float timeElapsed = Time.time - startTime;
        if (timeElapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(startColor.a, endColor.a, timeElapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
        else
        {
            spriteRenderer.color = endColor; // 완전히 불투명하게 설정
            Destroy(this); // 효과 스크립트 제거 (선택 사항)
        }
    }
}