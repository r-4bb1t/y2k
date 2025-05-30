using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
// using System.Collections; // 코루틴을 사용하지 않으므로 이 줄은 없어도 됩니다.

public class GameManager : MonoBehaviour
{
    [Header("Game Objects & Prefabs")]
    public GameObject baseZombiePrefab;
    public GameObject chargingZombiePrefab;
    public GameObject randomZombiePrefab;
    public List<Transform> spawnPoints;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public GameObject endingScreenPanel;    // 최종 탈출 성공 시 보여줄 패널
    public TextMeshProUGUI endingMessageText;  // 엔딩 패널에 표시될 메시지 텍스트
    // public TextMeshProUGUI introSubtitleText; // 컷신용이었으므로 제거 또는 주석 처리
    // public GameObject introSubtitlePanel;    // 컷신용이었으므로 제거 또는 주석 처리

    [Header("Level Elements")]
    public SpriteRenderer doorSpriteRenderer;
    public Sprite doorOpenSprite;
    public Sprite doorClosedSprite;
    public GameObject exitTriggerZone;

    [Header("Game Settings")]
    public float spawnInterval = 10f;
    public float survivalTimeGoal = 60f;
    public float minSpawnDistanceToPlayer = 5f;
    public float timeToSpawnChargingZombies = 30f;
    [Range(0f, 1f)]
    public float chargingZombieSpawnChance = 0.1f;
    public float timeToSpawnRandomZombies = 15f;
    [Range(0f, 1f)]
    public float randomZombieSpawnChance = 0.3f;

    // [Header("Cutscene Settings")] // 컷신 관련 변수들 제거 또는 주석 처리
    // public Transform playerCharacterTransform; // 대신 playerTransformForSpawning 사용
    // public Transform gatePositionTransform;
    // public float cutsceneMoveSpeed = 4f;

    // 내부 상태 변수들
    private float spawnTimer;
    private float currentSurvivalTime;
    private bool isGameOver = false;
    public bool isGameWon = false;
    private Transform playerTransformForSpawning; // 좀비 스폰 시 플레이어 위치 참조용
    // private bool isCutscenePlaying = true; // 컷신 플래그 제거
    private bool finalEscapeTriggered = false;
    // private PlayerController playerControllerScript; // 컷신에서 플레이어 제어용이었으므로 일단 제거

    void Start()
    {
        // 플레이어 참조 설정 (주로 좀비 스폰 시 거리 계산용)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransformForSpawning = playerObject.transform;
            // playerControllerScript = playerObject.GetComponent<PlayerController>(); // 필요하다면 유지
        }
        else
        {
            Debug.LogWarning("GameManager: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        // 게임 상태 초기화
        isGameOver = false;
        isGameWon = false;
        finalEscapeTriggered = false;
        Time.timeScale = 1f; // 게임 시간 정상 속도

        // UI 패널들 초기 비활성화
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (endingScreenPanel != null) endingScreenPanel.SetActive(false);
        // if (introSubtitlePanel != null) introSubtitlePanel.SetActive(false); // 컷신 UI 제거
        if (exitTriggerZone != null) exitTriggerZone.SetActive(false);

        // 문 상태 초기화
        if (doorSpriteRenderer != null && doorClosedSprite != null)
        {
            doorSpriteRenderer.sprite = doorClosedSprite;
        }

        // 컷신이 없으므로 게임 관련 타이머 즉시 초기화 및 시작
        currentSurvivalTime = 0f;
        spawnTimer = 0f; // 첫 좀비 바로 스폰 (또는 spawnInterval 값으로 설정)
        UpdateTimerUI();

        // 플레이어 컨트롤은 기본적으로 활성화된 상태로 시작한다고 가정
        // if (playerControllerScript != null) playerControllerScript.SetInputEnabled(true);
    }

    void Update()
    {
        // 게임이 완전히 끝난 상태 (게임오버 또는 최종 탈출 성공)일 때 스페이스바로 재시작
        bool canRestart = isGameOver || finalEscapeTriggered;
        if (canRestart)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RestartGame();
            }
            return;
        }

        // (isCutscenePlaying 체크 제거됨)

        // isGameWon 상태 (60초 생존, 좀비 제거, 문 열림)에서는 플레이어가 출구로 이동하기를 기다림
        if (isGameWon)
        {
            return;
        }

        // --- 이하 일반 게임 진행 로직 ---
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

    // PlayOpeningCutscene() 코루틴 전체 제거
    // ShowSubtitleForCutscene() 함수 전체 제거

    void SpawnZombie()
    {
        if (spawnPoints.Count == 0 || baseZombiePrefab == null) return;

        List<Transform> availableSpawnPoints = new List<Transform>();
        if (playerTransformForSpawning != null)
        {
            foreach (Transform sp in spawnPoints)
            {
                if (Vector2.Distance(sp.position, playerTransformForSpawning.position) >= minSpawnDistanceToPlayer)
                {
                    availableSpawnPoints.Add(sp);
                }
            }
        }
        if (availableSpawnPoints.Count == 0) availableSpawnPoints.AddRange(spawnPoints);
        if (availableSpawnPoints.Count == 0) return;

        Transform selectedSpawnPoint = availableSpawnPoints[Random.Range(0, availableSpawnPoints.Count)];
        GameObject zombieToSpawn = baseZombiePrefab;

        if (chargingZombiePrefab != null && currentSurvivalTime >= timeToSpawnChargingZombies && Random.Range(0f, 1f) < chargingZombieSpawnChance)
        {
            zombieToSpawn = chargingZombiePrefab;
        }
        else if (randomZombiePrefab != null && currentSurvivalTime >= timeToSpawnRandomZombies && Random.Range(0f, 1f) < randomZombieSpawnChance)
        {
            zombieToSpawn = randomZombiePrefab;
        }

        Instantiate(zombieToSpawn, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            if (isGameWon && !finalEscapeTriggered)
            {
                timerText.text = "문이 열렸다! 탈출하라!";
                return;
            }
            if (finalEscapeTriggered || isGameOver)
            {
                // timerText.gameObject.SetActive(false); // 선택사항: 게임 끝나면 타이머 숨기기
                return;
            }

            float timeLeft = survivalTimeGoal - currentSurvivalTime;
            int displayTime = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            timerText.text = "Time Left: " + displayTime.ToString() + "s";
        }
    }

    public void GameOver()
    {
        if (isGameOver || isGameWon || finalEscapeTriggered) return;
        isGameOver = true;
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Debug.Log("게임 오버!");
    }

    public void GameWin() // 60초 생존 시
    {
        if (isGameWon || isGameOver || finalEscapeTriggered) return;

        isGameWon = true;
        Debug.Log("목표 시간(" + survivalTimeGoal + "초) 생존! 문이 열립니다. 좀비를 제거합니다.");

        if (doorSpriteRenderer != null && doorOpenSprite != null)
        {
            doorSpriteRenderer.sprite = doorOpenSprite;
        }

        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        foreach (GameObject zombie in zombies)
        {
            Destroy(zombie);
        }
        Debug.Log(zombies.Length + " 마리의 좀비를 모두 제거했습니다.");
        UpdateTimerUI();

        if (exitTriggerZone != null)
        {
            exitTriggerZone.SetActive(true);
            Debug.Log("출구(ExitZone)가 활성화되었습니다.");
        }
    }

    public void PlayerReachedExit() // 출구 도달 시
    {
        if (!isGameWon || finalEscapeTriggered) return;

        finalEscapeTriggered = true;
        Time.timeScale = 0f;
        Debug.Log("플레이어가 출구에 도달했습니다. 탈출 성공!");

        if (endingScreenPanel != null)
        {
            if (endingMessageText != null)
            {
                endingMessageText.text = "탈출 성공!";
            }
            endingScreenPanel.SetActive(true);
        }

        if (timerText != null) timerText.gameObject.SetActive(false); // 타이머 숨김
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // 게임오버 패널 혹시 떠있으면 숨김
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}