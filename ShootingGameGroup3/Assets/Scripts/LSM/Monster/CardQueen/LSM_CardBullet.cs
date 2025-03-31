using UnityEngine;

public class LSM_CardBullet : MonoBehaviour
{
    public float speed = 3f; //미사일 속도
    private Vector2 direction;

    void Start() { }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Wall"))
        {
            GameManager.Instance.Player.HPChange(-1);
            Destroy(gameObject);
        }
    }
}
