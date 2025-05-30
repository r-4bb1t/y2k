using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxControl : MonoBehaviour
{
    public bool isFollowingPlayer = false;

    public GameObject player;

    void Update()
    {
        if (isFollowingPlayer)
        {
            transform.position = player.transform.position;
        }

        if (Input.GetKey(KeyCode.E))
        {
            isFollowingPlayer = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isFollowingPlayer = true;
        }
    }
}
