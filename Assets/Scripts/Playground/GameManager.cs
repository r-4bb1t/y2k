using UnityEngine;
using UnityEngine.UI; // TextMeshPro를 주로 사용하므로 이 using은 필수는 아닐 수 있습니다.
using TMPro;          // TextMeshPro 사용을 위해 필요합니다.
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필요합니다.

public class GameManager : MonoBehaviour
{
    [Header("Game Objects & Prefabs")]
    public GameObject baseZombiePrefab;        // 기본 좀비 프리팹
    public GameObject chargingZombiePrefab;    // 돌진 좀비 프리팹 (선택 사항)
    public List<Transform> spawnPoints;        // 좀비 스폰 위치들

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;          // 생존 시간(카운트다운) 표시 UI Text
    public GameObject gameOverPanel;           // 게임 오버 시 활성화할 UI 패널
    public GameObject gameWinPanel;            // 게임 승리 시 활성화할 UI 패널

    [Header("Game Settings")]
    public float spawnInterval = 10f;          // 좀비 스폰 간격 (초)
    public float survivalTimeGoal = 60f;       // 생존 목표 시간 (초) -> 60초에서 카운트다운 시작
    public float minSpawnDistanceToPlayer = 5f; // 플레이어로부터 최소 스폰 거리
    public float timeToSpawnChargingZombies = 30f; // 돌진 좀비 스폰 시작까지 필요한 생존 시간 (예: 30초 생존 후)
    [Range(0f, 1f)]
    public float chargingZombieSpawnChance = 0.1f; // 위 시간 이후 돌진 좀비 스폰 확률

    // 내부 상태 변수
    private float spawnTimer;
    private float currentSurvivalTime;         // 실제로 게임이 진행된 시간 (0부터 증가)
    private bool isGameOver = false;
    private bool isGameWon = false;
    private Transform playerTransform;

    void Start()
    {
        // 플레이어 오브젝트 찾기 (태그로)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("GameManager: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다. 플레이어 안전 스폰 기능이 제한될 수 있습니다.");
        }

        // 변수 초기화
        spawnTimer = spawnInterval; // 첫 스폰까지의 시간
        currentSurvivalTime = 0f;   // 생존 시간은 0부터 시작해서 카운트 업
        isGameOver = false;
        isGameWon = false;
        Time.timeScale = 1f; // 게임 시간 정상 속도로 설정 (재시작 시 중요)

        // UI 패널 초기에는 비활성화
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        // UI 초기 업데이트 (타이머 등)
        UpdateTimerUI();
    }

    void Update()
    {
        // 게임 오버 또는 게임 승리 상태일 때 스페이스 바 입력 감지
        if (isGameOver || isGameWon)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
            return; // 게임이 끝났으므로 아래 로직은 실행하지 않음
        }

        // 게임 진행 중 로직
        currentSurvivalTime += Time.deltaTime; // 실제 생존 시간 증가
        UpdateTimerUI(); // 타이머 UI 업데이트 (카운트다운 표시)

        // 생존 목표 시간 달성 (승리 조건)
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
            spawnTimer = spawnInterval; // 스폰 간격으로 타이머 리셋
        }
    }

    void SpawnZombie()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("GameManager: 스폰 위치(spawnPoints)가 설정되지 않았습니다.");
            return;
        }
        if (baseZombiePrefab == null)
        {
            Debug.LogWarning("GameManager: 기본 좀비 프리팹(baseZombiePrefab)이 설정되지 않았습니다.");
            return;
        }

        // 플레이어로부터 안전한 스폰 위치 필터링
        List<Transform> availableSpawnPoints = new List<Transform>();
        if (playerTransform != null)
        {
            foreach (Transform sp in spawnPoints)
            {
                if (Vector2.Distance(sp.position, playerTransform.position) >= minSpawnDistanceToPlayer)
                {
                    availableSpawnPoints.Add(sp);
                }
            }
        }

        // 안전한 스폰 포인트가 없으면 (플레이어가 모든 스폰 포인트에 너무 가깝거나, 플레이어 참조가 없는 경우) 모든 스폰 포인트 사용
        if (availableSpawnPoints.Count == 0)
        {
            availableSpawnPoints.AddRange(spawnPoints);
        }

        // 그래도 스폰할 위치가 없으면 리턴 (매우 드문 경우)
        if (availableSpawnPoints.Count == 0)
        {
            Debug.LogWarning("GameManager: 사용 가능한 스폰 위치가 없습니다.");
            return;
        }

        // 필터링된 스폰 위치 중 랜덤 선택
        Transform selectedSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

        // 스폰할 좀비 타입 결정
        GameObject zombieToSpawn = baseZombiePrefab;
        bool spawnedChargingZombie = false; // 디버그 로그용

        // 돌진 좀비 스폰 조건 확인 (돌진 좀비 프리팹이 있고, 특정 생존 시간을 넘겼고, 확률에 당첨되면)
        if (chargingZombiePrefab != null && currentSurvivalTime >= timeToSpawnChargingZombies)
        {
            if (Random.Range(0f, 1f) < chargingZombieSpawnChance)
            {
                zombieToSpawn = chargingZombiePrefab;
                spawnedChargingZombie = true;
            }
        }

        // 좀비 생성
        Instantiate(zombieToSpawn, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        // Debug.Log((spawnedChargingZombie ? "돌진" : "기본") + " 좀비가 " + selectedSpawnPoint.name + " 위치에 스폰되었습니다.");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // 남은 시간 계산 (survivalTimeGoal에서 currentSurvivalTime을 빼서 카운트다운)
            float timeLeft = survivalTimeGoal - currentSurvivalTime;

            // 화면에 표시될 시간 (0 미만으로 내려가지 않도록 하고, 소수점 올림하여 정수로)
            int displayTime = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));

            timerText.text = "Time: " + displayTime.ToString() + "s";
        }
    }

    public void GameOver()
    {
        if (isGameOver || isGameWon) return; // 중복 호출 방지

        isGameOver = true;
        Time.timeScale = 0f; // 게임 시간 정지
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true); // 게임 오버 패널 활성화
        }
        Debug.Log("게임 오버!");
    }

    public void GameWin()
    {
        if (isGameOver || isGameWon) return; // 중복 호출 방지

        isGameWon = true;
        Time.timeScale = 0f; // 게임 시간 정지
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true); // 게임 승리 패널 활성화
        }
        Debug.Log("게임 승리! (1분 생존)");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 게임 시간 정상화 (매우 중요!)
        // 현재 활성화된 씬(Scene)의 이름을 가져와서 해당 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}