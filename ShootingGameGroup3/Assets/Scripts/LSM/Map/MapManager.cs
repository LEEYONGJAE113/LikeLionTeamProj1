using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("Stage Maps")]
    [SerializeField]
    private GameObject stage1MapPrefab;

    [SerializeField]
    private GameObject stage2MapPrefab;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform[] spawnPoints = new Transform[5];

    [Header("Enemy Prefabs")]
    [SerializeField]
    private GameObject cardSoldierPrefab;

    [SerializeField]
    private GameObject hatRabbitPrefab;

    [Header("Player Spawn")]
    [SerializeField]
    private Transform playerSpawnPoint; // Unity Inspector에서 설정할 플레이어 시작 위치

    [Header("Stage 2 Settings")]
    [SerializeField]
    private Transform playerSpawnPoint2;

    [SerializeField]
    private Transform bossSpawnPoint;

    [SerializeField]
    private GameObject cardQueenPrefab;

    [Header("CardQueen Settings")]
    [SerializeField]
    private Transform[] queenSoldierSpawnPoints = new Transform[2];

    [SerializeField]
    private Transform queenBulletSpawnPoint;

    [SerializeField]
    private Transform queenLazerSpawnPoint; // 추가

    [Header("Warning Areas")]
    [SerializeField]
    private GameObject WarningArea1;

    [SerializeField]
    private GameObject WarningArea2;

    public Transform[] GetQueenSoldierSpawnPoints() => queenSoldierSpawnPoints;

    public Transform GetQueenBulletSpawnPoint() => queenBulletSpawnPoint;

    public Transform GetQueenLazerSpawnPoint() => queenLazerSpawnPoint;

    public GameObject GetWarningArea1() => WarningArea1;

    public GameObject GetWarningArea2() => WarningArea2;

    private GameObject currentMap;
    private GameObject currentBoss;
    private int currentStage = 1;
    private int currentWave = 0;
    private bool isStageCleared = false;
    private bool isWaveCleared = true;
    private Coroutine currentWaveRoutine; // 현재 실행 중인 웨이브 코루틴

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartStage1();
        GameManager.Instance.Player.SpdChange(2f);
    }

    void Update()
    {
        // 디버깅용 스테이지 전환 키
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log($"Stage {currentStage} Clear!");
            ClearCurrentStage();
        }

        // 웨이브 스킵 키
        if (Input.GetKeyDown(KeyCode.N))
        {
            SkipToNextWave();
        }
    }

    private void SkipToNextWave()
    {
        // 현재 웨이브의 몬스터들 제거
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (GameObject monster in monsters)
        {
            Destroy(monster);
        }

        // 현재 실행 중인 웨이브 코루틴 중지
        if (currentWaveRoutine != null)
        {
            StopCoroutine(currentWaveRoutine);
        }

        // 다음 웨이브 시작
        switch (currentWave)
        {
            case 0:
                currentWave++;
                SpawnWave2();
                break;
            case 1:
                currentWave++;
                SpawnWave3();
                break;
            case 2:
                currentWave++;
                SpawnWave4();
                break;
            case 3:
                currentWave++;
                SpawnWave5();
                break;
            case 4:
                ClearCurrentStage();
                break;
        }
    }

    public void StartStage1()
    {
        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        currentStage = 1;
        currentWave = 0;
        isStageCleared = false;
        currentMap = Instantiate(stage1MapPrefab);

        // 플레이어 위치 설정
        if (GameManager.Instance.Player != null && playerSpawnPoint != null)
        {
            GameManager.Instance.Player.transform.position = playerSpawnPoint.position;
        }

        currentWaveRoutine = StartCoroutine(Stage1WaveRoutine());

    }

    private IEnumerator Stage1WaveRoutine()
    {
        // Wave 1: 2곳에서 CardSoldier 각 1마리
        yield return new WaitForSeconds(1f);
        SpawnWave1();
        yield return new WaitUntil(() => CheckWaveCleared());
        currentWave++;

        // Wave 2: 모든 포인트에서 CardSoldier 각 1마리
        yield return new WaitForSeconds(2f);
        SpawnWave2();
        yield return new WaitUntil(() => CheckWaveCleared());
        currentWave++;

        // Wave 3: 각 포인트에서 CardSoldier 2마리씩 (3초 간격)
        yield return new WaitForSeconds(2f);
        SpawnWave3();
        yield return new WaitUntil(() => CheckWaveCleared());
        currentWave++;

        // Wave 4: 랜덤 포인트 1곳에서 HatRabbit 1마리
        yield return new WaitForSeconds(2f);
        SpawnWave4();
        yield return new WaitUntil(() => CheckWaveCleared());
        currentWave++;

        // Wave 5: 한 곳에서 CardSoldier 3마리 + 다른 한 곳에서 HatRabbit 1마리
        yield return new WaitForSeconds(2f);
        SpawnWave5();
        yield return new WaitUntil(() => CheckWaveCleared());
        ClearCurrentStage();
    }

    private void SpawnWave1()
    {
        // 랜덤하게 2개의 스폰 포인트 선택
        int[] selectedPoints = GetRandomSpawnPoints(2);

        foreach (int pointIndex in selectedPoints)
        {
            Instantiate(cardSoldierPrefab, spawnPoints[pointIndex].position, Quaternion.identity);
        }
    }

    private void SpawnWave2()
    {
        // 모든 스폰 포인트에서 생성
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Instantiate(cardSoldierPrefab, spawnPoints[i].position, Quaternion.identity);
        }
    }

    private void SpawnWave3()
    {
        // 첫 번째 배치: 모든 포인트에서 CardSoldier 1마리씩
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Instantiate(cardSoldierPrefab, spawnPoints[i].position, Quaternion.identity);
        }

        // 3초 후 두 번째 배치
        StartCoroutine(DelayedSpawnWave3());
    }

    private IEnumerator DelayedSpawnWave3()
    {
        yield return new WaitForSeconds(3f);

        // 두 번째 배치: 모든 포인트에서 CardSoldier 1마리씩 추가
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Instantiate(cardSoldierPrefab, spawnPoints[i].position, Quaternion.identity);
        }
    }

    private void SpawnWave4()
    {
        // 랜덤하게 1개의 스폰 포인트 선택
        int spawnPoint = Random.Range(0, spawnPoints.Length);
        Instantiate(hatRabbitPrefab, spawnPoints[spawnPoint].position, Quaternion.identity);
    }

    private void SpawnWave5()
    {
        // 랜덤하게 2개의 다른 스폰 포인트 선택
        int[] selectedPoints = GetRandomSpawnPoints(2);

        // 첫 번째 포인트에서 CardSoldier 3마리 생성
        for (int i = 0; i < 3; i++)
        {
            Instantiate(
                cardSoldierPrefab,
                spawnPoints[selectedPoints[0]].position,
                Quaternion.identity
            );
        }

        // 두 번째 포인트에서 HatRabbit 1마리 생성
        Instantiate(hatRabbitPrefab, spawnPoints[selectedPoints[1]].position, Quaternion.identity);
    }

    private int[] GetRandomSpawnPoints(int count)
    {
        List<int> available = new List<int>(new int[] { 0, 1, 2, 3, 4 });
        int[] selected = new int[count];

        for (int i = 0; i < count; i++)
        {
            int randIndex = Random.Range(0, available.Count);
            selected[i] = available[randIndex];
            available.RemoveAt(randIndex);
        }

        return selected;
    }

    private bool CheckWaveCleared()
    {
        // 씬에서 모든 몬스터를 찾아서 확인
        return GameObject.FindGameObjectsWithTag("Monster").Length == 0;
    }

    public void StartStage2()
    {
        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        currentStage = 2;
        currentWave = 0;
        isStageCleared = false;
        currentMap = Instantiate(stage2MapPrefab);

        // 플레이어 위치 설정
        if (GameManager.Instance.Player != null && playerSpawnPoint2 != null)
        {
            GameManager.Instance.Player.transform.position = playerSpawnPoint2.position;
        }

        // 보스(CardQueen) 생성
        if (bossSpawnPoint != null && cardQueenPrefab != null)
        {
            currentBoss = Instantiate(
                cardQueenPrefab,
                bossSpawnPoint.position,
                Quaternion.identity
            );

            // 보스 사망 이벤트 리스닝
            LSM_Monster bossMonster = currentBoss.GetComponent<LSM_Monster>();
            if (bossMonster != null)
            {
                StartCoroutine(CheckBossDeath());
            }
        }
    }

    private IEnumerator CheckBossDeath()
    {
        while (currentBoss != null)
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 보스가 파괴되면 스테이지 클리어
        ClearCurrentStage();
    }

    public void ClearCurrentStage()
    {
        isStageCleared = true;

        if (currentStage == 1)
        {
            StartStage2();
        }
        else if (currentStage == 2)
        {
            GameManager.Instance.ClearScene(); // 게임 클리어 처리
            Debug.Log("Game Clear!");
        }
    }

    public int GetCurrentStage()
    {
        return currentStage;
    }
}
