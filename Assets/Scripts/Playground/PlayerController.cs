using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // 플레이어 이동 속도

    // 컴포넌트 참조 변수
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // 좌우 반전을 위해 추가 (선택 사항)

    // 내부 상태 변수
    private Vector2 moveDirection;
    private bool isFacingRight = true; // 캐릭터가 오른쪽을 보고 있는지 여부 (좌우 반전용)

    void Start()
    {
        // 필수 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>(); // SpriteRenderer 가져오기

        // Rigidbody2D 기본 설정 (2D 탑다운 또는 사이드뷰에 적합하게)
        if (rb != null)
        {
            rb.gravityScale = 0f;        // 중력 사용 안 함 (탑다운의 경우)
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 물리적으로 회전하지 않도록 고정
        }

        if (animator == null)
        {
            Debug.LogWarning("PlayerController: Animator 컴포넌트가 없습니다!");
        }
        if (spriteRenderer == null)
        {
            Debug.LogWarning("PlayerController: SpriteRenderer 컴포넌트가 없습니다! (좌우 반전에 필요)");
        }
    }

    void Update()
    {
        // --- 1. 입력 처리 ---
        float moveX = Input.GetAxisRaw("Horizontal"); // 좌우 입력 (A, D, <-, ->)
        float moveY = Input.GetAxisRaw("Vertical");   // 상하 입력 (W, S, Up, Down)

        moveDirection = new Vector2(moveX, moveY).normalized; // 대각선 이동 시 속도 보정

        // --- 2. 애니메이션 제어 ---
        if (animator != null)
        {
            // 이동 벡터의 크기(길이)가 0.1보다 크면 움직이는 것으로 간주
            if (moveDirection.magnitude > 0.1f)
            {
                animator.SetBool("IsWalking", true);
            }
            else
            {
                animator.SetBool("IsWalking", false);
            }
        }

        // --- 3. 캐릭터 방향 전환 (좌우 반전 - 스프라이트 기준) ---
        // 탑다운 게임에서 캐릭터가 마우스나 특정 방향을 바라보게 하려면 다른 회전 로직이 필요합니다.
        // 이 코드는 캐릭터가 왼쪽 또는 오른쪽으로 이동할 때 스프라이트를 반전시킵니다.
        if (spriteRenderer != null) // spriteRenderer가 있을 때만 실행
        {
            if (moveX > 0 && !isFacingRight) // 오른쪽으로 이동하는데 왼쪽을 보고 있다면
            {
                Flip();
            }
            else if (moveX < 0 && isFacingRight) // 왼쪽으로 이동하는데 오른쪽을 보고 있다면
            {
                Flip();
            }
        }
    }

    void FixedUpdate()
    {
        // --- 4. 물리 기반 이동 ---
        // Rigidbody를 사용한 이동은 FixedUpdate에서 처리하는 것이 물리적으로 더 안정적입니다.
        if (rb != null)
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // 캐릭터 좌우 반전 함수
    void Flip()
    {
        isFacingRight = !isFacingRight; // 현재 방향 상태 반전
        // transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        // 위 방법 대신 SpriteRenderer의 flipX를 사용하는 것이 다른 Scale 문제(자식 오브젝트 등)를 피하기에 더 좋습니다.
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight; // isFacingRight가 true면 flipX는 false (원본), false면 flipX는 true (반전)
        }
    }

    // --- 5. 충돌 처리 ---
    void OnCollisionEnter2D(Collision2D collision)
    {
        // "Zombie" 태그를 가진 오브젝트와 충돌했는지 확인
        if (collision.gameObject.CompareTag("Zombie"))
        {
            Debug.Log("플레이어가 좀비와 충돌했습니다!");

            // GameManager를 찾아 GameOver 함수 호출
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.GameOver();
            }
            else
            {
                Debug.LogError("씬에서 GameManager를 찾을 수 없습니다!");
            }

            gameObject.SetActive(false); // 플레이어 오브젝트 비활성화
        }
    }

    // --- (선택 사항) 컷신 등에서 플레이어 조작을 제어하기 위한 함수 ---
    public void SetInputEnabled(bool enabled)
    {
        // 이 PlayerController 스크립트 자체를 활성화/비활성화하여 Update, FixedUpdate 실행을 제어
        this.enabled = enabled;

        if (!enabled) // 조작이 비활성화되면
        {
            if (rb != null) rb.velocity = Vector2.zero; // 물리적 움직임 정지
            if (animator != null) animator.SetBool("IsWalking", false); // 애니메이션을 Idle 상태로
        }
    }
}