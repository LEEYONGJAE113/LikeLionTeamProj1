using UnityEngine;

public class LSM_NormalBullet : MonoBehaviour
{
    private int baseDamage = 5;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            HandleMonsterCollision(collision);
        }
    }

    // 몬스터와 충돌 시 처리하는 함수
    private void HandleMonsterCollision(Collider2D collision)
    {
        LSM_Monster monster = collision.gameObject.GetComponent<LSM_Monster>();
        if (monster == null)
            return;

        // 충정 상태일 경우 데미지 증가
        int damage = baseDamage;
        if (LSM_Triump.Instance.loyalty)
        {
            damage += 5;
            Debug.Log("충정총알");
        }

        // 치명 상태일 경우 데미지 증가
        if (LSM_Triump.Instance.fatal)
        {
            damage += 30;
            Debug.Log("치명총알");
        }

        // 몬스터에게 데미지 적용
        monster.Damage(damage);

        // 총알 삭제
        Destroy(gameObject);
    }
}
