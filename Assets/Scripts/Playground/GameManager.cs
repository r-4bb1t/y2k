using UnityEngine;
// using UnityEngine.UI; // TextMeshPro를 주로 사용하면 이 줄은 필요 없을 수 있습니다.
using TMPro;          // TextMeshPro 사용
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수!
using System.Collections;

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
    // public GameObject endingScreenPanel;    // 엔딩 씬으로 대체되므로 제거
    // public TextMeshProUGUI endingMessageText;  // 엔딩 씬으로 대체되므로 제거
    public TextMeshProUGUI introSubtitleText;
    public GameObject introSubtitlePanel;

    [Header("Level Elements")]
    public SpriteRenderer doorSpriteRenderer;
    public Sprite doorOpenSprite;
    public Sprite doorClosedSprite;
    public GameObject exitTriggerZone;
    public Collider2D doorCollider; // ★ 새로 추가: 문의 Collider2D를 연결할 변수

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

    [Header("Scene Names")]
    public string endingSceneName = "YourEndingSceneNameHere"; // ★ 중요: 실제 엔딩 씬 이름으로 변경해주세요!

    // Cutscene Settings (컷신을 사용하지 않기로 했으므로 관련 변수 제거 또는 주석 처리)
    // public Transform playerCharacterTransform;
    // public Transform gatePositionTransform;
    // public float cutsceneMoveSpeed = 4f;

    // 내부 상태 변수들
    private float spawnTimer;
    private float currentSurvivalTime;
    private bool isGameOver = false;
    public bool isGameWon { get; private set; } = false;
    private Transform playerTransformForSpawning;
    // private bool isCutscenePlaying = true; // 컷신 사용 안 함
    private bool finalEscapeTriggered = false; // 최종 탈출(씬 로드)이 한 번만 실행되도록
    // private PlayerController playerControllerScript; // 컷신용이었으므로 일단 제거

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransformForSpawning = playerObject.transform;
            // playerControllerScript = playerObject.GetComponent<PlayerController>();
        }
        else
        {
            Debug.LogWarning("GameManager: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
        }

        isGameOver = false;
        isGameWon = false;
        finalEscapeTriggered = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        // if (endingScreenPanel != null) endingScreenPanel.SetActive(false); // 제거됨
        if (introSubtitlePanel != null) introSubtitlePanel.SetActive(false); // 컷신 UI를 인트로에 계속 쓰신다면 유지
        if (exitTriggerZone != null) exitTriggerZone.SetActive(false);

        if (doorSpriteRenderer != null && doorClosedSprite != null)
        {
            doorSpriteRenderer.sprite = doorClosedSprite;
        }

        // isCutscenePlaying = false; // 컷신 없으므로 바로 게임 시작 상태
        currentSurvivalTime = 0f;
        spawnTimer = 0f;
        UpdateTimerUI();
    }

    void Update()
    {
        // 게임 오버 상태일 때만 스페이스바로 재시작 (엔딩씬으로 넘어가면 이 GameManager는 파괴됨)
        if (isGameOver || isGameWon || finalEscapeTriggered) // 60초 생존했거나 이미 탈출씬으로 넘어갔다면
        {
            // 플레이어가 출구로 이동하기를 기다리거나, 이미 씬 전환됨.
            // 이 GameManager는 더 이상 게임 로직(타이머, 스폰)을 진행하지 않음.
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

    // PlayOpeningCutscene() 코루틴 및 ShowSubtitleForCutscene() 관련 함수들은 제거했다고 가정합니다.
    // 만약 간단한 인트로 메시지를 사용하고 싶다면 ShowSubtitleForCutscene() 등은 유지하셔도 됩니다.

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
                timerText.text = "Door opened! Escape!!!";
                return;
            }
            // finalEscapeTriggered 나 isGameOver 시 타이머 숨기는 로직은 PlayerReachedExit/GameOver에서 처리하거나 여기서도 가능
            if (finalEscapeTriggered || isGameOver)
            {
                // timerText.gameObject.SetActive(false); // 필요시 타이머 숨김
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
        SceneManager.LoadScene("GameOver");
    }

    // GameManager.cs
    public void GameWin() // 60초 생존 시
    {
        if (isGameWon || isGameOver || finalEscapeTriggered) return;

        isGameWon = true;
        Debug.Log("목표 시간(" + survivalTimeGoal + "초) 생존! 문이 열립니다. 좀비를 제거합니다.");

        // 1. 문 열기 (스프라이트 변경)
        if (doorSpriteRenderer != null && doorOpenSprite != null)
        {
            doorSpriteRenderer.sprite = doorOpenSprite;
        }

        // // ★ 2. 문의 콜라이더 비활성화 ★
        // if (doorCollider != null)
        // {
        //     doorCollider.enabled = false; // 콜라이더를 비활성화하여 통과 가능하게 만듦
        //     Debug.Log("문 콜라이더가 비활성화되었습니다.");
        // }
        // else
        // {
        //     Debug.LogWarning("GameManager: Door Collider가 연결되지 않았습니다.");
        // }

        // 3. 모든 좀비 제거
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie"); // 좀비 태그 확인!
        foreach (GameObject zombie in zombies)
        {
            Destroy(zombie);
        }
        Debug.Log(zombies.Length + " 마리의 좀비를 모두 제거했습니다.");
        UpdateTimerUI(); // "문이 열렸다!" 메시지 등으로 변경

        // 4. 출구 트리거 활성화
        if (exitTriggerZone != null)
        {
            exitTriggerZone.SetActive(true);
            Debug.Log("출구(ExitZone)가 활성화되었습니다.");
        }
    }

    // 플레이어가 열린 문 (ExitTriggerZone)에 도달했을 때 호출
    public void PlayerReachedExit()
    {
        if (!isGameWon || finalEscapeTriggered) return; // 조건 미달이거나 이미 탈출 처리됐으면 실행 안함

        finalEscapeTriggered = true; // 최종 탈출 처리됨
        Debug.Log("플레이어가 출구에 도달했습니다. 엔딩 씬으로 이동합니다...");

        // 새 씬을 로드하기 전에 Time.timeScale을 1로 되돌리는 것이 좋습니다.
        // 특히 엔딩 씬에 애니메이션이나 다른 시간 기반 요소가 있다면 더욱 그렇습니다.
        Time.timeScale = 1f;
        SceneManager.LoadScene("Ending"); // 엔딩 씬 로드
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}