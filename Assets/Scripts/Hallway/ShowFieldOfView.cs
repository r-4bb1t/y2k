using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShowFieldOfView : MonoBehaviour
{
    private float viewRadius;  // EnemyControl에서 가져올 것이므로 private
    private float viewAngle;   // EnemyControl에서 가져올 것이므로 private
    public int rayCount = 50;  // 시야 메쉬의 해상도 (레이 개수)

    private Mesh mesh;
    private EnemyControl enemyControl; // 부모의 EnemyControl 참조

    void Start()
    {
        // 시야 메쉬의 Material 설정 (노란색 반투명)
        Material fieldOfViewMat = new Material(Shader.Find("Sprites/Default"));
        fieldOfViewMat.color = new Color(1f, 1f, 0f, 0.2f);
        GetComponent<MeshRenderer>().material = fieldOfViewMat;

        // 부모 오브젝트에서 EnemyControl 컴포넌트 가져오기
        enemyControl = transform.parent.GetComponent<EnemyControl>();
        if (enemyControl == null)
        {
            Debug.LogError("ShowFieldOfView: Parent object does not have EnemyControl component!");
            enabled = false; // 스크립트 비활성화
            return;
        }

        // Mesh 초기화
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        // 시야 값은 EnemyControl에서 가져옴
        viewRadius = enemyControl.viewRadius;
        viewAngle = enemyControl.viewAngle;
    }

    void LateUpdate()
    {
        // 런타임에 EnemyControl의 시야 설정이 변경될 경우를 대비하여 업데이트
        viewRadius = enemyControl.viewRadius;
        viewAngle = enemyControl.viewAngle;

        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        int stepCount = rayCount;
        float stepAngleSize = viewAngle / stepCount;

        List<Vector3> viewPoints = new List<Vector3>(); // 시야 메쉬의 경계점들을 저장할 리스트

        // Enemy의 현재 위치와 시야의 중심 방향
        Vector2 origin = enemyControl.transform.position;
        Vector3 viewCenterDirection = enemyControl.transform.up; // Enemy의 정면 방향

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2f + stepAngleSize * i;
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * viewCenterDirection;

            // 시야 시각화를 위해 obstacleMask에만 레이캐스트 수행
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, enemyControl.obstacleMask);

            Vector3 hitPoint;
            if (hit.collider != null)
            {
                // 레이가 장애물에 부딪혔다면, 충돌 지점을 경계점으로 사용
                hitPoint = hit.point;
                // Debug.DrawRay(origin, dir * hit.distance, Color.blue); // 디버그용 (필요하면 활성화)
            }
            else
            {
                // 레이가 장애물에 부딪히지 않았다면, viewRadius 지점을 경계점으로 사용
                hitPoint = (Vector3)origin + dir * viewRadius;
                // Debug.DrawRay(origin, dir * viewRadius, Color.green); // 디버그용 (필요하면 활성화)
            }

            // 월드 좌표인 hitPoint를 이 시야 메쉬 오브젝트의 로컬 좌표로 변환하여 추가
            viewPoints.Add(transform.InverseTransformPoint(hitPoint));
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // 시야의 시작점은 이 자식 오브젝트의 로컬 원점 (0,0,0)
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

        // 이 오브젝트의 로컬 위치와 회전을 항상 0으로 유지하여 부모(Enemy)를 따라다니게 합니다.
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}