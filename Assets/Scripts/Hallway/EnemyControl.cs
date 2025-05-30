using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    public float viewAngle = 45f;           // �þ� ����
    public float viewRadius = 5f;           // �þ� �ݰ�
    public float moveSpeed = 2f;
    public float rotateDuration = 0.5f;     // ȸ�� ���� �ð�

    public int rayCount = 30;               // �þ� ���� ���� (�÷��̾� ������)

    public LayerMask targetMask;            // �÷��̾� ���̾�
    public LayerMask obstacleMask;          // �� �� ��ֹ� ���̾�

    private bool isRotating = false;
    private Vector2 moveDirection;

    public GameObject player;
    PlayerControl playerControl;

    void Start()
    {
        moveDirection = transform.up; // ���� �ٶ󺸴� �������� �ʱ�ȭ (Y�� ���� ����)
        player = GameObject.FindWithTag("Player").gameObject;
        playerControl = player.GetComponent<PlayerControl>();
    }

    void Update() // LateUpdate ��� Update���� ����ĳ��Ʈ ����
    {
        // �÷��̾� ���� ����
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
                Debug.Log("�÷��̾� ������!");
                if (playerControl.isHidden == false || (playerControl.isHidden == true && playerControl.isPlayerMoving == true))
                {
                    //���� ���� ����
                    Debug.Log("���� ����!");
                }
            }

            // ����׿� ���� (���� ����)
            Debug.DrawRay(origin, dir * viewRadius, Color.red);
        }

        if (!isRotating)
        {
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    // angle�� ���� ȸ�� ����. global=true�� ��� ���� ���� ������ �ݿ�.
    // �� �Լ��� �÷��̾� ������ ���� ����ĳ��Ʈ ������ ����մϴ�.
    Vector2 DirFromAngle(float angleInDegrees)
    {
        // EnemyControl�� �̵� �� ȸ�� ������ �ϰ��� �ְ� transform.up�� �������� ���
        Quaternion rotation = Quaternion.AngleAxis(angleInDegrees, Vector3.forward);
        return rotation * transform.up; // transform.up�� �������� ������ �����ϴ�.
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.tag;

        float targetAngle = transform.eulerAngles.z;

        switch (tag)
        {
            case "TurnRight":
                targetAngle += -90f; // Z���� �ð������ ���� (����Ƽ 2D �⺻)
                break;
            case "TurnLeft":
                targetAngle += 90f;
                break;
            case "TurnBack":
                targetAngle += 180f;
                break;
            default:
                return; // ȸ�� ����� ���� ��� ����
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

        moveDirection = transform.up; // �ٶ󺸴� ����(transform.up)�� �������� �̵� ���� �缳��

        isRotating = false;
    }
}