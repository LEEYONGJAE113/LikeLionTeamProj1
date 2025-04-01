using UnityEngine;

public class LSM_CardBullet : MonoBehaviour
{
    void Start() { }

    void Update() { }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.Player.HPChange(-1);
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
