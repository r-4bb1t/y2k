using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public float viewAngle = 90f;        // �þ� ����
    public float viewRadius = 5f;        // �þ� �ݰ�
    public float moveSpeed = 2f;
    public float rotateDuration = 0.5f; // ȸ�� ���� �ð�

    public int rayCount = 30;            // ���� ����

    private bool isRotating = false;

    public LayerMask targetMask;         // �÷��̾� ���̾�
    public LayerMask obstacleMask;       // �� �� ��ֹ� ���̾�
    private Vector2 moveDirection;

    void Start()
    {
        moveDirection = transform.up; // ���� �ٶ󺸴� �������� �ʱ�ȭ
    }

    void Update()
    {
        Vector2 origin = transform.position;
        float angleStep = viewAngle / rayCount;
        float startAngle = -viewAngle / 2f;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 dir = DirFromAngle(angle);  // ������ �Լ�
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, targetMask | obstacleMask);

            if (hit && ((1 << hit.collider.gameObject.layer) & targetMask) != 0)
            {
                Debug.Log("�÷��̾� ������!");
            }

            Debug.DrawRay(origin, dir * viewRadius, Color.yellow);
        }

        if (!isRotating)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    // angle�� ���� ȸ�� ����. global=true�� ��� ���� ���� ������ �ݿ�.
    Vector2 DirFromAngle(float angleInDegrees)
    {
        // ���� �ٶ󺸴� ����(transform.up)�� �������� ȸ��
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
            Vector2 dir = DirFromAngle(angle);  // ������ �Լ�
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * viewRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewRadius);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        // ������ Waypoint�� �±׿� ���� ���� ����
        Vector2 targetDir = moveDirection;
        float targetAngle = transform.eulerAngles.z;

        switch (tag)
        {
            case "Right":
                targetDir = Vector2.right;
                targetAngle = 0f;
                break;
            case "Left":
                targetDir = Vector2.left;
                targetAngle = 180f;
                break;
            case "Up":
                targetDir = Vector2.up;
                targetAngle = 90f;
                break;
            case "Down":
                targetDir = Vector2.down;
                targetAngle = -90f;
                break;
            default:
                return; // ��ȿ �±��� ��� ����
        }

        // ���� ȸ�� ���� �ƴ϶�� ȸ�� �ڷ�ƾ ����
        if (!isRotating)
        {
            StartCoroutine(RotateTowards(targetAngle, targetDir));
        }
    }

    IEnumerator RotateTowards(float targetZ, Vector2 newDirection)
    {
        isRotating = true;

        float elapsed = 0f;
        float startZ = transform.eulerAngles.z;

        // Unity�� Z�� ������ 0~360���̹Ƿ� ������ ���� ����
        float angleDiff = Mathf.DeltaAngle(startZ, targetZ);

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rotateDuration;

            float currentZ = startZ + angleDiff * t;
            transform.rotation = Quaternion.Euler(0f, 0f, currentZ);

            yield return null;
        }

        // ��Ȯ�� ����
        transform.rotation = Quaternion.Euler(0f, 0f, targetZ);
        moveDirection = newDirection;
        isRotating = false;
    }
}
