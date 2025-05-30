using UnityEngine;

public class RandomZombieAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;                // 이동 속도
    public float changeDirectionInterval = 3f;    // 방향을 바꾸는 주기 (초)

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float directionTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb.gravityScale != 0f) // 혹시 모를 중력 제거
        {
            rb.gravityScale = 0f;
        }
        PickNewDirection();
    }

    void Update()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            PickNewDirection();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * moveSpeed;

        // 이동 방향으로 회전 (선택 사항, 스프라이트가 위쪽을 기본으로 볼 때 -90도 오프셋)
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void PickNewDirection()
    {
        // -1.0 ~ 1.0 사이의 랜덤한 x, y 값을 가진 정규화된 벡터를 얻음 (원형 범위 내 랜덤 방향)
        moveDirection = Random.insideUnitCircle.normalized;
        directionTimer = changeDirectionInterval; // 타이머 리셋
        // Debug.Log(gameObject.name + " is moving in direction: " + moveDirection);
    }

    // 벽이나 다른 장애물에 부딪혔을 때 방향을 바꾸는 로직 (선택 사항)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어나 다른 좀비가 아닌 것과 부딪혔을 때 (예: 벽)
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Zombie"))
        {
            // Debug.Log(gameObject.name + " collided with " + collision.gameObject.name + ", changing direction.");
            PickNewDirection(); // 새 방향 선택
        }
    }
}