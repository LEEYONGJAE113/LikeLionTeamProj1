using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LSM_CardQueen : MonoBehaviour
{
    private LSM_Monster monsterScript;

    public GameObject Player;

    [SerializeField]
    private GameObject CardSoldier;

    [SerializeField]
    private GameObject CardBullet;

    [SerializeField]
    private GameObject RuinLazer;

    [SerializeField]
    private GameObject LazerWarning;

    private Transform[] soldierSpawnPoints;
    private Transform bulletSpawnPoint;
    private Transform RuinLazerSpawnPoint;

    public bool canSpawn = true;
    public float spawnInterval = 15f;

    public bool canShoot = true;
    public float shootInterval = 3f;

    private bool pattern150Triggered = false;
    private bool pattern100Triggered = false;
    private bool pattern50Triggered = false;
    private bool pattern10Triggered = false;

    [Header("Big Card Attack")]
    private GameObject warningArea1;
    private GameObject warningArea2;

    [SerializeField]
    private GameObject skullCardPrefab;

    [SerializeField]
    private float warningDuration = 2f;

    [SerializeField]
    private float attackDuration = 2f;

    void Awake()
    {
        monsterScript = GetComponent<LSM_Monster>();
    }

    void Start()
    {
        Player = GameManager.Instance.Player.gameObject;
        monsterScript.SetInvincibility(false);
        monsterScript.isTracking = false;
        monsterScript.isSmells = false;
        monsterScript.health = 200;

        // MapManager에서 스폰 포인트 가져오기
        soldierSpawnPoints = MapManager.Instance.GetQueenSoldierSpawnPoints();
        bulletSpawnPoint = MapManager.Instance.GetQueenBulletSpawnPoint();
        RuinLazerSpawnPoint = MapManager.Instance.GetQueenLazerSpawnPoint();
        warningArea1 = MapManager.Instance.GetWarningArea1();
        warningArea2 = MapManager.Instance.GetWarningArea2();

        // null 체크 추가
        if (warningArea1 == null || warningArea2 == null)
        {
            Debug.LogError("Warning areas not assigned in MapManager!");
        }

        // 초기에 경고 영역 비활성화
        if (warningArea1 != null)
            warningArea1.SetActive(false);
        if (warningArea2 != null)
            warningArea2.SetActive(false);
    }

    void Update()
    {
        //SpawnSolider();
        CardAttack();
        PatternManage();
    }

    void PatternManage()
    {
        int health = monsterScript.health;

        if (health <= 150 && !pattern150Triggered)
        {
            pattern150Triggered = true;
            Pattern150();
        }

        if (health <= 100 && !pattern100Triggered)
        {
            pattern100Triggered = true;
            Pattern100();
        }

        if (health <= 50 && !pattern50Triggered)
        {
            pattern50Triggered = true;
            Pattern50();
        }

        if (health <= 10 && !pattern10Triggered)
        {
            pattern10Triggered = true;
            Pattern10();
        }
    }

    void Pattern150()
    {
        Debug.Log("150줄 패턴 실행");
        Big_CardAttack();
    }

    void Pattern100()
    {
        Debug.Log("100줄 패턴 실행");
        Ruined_Lazer();
    }

    void Pattern50()
    {
        Debug.Log("50줄 패턴 실행");
    }

    void Pattern10()
    {
        Debug.Log("10줄 패턴 실행");
        Ruined_Lazer();
    }

    void SpawnSolider()
    {
        if (!canSpawn || soldierSpawnPoints == null || soldierSpawnPoints.Length < 2)
            return;

        GameObject soldier = Instantiate(
            CardSoldier,
            soldierSpawnPoints[0].position,
            Quaternion.identity
        );
        GameObject soldier2 = Instantiate(
            CardSoldier,
            soldierSpawnPoints[1].position,
            Quaternion.identity
        );
        canSpawn = false;
        StartCoroutine(SpawnDelay());
    }

    void CardAttack()
    {
        if (!canShoot || bulletSpawnPoint == null)
            return;

        GameObject cardBullet = Instantiate(
            CardBullet,
            bulletSpawnPoint.position,
            Quaternion.identity
        );

        cardBullet.transform.Rotate(0, 0, 90f);

        Rigidbody2D rb = cardBullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 direction = (Player.transform.position - bulletSpawnPoint.position).normalized;
            rb.linearVelocity = direction * 8f;
        }

        canShoot = false;
        StartCoroutine(ShootDelay());
    }

    IEnumerator SpawnDelay()
    {
        yield return new WaitForSeconds(spawnInterval);
        canSpawn = true;
    }

    IEnumerator ShootDelay()
    {
        yield return new WaitForSeconds(shootInterval);
        canShoot = true;
    }

    void Big_CardAttack()
    {
        GameObject selectedArea = (UnityEngine.Random.value > 0.5f) ? warningArea1 : warningArea2;
        StartCoroutine(BigCardSequence(selectedArea));
    }

    private IEnumerator BigCardSequence(GameObject warningArea)
    {
        if (warningArea1 == null || warningArea2 == null)
        {
            Debug.LogError("Warning areas not assigned!");
            yield break;
        }

        warningArea1.SetActive(false);
        warningArea2.SetActive(false);

        warningArea.SetActive(true);

        float elapsedTime = 0f;
        SpriteRenderer warningSprite = warningArea.GetComponent<SpriteRenderer>();

        while (elapsedTime < warningDuration)
        {
            warningSprite.enabled = !warningSprite.enabled;
            yield return new WaitForSeconds(0.2f);
            elapsedTime += 0.2f;
        }

        warningArea.SetActive(false);

        // Create skull card with Z-axis rotation of -90 degrees
        GameObject skullCard = Instantiate(
            skullCardPrefab,
            warningArea.transform.position,
            Quaternion.Euler(0, 0, -90f) // Changed to -90 degrees for correct orientation
        );

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            warningArea.transform.position,
            warningArea.transform.localScale,
            0f
        );

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                GameManager.Instance.Player.HPChange(-1f);
            }
        }

        yield return new WaitForSeconds(attackDuration);
        Destroy(skullCard);
    }

    void Ruined_Lazer()
    {
        StartCoroutine(LazerSequence());
    }

    IEnumerator LazerSequence()
    {
        GameObject warning = Instantiate(
            LazerWarning,
            RuinLazerSpawnPoint.position,
            Quaternion.identity
        );
        Transform targetTransform = Player.transform;
        Debug.Log("조준시작");

        float warningDuration = 5f;
        float elapsedTime = 0f;

        while (elapsedTime < warningDuration)
        {
            GameObject[] muffins = GameObject.FindGameObjectsWithTag("Muffin");
            if (muffins.Length > 0)
            {
                float minDistance = float.MaxValue;
                GameObject nearestMuffin = null;

                foreach (GameObject muffin in muffins)
                {
                    float distance = Vector2.Distance(
                        RuinLazerSpawnPoint.position,
                        muffin.transform.position
                    );
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestMuffin = muffin;
                    }
                }

                if (nearestMuffin != null)
                {
                    targetTransform = nearestMuffin.transform;
                }
            }

            if (warning != null)
            {
                Vector2 direction = (
                    RuinLazerSpawnPoint.position - targetTransform.position
                ).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                warning.transform.rotation = Quaternion.Euler(0, 0, angle);

                Transform warningEffect = warning.transform.GetChild(0);
                if (warningEffect != null)
                {
                    float distance = Vector2.Distance(
                        RuinLazerSpawnPoint.position,
                        targetTransform.position
                    );
                    Vector3 effectScale = warningEffect.localScale;
                    effectScale.x = distance;
                    warningEffect.localScale = effectScale;

                    Vector3 effectPosition = warningEffect.localPosition;
                    effectPosition.x = -distance / 2f;
                    warningEffect.localPosition = effectPosition;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (warning != null)
        {
            Destroy(warning);
        }

        GameObject ruinLazer = Instantiate(
            RuinLazer,
            RuinLazerSpawnPoint.position,
            RuinLazer.transform.rotation
        );
        Vector2 finalDirection = (
            targetTransform.position - RuinLazerSpawnPoint.position
        ).normalized;
        Rigidbody2D rb = ruinLazer.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = finalDirection * 30f; // Adjust speed as needed
        }
        Debug.Log("발사");

        yield return new WaitForSeconds(2f);
        Destroy(ruinLazer);
    }
}
