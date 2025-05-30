using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // �÷��̾ Ʈ���ſ� ��Ҵ��� Ȯ��
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered ExitTrigger.");
            // GameManager�� ã�Ƽ� PlayerReachedExit �Լ� ȣ��
            GameManager gm = FindObjectOfType<GameManager>();
            if (gm != null)
            {
                // GameManager�� isGameWon ���µ� �Բ� Ȯ���Ͽ�, ���� ������ ���� ������ ���� Ż�� ó��
                if (gm.isGameWon)
                {
                    gm.PlayerReachedExit();
                }
            }
        }
    }
}