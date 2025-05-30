using UnityEngine;
using System.Collections; // IEnumerator 등을 사용한다면 필요할 수 있음 (현재는 불필요)

public class ChargingZombieAI : MonoBehaviour
{
    [Header("Detection & Charge Stats")]
    public float detectionRange = 10f;          // 플레이어 감지 범위
    public float chargePreparationTime = 2f;  // 돌진 준비 시간
    public float chargeSpeed = 15f;           // 돌진 속도
    public float chargeDistance = 7f;           // 돌진 거리 (이전엔 5f였으나, 예시로 늘려봄)
    public float idleMoveSpeed = 2f;          // 평상시 이동 속도 (ZombieAI가 없을 경우 사용될 기본값)

    [Header("References")]
    public GameObject chargeDirectionIndicator; // 돌진 방향 및 거리 표시기 (Inspector에서 연결)

    // 내부 변수
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer indicatorRenderer; // 표시기 스프라이트 렌더러 (알파값 조절용)

    private enum State
    {
        Idle,             // 평상시 (플레이어 추적 또는 대기)
        PreparingCharge,  // 돌진 준비
        Charging          // 돌진 중
    }
    private State currentState = State.Idle;

    private float prepareTimer;         // 돌진 준비 타이머
    private Vector2 chargeDirection;    // 돌진 방향
    private Vector2 chargeStartPosition;// 돌진 시작 위치

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("ChargingZombieAI: Rigidbody2D가 없습니다!");
            enabled = false; // Rigidbody2D 없이는 작동 불가
            return;
        }
        // rb.gravityScale = 0; // 이미 Rigidbody2D 설정에서 하셨겠지만, 확인차원

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("ChargingZombieAI: Player를 찾을 수 없습니다. 좀비가 제대로 작동하지 않을 수 있습니다.");
            // player가 없어도 enabled = false;를 하지는 않음. 플레이어가 나중에 나타날 수도 있으므로.
        }

        if (chargeDirectionIndicator != null)
        {
            indicatorRenderer = chargeDirectionIndicator.GetComponent<SpriteRenderer>();
            chargeDirectionIndicator.SetActive(false); // 처음에는 표시기 비활성화
        }
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            // 플레이어가 없거나 비활성화 상태면 할 수 있는게 별로 없음
            if (rb != null) rb.velocity = Vector2.zero;
            currentState = State.Idle;
            if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                if (distanceToPlayer <= detectionRange)
                {
                    PrepareCharge();
                }
                else
                {
                    // 평소에는 기본 좀비처럼 플레이어를 향해 이동
                    float currentMoveSpeed;
                    ZombieAI basicAI = GetComponent<ZombieAI>(); // 같은 GameObject에 ZombieAI 스크립트가 있는지 확인

                    if (basicAI != null && basicAI.enabled) // ZombieAI가 있고 활성화 상태라면
                    {
                        currentMoveSpeed = basicAI.moveSpeed; // ZombieAI의 moveSpeed 사용
                    }
                    else
                    {
                        currentMoveSpeed = this.idleMoveSpeed; // 없다면 이 스크립트의 idleMoveSpeed 사용
                    }

                    Vector2 direction = (player.position - transform.position).normalized;
                    rb.velocity = direction * currentMoveSpeed; // 결정된 속도로 이동

                    // 평상시 이동 중에는 회전 로직도 추가하면 좋음 (기본 ZombieAI의 회전 로직 참고)
                    if (direction != Vector2.zero)
                    {
                        float offsetAngle = -90f; // 스프라이트 기본 방향에 따라 조절
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + offsetAngle;
                        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    }

                    if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
                }
                break;

            // ChargingZombieAI.cs의 Update() 함수 내 State.PreparingCharge 부분

case State.PreparingCharge:
    prepareTimer -= Time.deltaTime;
    Vector2 directionToPlayerWhilePreparing = (player.position - transform.position).normalized;

    if (chargeDirectionIndicator != null)
    {
        chargeDirectionIndicator.SetActive(true);

        // 1. 표시기의 길이(Y 스케일)를 chargeDistance로 설정
        //    (표시기 GameObject의 초기 X, Z 스케일은 유지)
        chargeDirectionIndicator.transform.localScale = new Vector3(
            chargeDirectionIndicator.transform.localScale.x, // 초기 설정한 X 스케일(두께) 유지
            chargeDistance,                                  // Y 스케일을 돌진 거리로
            chargeDirectionIndicator.transform.localScale.z  // 초기 설정한 Z 스케일 유지
        );

        // 2. 표시기의 위치 설정: 좀비 위치에서 (조준방향 * 돌진거리/2) 만큼 떨어진 곳 (표시기 길이의 중간점)
        //    이렇게 하면 표시기의 중심이 돌진 경로의 중간에 위치하게 됩니다.
        chargeDirectionIndicator.transform.position = (Vector2)transform.position + (directionToPlayerWhilePreparing * chargeDistance * 0.5f);
        
        // 3. 표시기 회전: 조준 방향으로 (Y축이 앞을 향하도록)
        chargeDirectionIndicator.transform.up = directionToPlayerWhilePreparing;

        // 4. (선택) 표시기 색상/알파값 등으로 준비 상태 표현
        if(indicatorRenderer != null)
        {
            // 예: 준비 시간이 다 되어갈수록 불투명하게 (또는 특정 색으로 깜빡이게 등)
            float alpha = Mathf.Lerp(1f, 0.3f, prepareTimer / chargePreparationTime); // 점점 진해지는 효과
            indicatorRenderer.color = new Color(indicatorRenderer.color.r, indicatorRenderer.color.g, indicatorRenderer.color.b, alpha);
        }
    }

    if (prepareTimer <= 0f)
    {
        StartCharge(directionToPlayerWhilePreparing);
    }
    break;

            case State.Charging:
                // rb.velocity는 StartCharge에서 이미 설정됨
                // 돌진 거리만큼 이동했는지 체크
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
        rb.velocity = Vector2.zero; // 돌진 준비 중에는 멈춤
        Debug.Log(gameObject.name + " is preparing to charge.");

        // 표시기 초기 알파값 등 설정
        if(indicatorRenderer != null)
        {
            indicatorRenderer.color = new Color(indicatorRenderer.color.r, indicatorRenderer.color.g, indicatorRenderer.color.b, 0.3f);
        }
    }

    void StartCharge(Vector2 direction)
    {
        currentState = State.Charging;
        chargeDirection = direction.normalized; // 만약을 위해 정규화
        chargeStartPosition = transform.position;
        rb.velocity = chargeDirection * chargeSpeed; // 설정된 방향과 속도로 돌진
        Debug.Log(gameObject.name + " starts charging in direction: " + chargeDirection);

        if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false); // 돌진 시작 시 표시기 숨김
    }

    void StopCharge()
    {
        currentState = State.Idle;
        rb.velocity = Vector2.zero; // 돌진 완료 후 멈춤
        Debug.Log(gameObject.name + " finished charge.");
        if (chargeDirectionIndicator != null) chargeDirectionIndicator.SetActive(false);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 돌진 중에만 플레이어와 충돌 시 게임 오버 처리
        if (currentState == State.Charging && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log(gameObject.name + " charged into Player!");
            // 플레이어에게 데미지를 주거나 게임 오버 처리
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                gm.GameOver(); // GameManager의 GameOver 함수 호출
            }
            // 플레이어 오브젝트를 비활성화 하거나 파괴하는 로직은 GameManager 또는 Player 스크립트에서 처리하는 것이 더 일반적일 수 있음
            // collision.gameObject.SetActive(false); // 예시: 플레이어 비활성화

            StopCharge(); // 플레이어와 충돌 후 돌진 멈춤
        }
        // 돌진 중에 벽이나 다른 장애물과 충돌했을 때
        else if (currentState == State.Charging && !collision.gameObject.CompareTag("Zombie")) // 다른 좀비와는 충돌 무시
        {
            Debug.Log(gameObject.name + " collided with " + collision.gameObject.name + " while charging.");
            StopCharge(); // 장애물과 충돌 후 돌진 멈춤
        }
    }
}