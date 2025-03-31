using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    #region Weapon Prefabs
    [Header("Weapon Prefabs")]
    [SerializeField]
    private GameObject ChargeRifle;

    [SerializeField]
    private GameObject Boomerang;

    [SerializeField]
    private GameObject Triump;

    [SerializeField]
    private GameObject Muffin;

    [SerializeField]
    private GameObject BFShotGun;

    [SerializeField]
    private GameObject AlcoholBurner;

    [SerializeField]
    private GameObject MoneyGun;

    [SerializeField]
    private GameObject BounceGun;

    #endregion

    [Header("References")]
    [SerializeField]
    private Transform gunPos;

    private enum WeaponName
    {
        차지라이플,
        부메랑,
        트라이엄프,
        머핀,
        무거운총,
        알코올램프,
        머니건,
        바운스건
    }

    private List<GameObject> weaponList;
    private Queue<GameObject> weaponQue;
    private WeaponName currentWeaponName;
    private GameObject currentWeapon;
    private Vector3 mousePos;

    // 무기 스크립트 캐싱
    private sdh_Gun chargeRifleScript;
    private CHW_BoomerangShooter boomerangScript;
    private LSM_Triump triumpScript;
    private LSM_Muffin muffinScript;
    private LYJ_BFShotGun bfShotGunScript;
    private LYJ_AlcoholBurnerHand alcoholBurnerScript;
    private LYJ_MoneyGun moneyGunScript;

    //private LHG_Bounce_Gun bounceGunScript;

    void Start()
    {
        InitializeWeapons();
        mousePos = GameManager.Instance.MouseManager.GetMousePos();
    }

    void Update()
    {
        FlipGun();
    }

    private void InitializeWeapons()
    {
        int currentStage = GameManager.Instance.stage;
        weaponQue = new Queue<GameObject>();
        weaponList = GetStageWeapons(currentStage);
        SetSelectableWeapon();
    }

    private List<GameObject> GetStageWeapons(int stage)
    {
        return stage switch
        {
            1 => new List<GameObject> { Boomerang },
            2 => new List<GameObject> { },
            3 => new List<GameObject> { Triump, Muffin },
            4 => new List<GameObject> { BFShotGun, AlcoholBurner, MoneyGun },
            5 => new List<GameObject> { ChargeRifle },
            _ => new List<GameObject>()
        };
    }

    private void SetSelectableWeapon()
    {
        ClearWeapons();

        foreach (GameObject weapon in weaponList)
        {
            GameObject newWeapon = Instantiate(weapon, gunPos.position, gunPos.rotation, gunPos);
            // 게임오브젝트는 활성화 상태로 유지
            newWeapon.SetActive(true);

            // 첫 번째 무기를 제외한 나머지의 스프라이트만 비활성화
            if (weaponQue.Count > 0)
            {
                newWeapon.GetComponent<SpriteRenderer>().enabled = false;
            }
            weaponQue.Enqueue(newWeapon);
        }

        if (weaponQue.Count > 0)
        {
            ChangeWeapon();
        }
    }

    private void ClearWeapons()
    {
        if (currentWeapon != null)
            Destroy(currentWeapon);

        foreach (var weapon in weaponQue)
        {
            if (weapon != null)
                Destroy(weapon);
        }
        weaponQue.Clear();
    }

    public void UpdateWeapons(int stageNumber)
    {
        weaponList = GetStageWeapons(stageNumber);
        SetSelectableWeapon();
    }

    public void ChangeWeapon()
    {
        if (currentWeapon != null)
        {
            // 이전 무기의 스프라이트만 숨김
            currentWeapon.GetComponent<SpriteRenderer>().enabled = false;
        }

        currentWeapon = weaponQue.Dequeue();
        // 새 무기의 스프라이트만 표시
        currentWeapon.GetComponent<SpriteRenderer>().enabled = true;
        weaponQue.Enqueue(currentWeapon);

        UpdateWeaponScripts();
        SetCurrentWeaponName();
    }

    private void UpdateWeaponScripts()
    {
        chargeRifleScript = currentWeapon.GetComponent<sdh_Gun>();
        boomerangScript = currentWeapon.GetComponent<CHW_BoomerangShooter>();
        triumpScript = currentWeapon.GetComponent<LSM_Triump>();
        muffinScript = currentWeapon.GetComponent<LSM_Muffin>();
        bfShotGunScript = currentWeapon.GetComponent<LYJ_BFShotGun>();
        alcoholBurnerScript = currentWeapon.GetComponent<LYJ_AlcoholBurnerHand>();
        moneyGunScript = currentWeapon.GetComponent<LYJ_MoneyGun>();
    }

    private void SetCurrentWeaponName()
    {
        if (chargeRifleScript != null)
            currentWeaponName = WeaponName.차지라이플;
        else if (boomerangScript != null)
            currentWeaponName = WeaponName.부메랑;
        else if (triumpScript != null)
            currentWeaponName = WeaponName.트라이엄프;
        else if (muffinScript != null)
            currentWeaponName = WeaponName.머핀;
        else if (bfShotGunScript != null)
            currentWeaponName = WeaponName.무거운총;
        else if (alcoholBurnerScript != null)
            currentWeaponName = WeaponName.알코올램프;
        else if (moneyGunScript != null)
            currentWeaponName = WeaponName.머니건;
    }

    #region Weapon Actions
    public void NormalShoot() //mousebuttondown 0
    {
        switch (currentWeaponName)
        {
            case WeaponName.차지라이플:
                chargeRifleScript.Shoot(mousePos);
                break;
            case WeaponName.부메랑:
                boomerangScript.ThrowBoomerang();
                break;
            case WeaponName.트라이엄프:
                triumpScript.Triump_Left_Click();
                break;
            case WeaponName.머핀:
                muffinScript.Shoot();
                break;
            case WeaponName.무거운총:
                bfShotGunScript.Fire();
                break;
            case WeaponName.알코올램프:
                alcoholBurnerScript.ThrowBurner();
                break;
            case WeaponName.머니건:
                moneyGunScript.FireCoin();
                break;
        }
    }

    public void SpecialSkill() //mousebuttondown 1
    {
        switch (currentWeaponName)
        {
            case WeaponName.차지라이플:
                chargeRifleScript.flashRoutineStart();
                break;
            case WeaponName.트라이엄프:
                triumpScript.Triump_Right_Click();
                break;
            case WeaponName.머니건:
                moneyGunScript.FireBuck();
                break;
        }
    }

    public void ChargeSkill() // mousebutton 1
    {
        switch (currentWeaponName)
        {
            case WeaponName.차지라이플:
                chargeRifleScript.Charging();
                break;
        }
    }

    public void ChargeSkillEnd() // mousebutton up 1
    {
        switch (currentWeaponName)
        {
            case WeaponName.차지라이플:
                chargeRifleScript.ShootOut(mousePos);
                break;
        }
    }

    public void ChangeBullet() //left shift
    {
        switch (currentWeaponName)
        {
            case WeaponName.트라이엄프:
                triumpScript.Triump_Left_Shift();
                break;
        }
    }
    #endregion

    void FlipGun()
    {
        Vector3 mousePos = GameManager.Instance.MouseManager.GetMousePos();
        Vector3 gunScale = gunPos.localScale;
        Vector3 gunLocalPos = gunPos.localPosition;

        if (mousePos.x < GameManager.Instance.Player.transform.position.x)
        {
            // 마우스가 왼쪽에 있을 때
            gunScale.x = -1; // 스프라이트 반전
            gunLocalPos.x = -Mathf.Abs(gunLocalPos.x); // 위치도 반전
        }
        else
        {
            // 마우스가 오른쪽에 있을 때
            gunScale.x = 1; // 기본 상태
            gunLocalPos.x = Mathf.Abs(gunLocalPos.x); // 기본 위치
        }

        gunPos.localScale = gunScale;
        gunPos.localPosition = gunLocalPos;
    }
}
