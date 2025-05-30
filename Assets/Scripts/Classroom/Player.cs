using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Teacher teacher;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        teacher = FindObjectOfType<Teacher>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        Vector2 movement = new Vector2(moveX, moveY).normalized;

        if (teacher.isWatching && (moveX != 0 || moveY != 0))
        {
            if (!IsHiddenBehindCover())
            {
                GameOver();
            }
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
}
