using System.Collections.Generic;
using UnityEngine;

public class LYJ_UIManager : MonoBehaviour
{
    public enum UIType { WeaponSlot, HP }

[SerializeField]
    Sprite banWeaponIcon;
[SerializeField]
    Sprite selectWeaponIcon;
[SerializeField]
    Sprite normalWeaponIcon;
    List<GameObject> Hearts;
    GameObject HeartIcon;
    public UIType type;

    void Awake()
    {
        switch (type)
        {
            case UIType.HP:
                Hearts = new List<GameObject>();
                for (int i = 0; i < GameManager.Instance.Player.BasePlayerHp; ++i)
                {
                    GameObject heart = Instantiate(HeartIcon, transform);
                    Hearts.Add(heart);
                }
                SetHpUI();
                break;
            // case UIType.WeaponSlot:
            //     break;
        }

    }

    public void SetHpUI()
    {
        foreach (var heart in Hearts)
        {
            heart.SetActive(false);
        }

        for (int i = 0; i < GameManager.Instance.Player.CurrentPlayerHp; ++i)
        {
            Hearts[i].SetActive(true);
        }
    }

    public void BanWeaponUI(int stage)
    {
        for (int i = 0; i < 8; ++i)
        {
            transform.GetChild(i).GetComponent<SpriteRenderer>().sprite = banWeaponIcon;
        }

        switch (stage)
        {
            case 1:
                transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                break;
            case 2:
                transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                break;
            case 3:
                transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                break;
            case 4:
                transform.GetChild(4).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                transform.GetChild(5).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                transform.GetChild(6).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                break;
            case 5:
                transform.GetChild(7).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
                break;
        }
    }

    public void SelectWeaponUI(int previousWeaponNo,int currentWeaponNo)
    {
        transform.GetChild(previousWeaponNo+1).GetComponent<SpriteRenderer>().sprite = normalWeaponIcon;
        transform.GetChild(currentWeaponNo+1).GetComponent<SpriteRenderer>().sprite = selectWeaponIcon;
    }
    
}
