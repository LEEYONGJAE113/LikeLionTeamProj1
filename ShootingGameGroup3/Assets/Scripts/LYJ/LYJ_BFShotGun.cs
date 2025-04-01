using System.Collections;
using UnityEngine;

public class LYJ_BFShotGun : MonoBehaviour
{
    #region Basic Stats
    private float _damage;
    public float Damage => _damage;

    private bool _readyToShoot;
    public bool ReadyToShoot => _readyToShoot;

    private int _maxBullet;
    public int MaxBullet => _maxBullet;

    private WaitForSeconds _attackDelay;
    public WaitForSeconds AttackDelay => _attackDelay;
    #endregion

    const float FIRE_ANGLE = 15f; // temp
    [SerializeField]
    GameObject _bullet;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _readyToShoot = true;
        // 이하 수치조정 필요, temp now
        _damage = 7;
        _maxBullet = 7;
        _attackDelay = new WaitForSeconds(1.5f);
    }

    void OnEnable()
    {
        // GameManager.Instance.Player.SpdChange(-3.5f);
    }

    void FixedUpdate()
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.right, GameManager.Instance.MouseManager.GetMousePos() - transform.position);
    }


    public void Fire()
    {
        if (!ReadyToShoot) { return; }

        Vector2 direction = (GameManager.Instance.MouseManager.GetMousePos() - transform.position).normalized;
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 이러면 굳이 X Y 따로 구할 필요 없음


        for (int i = 0; i < MaxBullet; ++i)
        {
            GameObject currentBullet = Instantiate(_bullet, transform.position, Quaternion.identity);

            float currentAngle = FIRE_ANGLE / (MaxBullet - 1) * i - FIRE_ANGLE / 2.0f; // 균등한 각도
            float calculatedAngle = baseAngle + currentAngle;
            float bulletVecX = Mathf.Cos(calculatedAngle * Mathf.Deg2Rad);
            float bulletVecY = Mathf.Sin(calculatedAngle * Mathf.Deg2Rad);

            // float currentAngle = TOTAL_ANGLE / MaxBullet * i/* - (TOTAL_ANGLE/2.0f)*//*아마 여기가 에임 이상의 원인으로 추정...*/;
            // float bulletVecX = GameManager.Instance.Aim.GetMousePos().x-transform.position.x + Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            // float bulletVecY = GameManager.Instance.Aim.GetMousePos().y-transform.position.y + Mathf.Sin(currentAngle * Mathf.Deg2Rad);

            Vector2 bulletVec = new Vector2(bulletVecX, bulletVecY).normalized;


            currentBullet.GetComponent<LYJ_Bullet>().ShootBullet(bulletVec, 15f/*temp*/, _damage);

            Destroy(currentBullet, 1f);
        }
        // Vector3 kickBack = -(GameManager.Instance.Aim.GetMousePos() - GameManager.Instance.Player.transform.position).normalized;
        Vector3 kickBack = -direction.normalized;
        GameManager.Instance.Player.KickBackRequest(kickBack, 20f/*temp*/);
        StartCoroutine(DelayFire());
    }



    IEnumerator DelayFire()
    {
        _readyToShoot = false;
        yield return AttackDelay;
        _readyToShoot = true;
    }

}
