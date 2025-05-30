// CameraFollow.cs (선택 사항)
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 플레이어 Transform을 Inspector에서 연결
    public float smoothSpeed = 0.125f;
    public Vector3 offset; // 카메라와 플레이어 사이의 거리 오프셋

    void LateUpdate() // 플레이어 이동이 끝난 후 카메라 이동
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            // Z축 위치는 카메라의 원래 Z축 위치를 유지 (2D에서는 보통 -10)
            desiredPosition.z = transform.position.z;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}