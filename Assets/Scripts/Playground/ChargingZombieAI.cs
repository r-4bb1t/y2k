using UnityEngine;

public class ChargingZombieAI : MonoBehaviour
{
    [Header("Detection & Charge Stats")]
    public float detectionRange = 10f;
    public float chargePreparationTime = 2f;
    public float chargeSpeed = 15f;
    public float chargeDistance = 7f;
    public float idleMoveSpeed = 2f;

    [Header("References")]
    public GameObject chargeDirectionIndicator;

    // 내부 변수
    private Transform player;
    private Rigidbody2D rb;
    private Collider2D myCollider; // 자신의 콜라이더 참조
    private SpriteRenderer indicatorRenderer;

    private enum State { Idle, PreparingCharge, Charging }
    private State currentState = State.Idle;

    private float prepareTimer;
    private Vector2 chargeDirection;
    private Vector2 chargeStartPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        myCollider = GetComponent<Collider2D>(); // 콜라이더 참조 가져오기

        if (rb == null || myCollider == null)
        {
            Debug.LogError("ChargingZombieAI: Rigidbody2D 또는 Collider2D가 없습니다!");
            enabled = false;
            return;
        }
        if (rb.gravityScale != 0f) rb.gravityScale = 0f;


        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("ChargingZombieAI: Player를 찾을 수 없습니다.");
        }

        if (chargeDirectionIndicator != null)
        {
            indicatorRenderer = chargeDirectionIndicator.GetComponent<SpriteRenderer>();
            chargeDirectionIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            if (rb != null) rb.velocity = Vector2.zero;
            currentState = State.Idle;
            if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
            if (myCollider != null && myCollider.isTrigger && currentState != State.Charging) // 안전장치: 혹시 모를 상황 대비
            {
                myCollider.isTrigger = false;
            }
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                if (myCollider.isTrigger) myCollider.isTrigger = false; // Idle 상태에서는 일반 콜라이더로

                if (distanceToPlayer <= detectionRange)
                {
                    PrepareCharge();
                }
                else
                {
                    float currentMoveSpeed;
                    ZombieAI basicAI = GetComponent<ZombieAI>();
                    if (basicAI != null && basicAI.enabled) currentMoveSpeed = basicAI.moveSpeed;
                    else currentMoveSpeed = this.idleMoveSpeed;

                    Vector2 direction = (player.position - transform.position).normalized;
                    rb.velocity = direction * currentMoveSpeed;

                    if (direction != Vector2.zero)
                    {
                        float offsetAngle = -90f;
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offsetAngle;
                        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    }
                    if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
                }
                break;

            case State.PreparingCharge:
                prepareTimer -= Time.deltaTime;
                Vector2 directionToPlayerWhilePreparing = (player.position - transform.position).normalized;

                if (chargeDirectionIndicator != null)
                {
                    chargeDirectionIndicator.SetActive(true);
                    chargeDirectionIndicator.transform.localScale = new Vector3(chargeDirectionIndicator.transform.localScale.x, chargeDistance, chargeDirectionIndicator.transform.localScale.z);
                    chargeDirectionIndicator.transform.position = (Vector2)transform.position + (directionToPlayerWhilePreparing * chargeDistance * 0.5f);
                    chargeDirectionIndicator.transform.up = directionToPlayerWhilePreparing;
                    if (indicatorRenderer != null)
                    {
                        float alpha = Mathf.Lerp(1f, 0.3f, prepareTimer / chargePreparationTime);
                        indicatorRenderer.color = new Color(indicatorRenderer.color.r, indicatorRenderer.color.g, indicatorRenderer.color.b, alpha);
                    }
                }

                if (prepareTimer <= 0f)
                {
                    StartCharge(directionToPlayerWhilePreparing);
                }
                break;

            case State.Charging:
                if (Vector2.Distance(chargeStartPosition, transform.position) >= chargeDistance)
                {
                    StopCharge();
                }
                break;
        }
    }

    void PrepareCharge()
    {
        currentState = State.PreparingCharge;
        prepareTimer = chargePreparationTime;
        rb.velocity = Vector2.zero;
        if (indicatorRenderer != null)
        {
            indicatorRenderer.color = new Color(indicatorRenderer.color.r, indicatorRenderer.color.g, indicatorRenderer.color.b, 0.3f);
        }
    }

    void StartCharge(Vector2 direction)
    {
        currentState = State.Charging;
        chargeDirection = direction.normalized;
        chargeStartPosition = transform.position;
        rb.velocity = chargeDirection * chargeSpeed;

        if (myCollider != null) myCollider.isTrigger = true; // ★ 돌진 시작 시 트리거로 변경

        if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
    }

    void StopCharge()
    {
        currentState = State.Idle;
        rb.velocity = Vector2.zero;

        if (myCollider != null) myCollider.isTrigger = false; // ★ 돌진 종료 시 다시 일반 콜라이더로 변경

        if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
    }

    // 돌진 중 (isTrigger = true일 때) 충돌 감지
    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (currentState != State.Charging) return; // 돌진 중일 때만 이 로직 실행

        // 플레이어와 충돌했는지 확인
        if (otherCollider.CompareTag("Player"))
        {
            Debug.Log(gameObject.name + " charged into Player (Trigger)!");
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null) gm.GameOver();
            // collision.gameObject.SetActive(false); // 플레이어 비활성화는 GameManager에서 처리하는 것이 나을 수 있음
            StopCharge(); // 돌진 멈춤
        }
        // 다른 좀비가 아닌 '단단한' 장애물(예: 벽)과 충돌했는지 확인
        // (otherCollider.isTrigger가 false인 경우 = 단단한 콜라이더)
        else if (!otherCollider.CompareTag("Zombie") && !otherCollider.isTrigger)
        {
            Debug.Log(gameObject.name + " collided with " + otherCollider.gameObject.name + " while charging (Trigger).");
            StopCharge(); // 돌진 멈춤
        }
        // 다른 좀비 (CompareTag("Zombie"))와 충돌한 경우는 아무것도 하지 않고 통과 (StopCharge() 호출 안 함)
    }

    // 돌진 중이 아닐 때 (isTrigger = false일 때) 또는 다른 경우의 물리적 충돌
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Charging) return; // 돌진 중에는 OnTriggerEnter2D가 주로 처리

        // Idle 상태에서 플레이어나 벽과 부딪혔을 때의 로직 (선택 사항)
        // 예를 들어, Idle 상태에서 플레이어에게 닿으면 데미지를 준다거나,
        // 벽에 부딪히면 방향을 바꾼다거나 하는 로직을 여기에 추가할 수 있습니다.
        // 지금은 특별한 로직 없음.
        // if (currentState == State.Idle && collision.gameObject.CompareTag("Player")) { /* 일반 공격 */ }
    }
}