using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public float viewAngle = 90f;        // 시야 각도
    public float viewRadius = 5f;        // 시야 반경
    public int rayCount = 30;            // 레이 개수
    public LayerMask targetMask;         // 플레이어 레이어
    public LayerMask obstacleMask;       // 벽 등 장애물 레이어

    void Update()
    {
        Vector2 origin = transform.position;
        float angleStep = viewAngle / rayCount;
        float startAngle = transform.eulerAngles.z - viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle, true);
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, targetMask | obstacleMask);

            if (hit)
            {
                if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                {
                    Debug.Log("플레이어 감지됨!");
                }
            }

            // 시각화용
            Debug.DrawRay(origin, dir * viewRadius, Color.yellow);
        }
    }

    // angle은 로컬 회전 기준. global=true일 경우 적의 현재 방향을 반영.
    Vector2 DirFromAngle(float angleInDegrees, bool global)
    {
        if (!global)
            angleInDegrees += transform.eulerAngles.z;

        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return; // 실행 중이 아닐 땐 무시

        float angleStep = viewAngle / rayCount;
        float startAngle = transform.eulerAngles.z - viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle, true);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * viewRadius);
        }

        // 반경 원 그리기
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
