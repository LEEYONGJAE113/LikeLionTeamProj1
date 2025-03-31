using System.Collections;
using UnityEngine;

public class LYJ_Enemy_Oil : MonoBehaviour
{
    const float FIRE_CHANCE = 0.1f;
    const float BURN_DAMAGE = 0.5f;
    const float BURN_DELAY = 0.5f;
    float moneyChaseRange = 5f;
    float[] hpForWave = { 5, 10, 15, 20, 25 }; // temp
    float hp;
    private float moveSpeed = 1.5f;
    bool isHitRecent;
    bool isHitWithOil;
    int burnStack;

    Rigidbody2D _rb;
    WaitForSeconds oilDropInterval;
    WaitForSeconds burnDelay;
    SpriteRenderer spriteRenderer;
    [SerializeField]
    GameObject oil;

    Transform target;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        oilDropInterval = new WaitForSeconds(2f);
        burnDelay = new WaitForSeconds(BURN_DELAY);
        spriteRenderer = GetComponent<SpriteRenderer>();
        target = GameManager.Instance.Player.transform;
        isHitRecent = false;
        isHitWithOil = false;
    }

    void OnEnable()
    {
        StartCoroutine(DropOil());
        hp = hpForWave[Mathf.Min(GameManager.Instance.SpawnManager.CurrentWave, 4)];
        isHitRecent = false;
        isHitWithOil = false;
        burnStack = 0;
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isHitRecent)
        {
            if (collision.CompareTag("Bullet"))
            {
                hp -= collision.GetComponent<LYJ_Bullet>().Damage;
                BurnFire(FIRE_CHANCE);
            }
            if (collision.CompareTag("Alcohol"))
            {
                hp -= collision.GetComponent<LYJ_AlcoholBurner>().Damage;
                BurnFire(100);
            }
            
            StartCoroutine(HitReaction());
        }
        if (!isHitWithOil && collision.CompareTag("Oil") && collision.GetComponent<LYJ_Oil>().IsBurn)
        {
            hp -= 0.5f;
            BurnFire(FIRE_CHANCE);
            StartCoroutine(OilReaction());
        } 

        if (hp <= 0)
        {
            Die();
        }
    }

    void UpdateTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, moneyChaseRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Money"))
            {
                target = hit.transform;
                return;
            }
        }
        target = GameManager.Instance.Player.transform;
    }
    void FixedUpdate()
    {
        UpdateTarget();
        Vector2 nextVec = (target.position-transform.position).normalized * moveSpeed;
        _rb.linearVelocity = nextVec;
        spriteRenderer.flipX = target.transform.position.x < transform.position.x;
    }

    IEnumerator DropOil()
    {
        while (gameObject.activeSelf)
        {
            yield return oilDropInterval;
            Instantiate(oil, transform.position, Quaternion.identity); // 나중에 오브젝트 풀링으로 수정 필요
        }
    }

    void BurnFire(float percent)
    {
        if (burnStack >= 3) { burnStack = 3; }
        if (Random.value <= percent)
        {
            StartCoroutine(Burn());
        }
    }

    IEnumerator HitReaction()
    {
        isHitRecent = true;
        yield return new WaitForSeconds(0.1f);
        isHitRecent = false;
    }

    IEnumerator OilReaction()
    {
        isHitWithOil = true;
        yield return new WaitForSeconds(0.5f);
        isHitWithOil = false;
    }

    IEnumerator Burn()
    {
        while (hp >= 0 && burnStack <= 3)
        {
            hp -= BURN_DAMAGE * burnStack;
            yield return burnDelay;
            spriteRenderer.color = Color.red;
        }
    }

    void Die()
    {
        StopCoroutine(HitReaction());
        StopCoroutine(Burn());
        StopCoroutine(DropOil());
        burnStack = 0;
        PoolManager.Instance.ReturnGameObject(gameObject);
        Destroy(gameObject);
    }
}
