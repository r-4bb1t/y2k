using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하려면 추가
using System.Collections.Generic; // List를 사용하려면 추가
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가

public class GameManager : MonoBehaviour
{
    public GameObject zombiePrefab;         // 좀비 프리팹을 Inspector에서 연결
    public List<Transform> spawnPoints;     // 스폰 지점들을 Inspector에서 연결
    public float spawnInterval = 10f;       // 좀비 스폰 간격 (10초)
    private float spawnTimer;               // 스폰 타이머

    public float survivalTimeGoal = 120f;   // 생존 목표 시간 (예: 2분)
    private float currentSurvivalTime;      // 현재 생존 시간
    private bool isGameOver = false;
    private bool isGameWon = false;

    public Text timerText;                  // 생존 시간을 표시할 UI Text (Inspector에서 연결)
    public Text zombieCountText;            // 현재 좀비 수를 표시할 UI Text (선택 사항)
    public GameObject gameOverPanel;        // 게임 오버 시 활성화할 UI 패널 (Inspector에서 연결)
    public GameObject gameWinPanel;         // 게임 승리 시 활성화할 UI 패널 (Inspector에서 연결)

    private int currentZombieCount = 0;

    void Start()
    {
        spawnTimer = spawnInterval; // 첫 스폰을 위해 타이머 초기화
        currentSurvivalTime = 0f;
        Time.timeScale = 1f; // 게임 시작 시 시간 흐름 정상화

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (gameWinPanel) gameWinPanel.SetActive(false);

        UpdateTimerUI();
        UpdateZombieCountUI();
    }

    void Update()
    {
        if (isGameOver || isGameWon) return; // 게임이 끝났으면 업데이트 중지

        // 생존 시간 업데이트
        currentSurvivalTime += Time.deltaTime;
        UpdateTimerUI();

        // 생존 목표 시간 달성 시 게임 승리
        if (currentSurvivalTime >= survivalTimeGoal)
        {
            GameWin();
            return;
        }

        // 좀비 스폰 타이머
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnZombie();
            spawnTimer = spawnInterval; // 타이머 리셋
        }
    }

    void SpawnZombie()
    {
        if (zombiePrefab == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("좀비 프리팹 또는 스폰 지점이 설정되지 않았습니다.");
            return;
        }

        // 랜덤한 스폰 지점 선택
        int spawnPointIndex = Random.Range(0, spawnPoints.Count);
        Transform selectedSpawnPoint = spawnPoints[spawnPointIndex];

        // 좀비 생성
        Instantiate(zombiePrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        currentZombieCount++;
        UpdateZombieCountUI();
        Debug.Log("좀비 스폰! 현재 좀비 수: " + currentZombieCount);
    }

    public void ZombieDefeated() // 좀비가 (만약 공격 등으로) 죽었을 때 호출될 함수 (지금은 피하기만 하므로 사용 안함)
    {
        currentZombieCount--;
        UpdateZombieCountUI();
    }


    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 남은 시간 표시 또는 경과 시간 표시 (선택)
            // float timeLeft = survivalTimeGoal - currentSurvivalTime;
            // timerText.text = "Time Left: " + Mathf.Max(0, timeLeft).ToString("F1");
            timerText.text = "Survived: " + currentSurvivalTime.ToString("F1") + "s";
        }
    }

    void UpdateZombieCountUI()
    {
         if (zombieCountText != null)
         {
             zombieCountText.text = "Zombies: " + currentZombieCount;
         }
    }

    public void GameOver()
    {
        if (isGameOver || isGameWon) return; // 이미 게임이 끝났으면 중복 실행 방지

        isGameOver = true;
        Debug.Log("게임 오버!");
        Time.timeScale = 0f; // 시간 정지
        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    public void GameWin()
    {
        if (isGameOver || isGameWon) return;

        isGameWon = true;
        Debug.Log("게임 승리!");
        Time.timeScale = 0f; // 시간 정지
        if (gameWinPanel) gameWinPanel.SetActive(true);
    }

    // UI 버튼 등에서 호출할 재시작 함수
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬 다시 로드
    }
}