using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    public float moveSpeed = 2f;
    private Transform player; // 플레이어의 위치를 추적

    void Start()
    {
        // "Player" 태그를 가진 게임 오브젝트를 찾아서 player 변수에 할당
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없습니다. Player 태그를 확인하세요.");
            enabled = false; // 플레이어가 없으면 AI 비활성화
        }
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeInHierarchy) // 플레이어가 없거나 비활성화 상태면 추적 중지
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero; // 3D: GetComponent<Rigidbody>().velocity = Vector3.zero;
            return;
        }

        // 플레이어 방향으로 이동
        Vector2 direction = (player.position - transform.position).normalized; // 3D: Vector3
        // transform.position += (Vector3)direction * moveSpeed * Time.deltaTime; // Rigidbody를 안쓸 때
        GetComponent<Rigidbody2D>().velocity = direction * moveSpeed; // 3D: GetComponent<Rigidbody>().velocity

        // (선택 사항) 플레이어를 바라보도록 회전
         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 2D Sprite 기준
         transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}