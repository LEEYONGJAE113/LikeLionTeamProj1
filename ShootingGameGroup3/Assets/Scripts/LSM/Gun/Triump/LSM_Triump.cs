using System.Collections;
using UnityEngine;

public class LSM_Triump : MonoBehaviour
{
    public static LSM_Triump Instance { get; private set; }

    [Header("총알 프리팹")]
    public GameObject normal_bullet;
    public GameObject jack_bullet;
    public GameObject queen_bullet;
    public GameObject king_bullet;

    public Transform pos = null;
    public SpriteRenderer gunSpriteRenderer = null;

    private bool isReady = true;
    private bool isCooldown = false;
    private float remainingCooldown = 0f;
    private bool draw = false;
    public bool loyalty = false;
    public bool fatal = false;

    private int loyalty_remain = 3;
    private int fatal_remain = 1;
    private int spadeStack = 0;
    private int jokerStack = 0;

    private const float bulletSpeed = 20f;
    private const float shoot_delay = 0.5f;

    public enum ShootType
    {
        Normal,
        Jack,
        Queen,
        King
    }

    public enum DrawType
    {
        Jack,
        Queen,
        King
    }

    public ShootType currentShootType = ShootType.Normal;
    public DrawType currentDrawType = DrawType.Jack;

    private void Awake()
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

    private void Start()
    {
        gunSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (isCooldown)
        {
            remainingCooldown -= Time.deltaTime;
            if (remainingCooldown <= 0f)
            {
                isCooldown = false;
                isReady = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Triump_Left_Shift();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Triump_Left_Click();
        }
        if (Input.GetMouseButtonDown(1))
        {
            Triump_Right_Click();
        }
        HandleJoker();
        CheckSpadeStack();
        //FlipGun();
    }

    // 왼쪽 클릭 - 발사
    public void Triump_Left_Click()
    {
        if (isReady)
        {
            Vector3 targetPosition = GameManager.Instance.MouseManager.GetMousePos();
            targetPosition.z = 0;

            FireBullet(currentShootType, targetPosition);
            isReady = false;
            isCooldown = true;
            remainingCooldown = shoot_delay;
        }
    }

    // 오른쪽 클릭 - 드로우 카드 선택
    public void Triump_Right_Click()
    {
        if (draw)
        {
            SetShootTypeBasedOnDraw();
        }
    }

    // LeftShift - 드로우 카드 변경
    public void Triump_Left_Shift()
    {
        if (draw)
        {
            CycleDrawType();
        }
    }

    // 스페이드 스택 증가 체크
    public void CheckSpadeStack()
    {
        if (spadeStack == 10)
        {
            draw = true;
            spadeStack = 0;
        }
    }

    // 현재 발사 타입에 맞는 총알을 발사하는 함수
    private void FireBullet(ShootType shootType, Vector3 targetPosition)
    {
        if (shootType == ShootType.Queen)
        {
            FireQueenBullets();
            currentShootType = ShootType.Normal; // Queen 발사 후 기본 총알로 돌아가도록 설정
            return;
        }

        GameObject bulletPrefab = GetBulletPrefab(shootType);

        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, pos.position, Quaternion.identity);
            Vector3 direction = (targetPosition - pos.position).normalized;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction * bulletSpeed;
        }

        UpdateStack(shootType);
        CheckFatalState();
        CheckLoyaltyState();

        // 특수 총알 발사 후 기본 총알로 돌아가기
        if (shootType != ShootType.Normal)
        {
            currentShootType = ShootType.Normal;
        }
    }

    // Queen 상태에서 두 개의 탄환 생성
    private void FireQueenBullets()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
        GameObject firstTarget = null;
        GameObject secondTarget = null;
        float minDist1 = Mathf.Infinity;
        float minDist2 = Mathf.Infinity;
        Vector3 currentPos = transform.position;

        // 가장 가까운 두 적을 찾아 저장
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, currentPos);
            if (dist < minDist1 && dist <= 10f)
            {
                minDist2 = minDist1;
                secondTarget = firstTarget;
                minDist1 = dist;
                firstTarget = enemy;
            }
            else if (dist < minDist2 && dist <= 10f)
            {
                minDist2 = dist;
                secondTarget = enemy;
            }
        }

        // 두 적을 향해 Queen 총알 발사
        for (int i = 0; i < 2; i++)
        {
            GameObject bullet = Instantiate(queen_bullet, pos.position, Quaternion.identity);
            LSM_QueenBullet queenScript = bullet.GetComponent<LSM_QueenBullet>();
            queenScript.SetTarget(i == 0 ? firstTarget : secondTarget ?? firstTarget);
        }
    }

    // 발사 타입에 맞는 총알 프리팹을 반환하는 함수
    private GameObject GetBulletPrefab(ShootType shootType)
    {
        switch (shootType)
        {
            case ShootType.Normal:
                return normal_bullet;
            case ShootType.Jack:
                return jack_bullet;
            case ShootType.Queen:
                return queen_bullet;
            case ShootType.King:
                return king_bullet;
            default:
                return null;
        }
    }

    // 스페이드나 조커 스택을 업데이트하는 함수
    private void UpdateStack(ShootType shootType)
    {
        switch (shootType)
        {
            case ShootType.Normal:
                if (spadeStack < 10 && !draw)
                    spadeStack++;
                break;
            case ShootType.Jack:
            case ShootType.Queen:
            case ShootType.King:
                if (jokerStack < 3)
                    jokerStack++;
                break;
        }
    }

    // 드로우 타입을 순차적으로 변경하는 함수
    private void CycleDrawType()
    {
        currentDrawType = (DrawType)(((int)currentDrawType + 1) % 3);
    }

    // 드로우 타입에 맞는 발사 타입을 설정하는 함수
    private void SetShootTypeBasedOnDraw()
    {
        currentShootType = currentDrawType switch
        {
            DrawType.Jack => ShootType.Jack,
            DrawType.Queen => ShootType.Queen,
            DrawType.King => ShootType.King,
            _ => currentShootType
        };

        draw = false;
    }

    // 충정 상태를 갱신하는 함수
    private void CheckLoyaltyState()
    {
        if (loyalty)
        {
            loyalty_remain--;
            if (loyalty_remain == 0)
            {
                loyalty = false;
                loyalty_remain = 3; // 초기화
            }
        }
    }

    // 조커 드로우 처리 함수
    private void HandleJoker()
    {
        if (jokerStack == 3)
        {
            FateBlessing();
            jokerStack = 0;
        }
    }

    // 운명 실현 관련 함수
    private void FateBlessing()
    {
        int selecting = Random.Range(0, 3);
        switch (selecting)
        {
            case 0:
                Blessing_Speed();
                break;
            case 1:
                Blessing_Health();
                break;
            case 2:
                Blessing_Power();
                break;
        }
    }

    // 신속 실현
    private void Blessing_Speed()
    {
        GameManager.Instance.Player.SpdChange(3f);
        Debug.Log("신속실현");
    }

    // 회복 실현
    private void Blessing_Health()
    {
        GameManager.Instance.Player.HPChange(1f);
        Debug.Log("회복실현");
    }

    // 치명 실현
    private void Blessing_Power()
    {
        fatal = true;
        Debug.Log("치명실현");
    }

    // 치명 상태를 갱신하는 함수
    private void CheckFatalState()
    {
        if (fatal)
        {
            fatal_remain--;
            if (fatal_remain <= 0)
            {
                fatal = false;
                fatal_remain = 1; // 초기화
            }
        }
    }

    void FlipGun()
    {
        if (GameManager.Instance.MouseManager.GetMousePos().x < transform.position.x)
        {
            gunSpriteRenderer.flipX = true;
        }
        else
        {
            gunSpriteRenderer.flipX = false;
        }
    }
}
