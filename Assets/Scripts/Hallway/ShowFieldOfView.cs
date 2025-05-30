using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShowFieldOfView : MonoBehaviour
{
    private float viewRadius;  // EnemyControl���� ������ ���̹Ƿ� private
    private float viewAngle;   // EnemyControl���� ������ ���̹Ƿ� private
    public int rayCount = 50;  // �þ� �޽��� �ػ� (���� ����)

    private Mesh mesh;
    private EnemyControl enemyControl; // �θ��� EnemyControl ����

    void Start()
    {
        // �þ� �޽��� Material ���� (����� ������)
        Material fieldOfViewMat = new Material(Shader.Find("Sprites/Default"));
        fieldOfViewMat.color = new Color(1f, 1f, 0f, 0.2f);
        GetComponent<MeshRenderer>().material = fieldOfViewMat;

        // �θ� ������Ʈ���� EnemyControl ������Ʈ ��������
        enemyControl = transform.parent.GetComponent<EnemyControl>();
        if (enemyControl == null)
        {
            Debug.LogError("ShowFieldOfView: Parent object does not have EnemyControl component!");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
            return;
        }

        // Mesh �ʱ�ȭ
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // �þ� ���� EnemyControl���� ������
        viewRadius = enemyControl.viewRadius;
        viewAngle = enemyControl.viewAngle;
    }

    void LateUpdate()
    {
        // ��Ÿ�ӿ� EnemyControl�� �þ� ������ ����� ��츦 ����Ͽ� ������Ʈ
        viewRadius = enemyControl.viewRadius;
        viewAngle = enemyControl.viewAngle;

        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        int stepCount = rayCount;
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>(); // �þ� �޽��� ��������� ������ ����Ʈ

        // Enemy�� ���� ��ġ�� �þ��� �߽� ����
        Vector2 origin = enemyControl.transform.position;
        Vector3 viewCenterDirection = enemyControl.transform.up; // Enemy�� ���� ����

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2f + stepAngleSize * i;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * viewCenterDirection;

            // �þ� �ð�ȭ�� ���� obstacleMask���� ����ĳ��Ʈ ����
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, enemyControl.obstacleMask);

            Vector3 hitPoint;
            if (hit.collider != null)
            {
                // ���̰� ��ֹ��� �ε����ٸ�, �浹 ������ ��������� ���
                hitPoint = hit.point;
                // Debug.DrawRay(origin, dir * hit.distance, Color.blue); // ����׿� (�ʿ��ϸ� Ȱ��ȭ)
            }
            else
            {
                // ���̰� ��ֹ��� �ε����� �ʾҴٸ�, viewRadius ������ ��������� ���
                hitPoint = (Vector3)origin + dir * viewRadius;
                // Debug.DrawRay(origin, dir * viewRadius, Color.green); // ����׿� (�ʿ��ϸ� Ȱ��ȭ)
            }

            // ���� ��ǥ�� hitPoint�� �� �þ� �޽� ������Ʈ�� ���� ��ǥ�� ��ȯ�Ͽ� �߰�
            viewPoints.Add(transform.InverseTransformPoint(hitPoint));
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // �þ��� �������� �� �ڽ� ������Ʈ�� ���� ���� (0,0,0)
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = viewPoints[i];

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // �� ������Ʈ�� ���� ��ġ�� ȸ���� �׻� 0���� �����Ͽ� �θ�(Enemy)�� ����ٴϰ� �մϴ�.
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}