using UnityEngine;
uusing UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // 플레이어 이동 속도

    // 컴포넌트 참조 변수
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // 내부 상태 변수
    private Vector2 moveDirection;
    private bool isFacingRight = true;   // 캐릭터가 오른쪽을 보고 있는지 여부 (좌우 반전용)
    private bool controlsEnabled = true; // 플레이어 조작 가능 상태 플래그

    void Start()
    {
        // 필수 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Rigidbody2D 기본 설정 (2D 탑다운 또는 사이드뷰에 적합하게)
        if (rb != null)
        {
            rb.gravityScale = 0f;        // 중력 사용 안 함
            rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 물리적으로 회전하지 않도록 고정
        }
        else
        {
            Debug.LogError("PlayerController: Rigidbody2D 컴포넌트가 없습니다!");
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
        // 조작이 불가능한 상태면 Update 로직의 입력 처리 부분을 건너뜀
        if (!controlsEnabled)
        {
            // 필요하다면 여기서도 움직임을 멈추고 Idle 애니메이션을 강제할 수 있지만,
            // SetInputEnabled(false) 호출 시 이미 처리하고 있음.
            return;
        }

        // --- 1. 입력 처리 ---
        float moveX = Input.GetAxisRaw("Horizontal"); // 좌우 입력 (A, D, <-, ->)
        float moveY = Input.GetAxisRaw("Vertical");   // 상하 입력 (W, S, Up, Down)

        moveDirection = new Vector2(moveX, moveY).normalized; // 대각선 이동 시 속도 보정

        // --- 2. 애니메이션 제어 ---
        if (animator != null)
        {
            if (moveDirection.magnitude > 0.1f) // 이동 벡터의 크기가 0.1보다 크면 움직이는 것으로 간주
            {
                animator.SetBool("IsWalking", true);
            }
            else
            {
                animator.SetBool("IsWalking", false);
            }
        }

        // --- 3. 캐릭터 방향 전환 (좌우 반전 - 스프라이트 기준) ---
        if (spriteRenderer != null)
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
        // 조작이 불가능한 상태면 물리 업데이트도 건너뛸 수 있지만,
        // 보통 velocity를 0으로 설정해두면 FixedUpdate는 실행되어도 괜찮습니다.
        // 만약 SetInputEnabled(false)에서 rb.velocity = Vector2.zero를 호출했다면,
        // controlsEnabled가 false일 때 여기서 추가로 멈출 필요는 없습니다.
        if (!controlsEnabled && rb != null) // 확실히 멈추고 싶다면
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // --- 4. 물리 기반 이동 ---
        if (rb != null && controlsEnabled) // Rigidbody가 있고 조작 가능할 때만
        {
            rb.velocity = moveDirection * moveSpeed;
        }
    }

    // 캐릭터 좌우 반전 함수
    void Flip()
    {
        isFacingRight = !isFacingRight;
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

    // --- 플레이어 조작 가능/불가능 상태를 설정하는 public 함수 ---
    public void SetInputEnabled(bool status)
    {
        controlsEnabled = status;

        // 조작이 비활성화되면 즉시 움직임을 멈추고 Idle 애니메이션으로 전환
        if (!controlsEnabled)
        {
            moveDirection = Vector2.zero; // 다음 FixedUpdate에서 속도가 0이 되도록 입력 방향 초기화
            if (rb != null)
            {
                rb.velocity = Vector2.zero; // 즉시 물리적 움직임 정지
            }
            if (animator != null)
            {
                animator.SetBool("IsWalking", false); // 애니메이션을 Idle 상태로
            }
        }
    }
}