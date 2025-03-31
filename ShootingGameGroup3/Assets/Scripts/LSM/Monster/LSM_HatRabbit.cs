using UnityEngine;

public class LSM_HatRabbit : MonoBehaviour
{
    public bool hideTime = true;
    private LSM_Monster monsterScript;
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite hatOffSprite; // Inspector에서 할당할 모자 벗은 스프라이트

    void Awake()
    {
        monsterScript = GetComponent<LSM_Monster>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        monsterScript.move_speed = 5f;
        monsterScript.SetInvincibility(true);
        monsterScript.isTracking = false;
        monsterScript.isSmells = false;
    }

    void Update() { }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("SpecialBullet"))
        {
            HatOff();
            Debug.Log("모자 벗기기 성공");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SpecialBullet"))
        {
            HatOff();
            Debug.Log("모자 벗기기 성공");
        }
    }

    void HatOff()
    {
        monsterScript.SetInvincibility(false);
        monsterScript.isTracking = true;

        // 스프라이트 변경
        if (spriteRenderer != null && hatOffSprite != null)
        {
            spriteRenderer.sprite = hatOffSprite;
        }
    }
}
