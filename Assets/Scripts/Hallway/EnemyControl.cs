using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public float viewAngle = 45f;           // 시야 각도
    public float viewRadius = 5f;           // 시야 반경
    public float moveSpeed = 2f;
    public float rotateDuration = 0.5f;     // 회전 지속 시간

    public int rayCount = 30;               // 시야 레이 개수 (플레이어 감지용)

    public LayerMask targetMask;            // 플레이어 레이어
    public LayerMask obstacleMask;          // 벽 등 장애물 레이어

    private bool isRotating = false;
    private Vector2 moveDirection;

    public GameObject player;
    PlayerControl playerControl;

    void Start()
    {
        moveDirection = transform.up; // 적이 바라보는 방향으로 초기화 (Y축 양의 방향)
        player = GameObject.FindWithTag("Player").gameObject;
        playerControl = player.GetComponent<PlayerControl>();
    }

    void Update() // LateUpdate 대신 Update에서 레이캐스트 수행
    {
        // 플레이어 감지 로직
        Vector2 origin = transform.position;
        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, targetMask | obstacleMask);

            if (hit && ((1 << hit.collider.gameObject.layer) & targetMask) != 0)
            {
                Debug.Log("플레이어 감지됨!");
                if (playerControl.isHidden == false || (playerControl.isHidden == true && playerControl.isPlayerMoving == true))
                {
                    //게임 오버 로직
                    Debug.Log("게임 오버!");
                }
            }

            // 디버그용 레이 (색상 변경)
            Debug.DrawRay(origin, dir * viewRadius, Color.red);
        }

        if (!isRotating)
        {
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    // angle은 로컬 회전 기준. global=true일 경우 적의 현재 방향을 반영.
    // 이 함수는 플레이어 감지를 위한 레이캐스트 방향을 계산합니다.
    Vector2 DirFromAngle(float angleInDegrees)
    {
        // EnemyControl의 이동 및 회전 로직과 일관성 있게 transform.up을 기준으로 계산
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, Vector3.forward);
        return rotation * transform.up; // transform.up을 기준으로 각도를 벌립니다.
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        float targetAngle = transform.eulerAngles.z;

        switch (tag)
        {
            case "TurnRight":
                targetAngle += -90f; // Z축은 시계방향이 음수 (유니티 2D 기본)
                break;
            case "TurnLeft":
                targetAngle += 90f;
                break;
            case "TurnBack":
                targetAngle += 180f;
                break;
            default:
                return; // 회전 명령이 없는 경우 무시
        }

        if (!isRotating)
        {
            StartCoroutine(RotateTowards(targetAngle));
        }
    }

    IEnumerator RotateTowards(float targetZ)
    {
        isRotating = true;

        float elapsed = 0f;
        float startZ = transform.eulerAngles.z;
        float angleDiff = Mathf.DeltaAngle(startZ, targetZ);

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;
            float currentZ = startZ + angleDiff * t;
            transform.rotation = Quaternion.Euler(0f, 0f, currentZ);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, targetZ);

        moveDirection = transform.up; // 바라보는 방향(transform.up)을 기준으로 이동 방향 재설정

        isRotating = false;
    }
}