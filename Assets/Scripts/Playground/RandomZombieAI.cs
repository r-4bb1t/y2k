using UnityEngine;

public class RandomZombieAI : MonoBehaviour
{
    public float moveSpeed = 1.5f;                // �̵� �ӵ�
    public float changeDirectionInterval = 3f;    // ������ �ٲٴ� �ֱ� (��)

    private Rigidbody2D rb;
    private Vector2 moveDirection;
    private float directionTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb.gravityScale != 0f) // Ȥ�� �� �߷� ����
        {
            rb.gravityScale = 0f;
        }
        PickNewDirection();
    }

    void Update()
    {
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            PickNewDirection();
        }
    }

    void FixedUpdate()
    {
        rb.velocity = moveDirection * moveSpeed;

        // �̵� �������� ȸ�� (���� ����, ��������Ʈ�� ������ �⺻���� �� �� -90�� ������)
        if (moveDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void PickNewDirection()
    {
        // -1.0 ~ 1.0 ������ ������ x, y ���� ���� ����ȭ�� ���͸� ���� (���� ���� �� ���� ����)
        moveDirection = Random.insideUnitCircle.normalized;
        directionTimer = changeDirectionInterval; // Ÿ�̸� ����
        // Debug.Log(gameObject.name + " is moving in direction: " + moveDirection);
    }

    // ���̳� �ٸ� ��ֹ��� �ε����� �� ������ �ٲٴ� ���� (���� ����)
    void OnCollisionEnter2D(Collision2D collision)
    {
        // �÷��̾ �ٸ� ���� �ƴ� �Ͱ� �ε����� �� (��: ��)
        if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Zombie"))
        {
            // Debug.Log(gameObject.name + " collided with " + collision.gameObject.name + ", changing direction.");
            PickNewDirection(); // �� ���� ����
        }
    }
}