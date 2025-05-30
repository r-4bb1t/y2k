using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private float moveSpeed = 3f;
    private Rigidbody2D rb;
    private Teacher teacher;
    public bool isGameOver { get; private set; } = false;
    public bool isGameClear { get; private set; } = false;


    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        teacher = FindObjectOfType<Teacher>();

        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    void Update()
    {
        if (isGameOver) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;
        rb.velocity = movement * moveSpeed;

        if (animator != null)
        {
            bool isMoving = movement.magnitude > 0;
            animator.SetBool("isMoving", isMoving);

            if (spriteRenderer != null && moveX != 0)
            {
                spriteRenderer.flipX = moveX < 0;
            }
        }

        if (teacher.isWatching && !IsHiddenBehindCover())
        {
            GameOver();
        }
    }



    bool IsHiddenBehindCover()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 10f, LayerMask.GetMask("Cover"));
        return hit.collider != null;
    }

    void GameOver()
    {
        if (isGameOver || isGameClear) return;
        isGameOver = true;

        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);

        StartCoroutine(DelayedFadeOut());
    }

    IEnumerator DelayedFadeOut()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("GameOver");
    }



    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Door"))
        {
            GameClear();
        }
    }

    void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10 - (int)(transform.position.y);
        }
    }

    void GameClear()
    {
        if (isGameOver || isGameClear) return;
        isGameClear = true;
        SceneManager.LoadScene("Hallway");
    }
}
