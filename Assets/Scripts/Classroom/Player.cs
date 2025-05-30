using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Teacher teacher;

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
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;

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

        rb.velocity = movement * moveSpeed;
    }

    bool IsHiddenBehindCover()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, 5f, LayerMask.GetMask("Cover"));
        return hit.collider != null;
    }

    void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Door"))
        {
            GameClear();
        }
    }

    void GameClear()
    {
        Debug.Log("게임 클리어!");
    }
}
