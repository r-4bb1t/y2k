using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb; // 3D의 경우 Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 3D: GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 키보드 입력 받기
        float moveX = Input.GetAxisRaw("Horizontal"); // 좌우 (A, D, <-, ->)
        float moveY = Input.GetAxisRaw("Vertical");   // 상하 (W, S, up, down)

        // 이동 벡터 생성 및 정규화 (대각선 이동 시 속도 빨라짐 방지)
        Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

        // 이동 적용 (Rigidbody를 사용할 경우 FixedUpdate에서 물리 처리하는 것이 더 정확할 수 있음)
        rb.velocity = moveDirection * moveSpeed;
    }

    // 충돌 감지
    void OnCollisionEnter2D(Collision2D collision) // 3D: void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zombie")) // 좀비와 충돌했는지 태그로 확인
        {
            Debug.Log("게임 오버!");
            // GameManager에게 게임 오버 알리기
            FindObjectOfType<GameManager>().GameOver();
            gameObject.SetActive(false); // 플레이어 비활성화
        }
    }
}
