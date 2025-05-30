using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;

    Vector2 moveDir = Vector2.zero;

    Rigidbody2D playerRB;
    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            moveDir = new Vector2(0, 1f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveDir = new Vector2(-1f, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDir = new Vector2(0, -1f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDir = new Vector2(1f, 0);
        }

        playerRB.velocity = moveDir.normalized * moveSpeed;
    }
}
