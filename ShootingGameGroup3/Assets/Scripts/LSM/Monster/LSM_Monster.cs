using UnityEngine;

public class LSM_Monster : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    public int health = 10;
    public float move_speed = 1.5f;
    public bool invincibility = false;
    public bool isSmells = false;
    public bool isTracking = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Tracking();
        Death();
    }

    public void SetInvincibility(bool state)
    {
        invincibility = state;
        Debug.Log("무적 상태: " + (invincibility ? "활성화" : "비활성화"));
    }

    public void Damage(int damage)
    {
        if (invincibility)
        {
            Debug.Log("무적 상태");
            return;
        }

        health -= damage;
        Debug.Log("남은 체력: " + health);
    }

    public void Death()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            Debug.Log("몬스터가 죽었습니다.");
        }
    }

    public void Tracking()
    {
        if (!isTracking)
            return;

        if (player == null)
            return;

        if (isSmells)
        {
            GameObject[] muffins = GameObject.FindGameObjectsWithTag("Muffin");
            if (muffins.Length > 0)
            {
                GameObject nearestMuffin = null;
                float minDistance = float.MaxValue;

                foreach (GameObject muffin in muffins)
                {
                    float distance = Vector2.Distance(
                        transform.position,
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
                    Vector3 direction = nearestMuffin.transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
                    spriteRenderer.flipX = (direction.x < 0);
                    rb.linearVelocity = direction.normalized * move_speed;
                }
            }
        }
        else
        {
            Vector3 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            //transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
            spriteRenderer.flipX = (direction.x < 0);
            rb.linearVelocity = direction.normalized * move_speed;
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // 충돌 시 몬스터의 속도를 0으로 설정
            rb.linearVelocity = Vector2.zero;
            GameManager.Instance.Player.HPChange(-1f);
            //Debug.Log("플레이어에게 피해를 주었습니다.");
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // 충돌 중에도 속도를 0으로 유지
            rb.linearVelocity = Vector2.zero;
        }
    }
}
