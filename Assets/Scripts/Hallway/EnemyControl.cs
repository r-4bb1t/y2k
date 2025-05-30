using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public float viewAngle = 90f;        // 시야 각도
    public float viewRadius = 5f;        // 시야 반경
    public float moveSpeed = 2f;
    public float rotateDuration = 0.5f; // 회전 지속 시간

    public int rayCount = 30;            // 레이 개수

    private bool isRotating = false;

    public LayerMask targetMask;         // 플레이어 레이어
    public LayerMask obstacleMask;       // 벽 등 장애물 레이어
    private Vector2 moveDirection;

    void Start()
    {
        moveDirection = transform.up; // 적이 바라보는 방향으로 초기화
    }

    void Update()
    {
        Vector2 origin = transform.position;
        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle);  // 수정된 함수
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, targetMask | obstacleMask);

            if (hit && ((1 << hit.collider.gameObject.layer) & targetMask) != 0)
            {
                Debug.Log("플레이어 감지됨!");
            }

            Debug.DrawRay(origin, dir * viewRadius, Color.yellow);
        }

        if (!isRotating)
        {
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    // angle은 로컬 회전 기준. global=true일 경우 적의 현재 방향을 반영.
    Vector2 DirFromAngle(float angleInDegrees)
    {
        // 현재 바라보는 방향(transform.up)을 기준으로 회전
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, Vector3.forward);
        return rotation * transform.up;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle);  // 수정된 함수
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * viewRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        float targetAngle = transform.eulerAngles.z;

        switch (tag)
        {
            case "TurnRight":
                targetAngle += -90f; // Z축은 시계방향이 음수
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

        // 회전 중이 아니면 회전 시작
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

        //바라보는 방향(transform.up)을 기준으로 이동 방향 재설정
        moveDirection = transform.up;

        isRotating = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + moveDirection.normalized);
    }
}
