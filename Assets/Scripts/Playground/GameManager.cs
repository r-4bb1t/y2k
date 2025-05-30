using UnityEngine;
using UnityEngine.UI; // Unity UI 시스템 사용 (레거시 UI용, TextMeshPro는 아래)
using TMPro;          // TextMeshPro 사용
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Objects & Prefabs")]
    public GameObject baseZombiePrefab;        // 기본 좀비 프리팹
    public GameObject chargingZombiePrefab;    // 돌진 좀비 프리팹 (선택 사항)
    public List<Transform> spawnPoints;        // 좀비 스폰 위치들

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;          // 생존 시간 표시 UI Text
    public TextMeshProUGUI zombieCountText;    // 현재 좀비 수 표시 UI Text
    public GameObject gameOverPanel;           // 게임 오버 시 활성화할 UI 패널
    public GameObject gameWinPanel;            // 게임 승리 시 활성화할 UI 패널

    [Header("Game Settings")]
    public float spawnInterval = 10f;          // 좀비 스폰 간격 (초)
    public float survivalTimeGoal = 120f;      // 생존 목표 시간 (초)
    public float minSpawnDistanceToPlayer = 5f; // 플레이어로부터 최소 스폰 거리
    public float timeToSpawnChargingZombies = 60f; // 돌진 좀비 스폰 시작 시간
    [Range(0f, 1f)]
    public float chargingZombieSpawnChance = 0.2f; // 돌진 좀비 스폰 확률 (위 시간 이후)

    // 내부 상태 변수
    private float spawnTimer;
    private float currentSurvivalTime;
    private int currentZombieCount = 0;
    private bool isGameOver = false;
    private bool isGameWon = false;
    private Transform playerTransform;

    void Start()
    {
        // 플레이어 오브젝트 찾기
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("GameManager: Player를 찾을 수 없습니다. 플레이어 안전 스폰 기능이 제한될 수 있습니다.");
        }

        // 초기화
        spawnTimer = spawnInterval; // 첫 스폰은 바로 시작하도록 하려면 0f 또는 spawnInterval 값으로 조절
        currentSurvivalTime = 0f;
        currentZombieCount = 0; // 씬에 이미 좀비가 있다면 여기서 초기화 필요
        isGameOver = false;
        isGameWon = false;
        Time.timeScale = 1f; // 게임 시간 정상 속도

        // UI 패널 초기 비활성화
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        // UI 초기 업데이트
        UpdateTimerUI();
        UpdateZombieCountUI();
    }

    void Update()
    {
        if (isGameOver || isGameWon) return; // 게임이 끝났으면 업데이트 중지

        // 생존 시간 업데이트
        currentSurvivalTime += Time.deltaTime;
        UpdateTimerUI();

        // 생존 목표 시간 달성 체크
        if (currentSurvivalTime >= survivalTimeGoal)
        {
            GameWin();
            return; // 승리했으므로 더 이상 진행 안 함
        }

        // 좀비 스폰 타이머 업데이트
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnZombie();
            spawnTimer = spawnInterval; // 타이머 리셋
        }
    }

    void SpawnZombie()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("GameManager: 스폰 위치가 설정되지 않았습니다.");
            return;
        }
        if (baseZombiePrefab == null)
        {
             Debug.LogWarning("GameManager: 기본 좀비 프리팹이 설정되지 않았습니다.");
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
        
        // 안전한 스폰 포인트가 없으면 모든 스폰 포인트 사용 (플레이어가 모든 스폰 포인트에 너무 가까울 경우)
        if (availableSpawnPoints.Count == 0)
        {
            availableSpawnPoints.AddRange(spawnPoints);
        }
        
        if (availableSpawnPoints.Count == 0) // 그래도 스폰 포인트가 없으면 (매우 드문 경우)
        {
            Debug.LogWarning("GameManager: 사용 가능한 스폰 위치가 없습니다.");
            return;
        }


        // 랜덤한 스폰 지점 선택
        Transform selectedSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];

        // 스폰할 좀비 타입 결정
        GameObject zombieToSpawn = baseZombiePrefab;
        bool spawnChargingZombie = false;

        if (chargingZombiePrefab != null && currentSurvivalTime >= timeToSpawnChargingZombies)
        {
            if (Random.Range(0f, 1f) < chargingZombieSpawnChance)
            {
                zombieToSpawn = chargingZombiePrefab;
                spawnChargingZombie = true;
            }
        }

        // 좀비 생성
        GameObject spawnedZombie = Instantiate(zombieToSpawn, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        currentZombieCount++;
        UpdateZombieCountUI();

        if (spawnChargingZombie)
        {
            Debug.Log("돌진 좀비 스폰! 현재 좀비 수: " + currentZombieCount);
        }
        else
        {
            Debug.Log("기본 좀비 스폰! 현재 좀비 수: " + currentZombieCount);
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.FloorToInt(currentSurvivalTime).ToString() + "s";
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
        if (isGameOver || isGameWon) return; // 이미 게임 종료 상태면 중복 실행 방지

        isGameOver = true;
        Time.timeScale = 0f; // 시간 정지
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Debug.Log("게임 오버!");
    }

    public void GameWin()
    {
        if (isGameOver || isGameWon) return; // 이미 게임 종료 상태면 중복 실행 방지

        isGameWon = true;
        Time.timeScale = 0f; // 시간 정지
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }
        Debug.Log("게임 승리!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 게임 시간 다시 흐르도록 설정
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬 다시 로드
    }

    // 만약 좀비를 죽이는 기능이 있다면 이 함수를 호출하여 좀비 카운트 감소
    public void ZombieKilled()
    {
        currentZombieCount--;
        if (currentZombieCount < 0) currentZombieCount = 0; // 음수 방지
        UpdateZombieCountUI();
    }
}