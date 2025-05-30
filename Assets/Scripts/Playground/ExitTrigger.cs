using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어가 트리거에 닿았는지 확인
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered ExitTrigger.");
            // GameManager를 찾아서 PlayerReachedExit 함수 호출
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                // GameManager의 isGameWon 상태도 함께 확인하여, 문이 실제로 열린 상태일 때만 탈출 처리
                if (gm.isGameWon)
                {
                    gm.PlayerReachedExit();
                }
            }
        }
    }
}