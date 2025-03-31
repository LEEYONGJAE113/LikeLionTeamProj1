using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // 추가

public class Player : MonoBehaviour
{
    #region Const Numbers
    float MOVE_SPEED = 3f;
    float ROLLING_POWER = 10f;
    float ROLLING_TIME = 0.2f;
    float ROLLING_COOLDOWN = 2.8f; // 구르는 시간과 합쳐서 3초
    float MAX_HP = 3f;
    #endregion

    #region Player Stats
    float moveSpeed;
    float hp;
    float aas = 0;

    [SerializeField]
    GameObject currentWeapon;

    private bool isInvincible = false;
    private float invincibleDuration = 2f;
    private float blinkInterval = 0.2f;
    #endregion

    #region Player Actions
    bool isRolling;
    bool isKickBacked;
    bool readyToRoll;
    WaitForSeconds rollingTime;
    WaitForSeconds rollingCooldown;
    #endregion

    #region Components (Include Input System Vector)
    Vector2 inputVec;
    Rigidbody2D rb;
    SpriteRenderer playerSpriteRenderer;
    private Vector2 prevPosition;
    private ContactPoint2D[] contacts = new ContactPoint2D[10];
    private int contactCount = 0;
    #endregion

    void Awake()
    {
        rollingTime = new WaitForSeconds(ROLLING_TIME);
        rollingCooldown = new WaitForSeconds(ROLLING_COOLDOWN);
        moveSpeed = MOVE_SPEED;
        hp = MAX_HP;
        InitAllActions();
        rb = GetComponent<Rigidbody2D>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMove(InputValue input)
    {
        inputVec = input.Get<Vector2>();
    }

    void OnJump()
    {
        if (isRolling || !readyToRoll)
        {
            return;
        }
        StartCoroutine(Rolling());
    }

    void FixedUpdate()
    {
        if (isRolling)
        {
            return;
        }
        Move();
        FlipPlayer();
    }

    void Move()
    {
        Vector2 nextVec = inputVec * moveSpeed;
        Vector2 currentPosition = rb.position;
        bool hasMonsterCollision = false;

        contactCount = rb.GetContacts(contacts);

        for (int i = 0; i < contactCount; i++)
        {
            if (contacts[i].collider.CompareTag("Monster"))
            {
                Vector2 collisionNormal = contacts[i].normal;

                // 몬스터와의 충돌 각도가 너무 작으면 이동 제한
                float collisionAngle = Vector2.Angle(nextVec, -collisionNormal);
                if (collisionAngle < 90f)
                {
                    // 몬스터 방향으로의 이동 성분을 더 강하게 제거
                    nextVec = (Vector2)Vector3.ProjectOnPlane(nextVec, collisionNormal) * 0.5f;
                    hasMonsterCollision = true;
                }
            }
        }

        if (!hasMonsterCollision || nextVec.magnitude > 0)
        {
            rb.MovePosition(currentPosition + nextVec * Time.fixedDeltaTime);
        }
    }

    void InitAllActions()
    {
        isRolling = false;
        isKickBacked = false;
        readyToRoll = true;
    }

    void FlipPlayer()
    {
        if (GameManager.Instance.MouseManager.GetMousePos().x < transform.position.x)
        {
            playerSpriteRenderer.flipX = true;
            // 무기 뒤집기는 따로 분리  
        }
        else
        {
            playerSpriteRenderer.flipX = false;
            // 무기 뒤집기는 따로 분리
        }
    }

    IEnumerator Rolling()
    {
        isRolling = true;
        readyToRoll = false;
        Vector3 rollDir = new Vector3(inputVec.x, inputVec.y, 0).normalized;
        if (rollDir == Vector3.zero)
        {
            Vector3 rollToMouseVec =
                GameManager.Instance.MouseManager.GetMousePos() - transform.position;
            rollDir = new Vector3(rollToMouseVec.x, rollToMouseVec.y, 0).normalized;
        }
        rb.AddForce(rollDir * ROLLING_POWER, ForceMode2D.Impulse);
        yield return rollingTime;
        rb.linearVelocity = Vector2.zero;
        isRolling = false;
        yield return rollingCooldown;
        readyToRoll = true;
    }

    void Shoot()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
    }

    void Update()
    {
        ControlWeapon();
    }

    void ControlWeapon()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            GameManager.Instance.WeaponManager.ChangeBullet();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameManager.Instance.WeaponManager.ChangeWeapon();
        }

        if (Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.WeaponManager.NormalShoot();
        }

        if (Input.GetMouseButtonDown(1))
        {
            GameManager.Instance.WeaponManager.SpecialSkill();
        }

        if (Input.GetMouseButton(1))
        {
            GameManager.Instance.WeaponManager.ChargeSkill();
        }

        if (Input.GetMouseButtonUp(1))
        {
            GameManager.Instance.WeaponManager.ChargeSkillEnd();
        }
    }

    public void HPChange(float num)
    {
        // 피해를 입을 때만 무적 적용 (회복할 때는 제외)
        if (num < 0 && !isInvincible)
        {
            hp += num;
            if (hp <= 0)
            {
                hp = 0;
                Debug.Log("플레이어 사망");
                // 현재 씬 다시 로드
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
            Debug.Log("피해 입음");
            StartCoroutine(InvincibleRoutine());
        }
        else if (num > 0) // 회복
        {
            hp += num;
            if (hp >= MAX_HP)
            {
                hp = MAX_HP;
            }
        }
        Debug.Log("HP: " + hp);
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;
        float endTime = Time.time + invincibleDuration;

        // 깜빡임 효과
        while (Time.time < endTime)
        {
            playerSpriteRenderer.enabled = false;
            yield return new WaitForSeconds(blinkInterval);
            playerSpriteRenderer.enabled = true;
            yield return new WaitForSeconds(blinkInterval);
        }

        playerSpriteRenderer.enabled = true;
        isInvincible = false;
    }

    public void SpdChange(float num)
    {
        moveSpeed += num;
        if (moveSpeed <= 1)
        {
            moveSpeed = 1;
        }
    }

    public void AASChange(float num)
    {
        aas += num;
    }

    public void KickBackRequest(Vector3 vec, float amount)
    {
        StartCoroutine(KickBack(vec, amount));
    }

    IEnumerator KickBack(Vector3 vec, float Amount)
    {
        isKickBacked = true;
        rb.linearVelocity = vec * Amount;
        yield return new WaitForSeconds(0.2f);
        isKickBacked = false;
        
    }

    int maxHp = 7; // temp
    int baseHp = 5;
    int currentHp = 5;
    public int BasePlayerHp => baseHp;
    public int CurrentPlayerHp => currentHp;
}
