using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    float horizontalInput;
    float verticalInput;

    public bool isHidden = false;
    public bool isPlayerMoving = false;

    Vector2 moveDir = Vector2.zero;
    SpriteRenderer playerRenderer;
    Rigidbody2D playerRB;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
        playerRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(horizontalInput, verticalInput).normalized;

        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        if (horizontalInput == 0 && verticalInput == 0)
        {
            animator.SetBool("isMoving", false);
            isPlayerMoving = false;
        }
        else
        {
            animator.SetBool("isMoving", true);
            isPlayerMoving = true;
        }

        playerRB.velocity = moveDir * moveSpeed;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Hidable"))
        {
            isHidden = true;
            playerRenderer.enabled = false;
        }
        if (other.CompareTag("Finish"))
        {
            Debug.Log("탈출!");
            //다음 스테이지로 진행
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Hidable"))
        {
            isHidden = false;
            playerRenderer.enabled = true;
        }
    }
}
