using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public float viewAngle = 90f;        // �þ� ����
    public float viewRadius = 5f;        // �þ� �ݰ�
    public int rayCount = 30;            // ���� ����
    public LayerMask targetMask;         // �÷��̾� ���̾�
    public LayerMask obstacleMask;       // �� �� ��ֹ� ���̾�

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
                    Debug.Log("�÷��̾� ������!");
                }
            }

            // �ð�ȭ��
            Debug.DrawRay(origin, dir * viewRadius, Color.yellow);
        }
    }

    // angle�� ���� ȸ�� ����. global=true�� ��� ���� ���� ������ �ݿ�.
    Vector2 DirFromAngle(float angleInDegrees, bool global)
    {
        if (!global)
            angleInDegrees += transform.eulerAngles.z;

        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return; // ���� ���� �ƴ� �� ����

        float angleStep = viewAngle / rayCount;
        float startAngle = transform.eulerAngles.z - viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle, true);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * viewRadius);
        }

        // �ݰ� �� �׸���
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }
}
