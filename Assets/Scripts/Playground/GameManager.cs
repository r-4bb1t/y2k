using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game Objects & Prefabs")]
    public GameObject baseZombiePrefab;
    public GameObject chargingZombiePrefab;
    public GameObject randomZombiePrefab;      // 새로 추가

    public List<Transform> spawnPoints;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("Game Settings")]
    public float spawnInterval = 10f;
    public float survivalTimeGoal = 60f;
    public float minSpawnDistanceToPlayer = 5f;

    [Header("Charging Zombie Settings")]
    public float timeToSpawnChargingZombies = 30f; // 이 값을 조절하여 등장 시기 변경
    [Range(0f, 1f)]
    public float chargingZombieSpawnChance = 0.1f; // 이 값을 조절하여 등장 확률 변경

    [Header("Random Zombie Settings")] // 새로 추가
    public float timeToSpawnRandomZombies = 15f;
    [Range(0f, 1f)]
    public float randomZombieSpawnChance = 0.3f;

    // 내부 상태 변수
    private float spawnTimer;
    private float currentSurvivalTime;
    private bool isGameOver = false;
    private bool isGameWon = false;
    private Transform playerTransform;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("GameManager: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        spawnTimer = spawnInterval;
        currentSurvivalTime = 0f;
        isGameOver = false;
        isGameWon = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameOver || isGameWon)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
            return;
        }

        currentSurvivalTime += Time.deltaTime;
        UpdateTimerUI();

        if (currentSurvivalTime >= survivalTimeGoal)
        {
            GameWin();
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnZombie();
            spawnTimer = spawnInterval;
        }
    }

    void SpawnZombie()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("GameManager: 스폰 위치(spawnPoints)가 설정되지 않았습니다.");
            return;
        }
        // 기본 좀비 프리팹은 필수
        if (baseZombiePrefab == null)
        {
            Debug.LogWarning("GameManager: 기본 좀비 프리팹(baseZombiePrefab)이 설정되지 않았습니다.");
            return;
        }


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
        if (availableSpawnPoints.Count == 0) availableSpawnPoints.AddRange(spawnPoints);
        if (availableSpawnPoints.Count == 0) return;

        Transform selectedSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
        GameObject zombieToSpawn = baseZombiePrefab; // 기본적으로는 일반 좀비
        string spawnedType = "기본";

        // 어떤 좀비를 스폰할지 결정 (우선순위: 돌진 > 무작위 > 기본)
        // 1. 돌진 좀비 스폰 조건 확인
        if (chargingZombiePrefab != null && currentSurvivalTime >= timeToSpawnChargingZombies && Random.Range(0f, 1f) < chargingZombieSpawnChance)
        {
            zombieToSpawn = chargingZombiePrefab;
            spawnedType = "돌진";
        }
        // 2. 돌진 좀비가 아니라면, 무작위 좀비 스폰 조건 확인
        else if (randomZombiePrefab != null && currentSurvivalTime >= timeToSpawnRandomZombies && Random.Range(0f, 1f) < randomZombieSpawnChance)
        {
            zombieToSpawn = randomZombiePrefab;
            spawnedType = "무작위";
        }
        // 3. 위 조건에 모두 해당하지 않으면 기본 좀비 (이미 zombieToSpawn = baseZombiePrefab; 로 설정됨)

        Instantiate(zombieToSpawn, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        Debug.Log(spawnedType + " 좀비가 " + selectedSpawnPoint.name + " 위치에 스폰되었습니다.");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            float timeLeft = survivalTimeGoal - currentSurvivalTime;
            int displayTime = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            timerText.text = "Time: " + displayTime.ToString() + "s";
        }
    }

    public void GameOver()
    {
        if (isGameOver || isGameWon) return;
        isGameOver = true;
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Debug.Log("게임 오버!");
    }

    public void GameWin()
    {
        if (isGameOver || isGameWon) return;
        isGameWon = true;
        Time.timeScale = 0f;
        if (gameWinPanel != null) gameWinPanel.SetActive(true);
        Debug.Log("게임 승리! (" + survivalTimeGoal + "초 생존)");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}