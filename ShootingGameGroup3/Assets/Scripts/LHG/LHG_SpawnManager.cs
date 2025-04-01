using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LHG_SpawnManager : MonoBehaviour
{
    public GameObject monsterPrefab1; // ������ ù ��° ���� ������
    public GameObject monsterPrefab2; // ������ �� ��° ���� ������
    public float spawnInterval = 2f; // ���� ����
    private float timer;
    private int monsterCount = 0; // ���� ���� ��
    private int currentStage = 0; // ���� ��������
    private int[] stageMonsterCounts = { 3, 6, 9 }; // �� �������������� �ִ� ���� ��
    private int[] stageHealth = { 1, 2, 3 }; // �� �������������� ���� ü��
    private bool allMonstersDefeated = false; // ��� ���Ͱ� ó���Ǿ����� ����

    public Text gameOverText; // ���� ���� �ؽ�Ʈ UI

    private void Start()
    {
        // ���� ���� �ؽ�Ʈ�� ó���� ��Ȱ��ȭ
        gameOverText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Ÿ�̸� ������Ʈ
        timer += Time.deltaTime;

        // ���� ������ ������ ���� ���� ���� �ִ� ������ ������ ���� ����
        if (
            timer >= spawnInterval
            && monsterCount < GetCurrentMaxMonsters()
            && !allMonstersDefeated
        )
        {
            SpawnMonster();
            timer = 0f; // Ÿ�̸� ����
        }

        // ��� ���Ͱ� ó���Ǿ����� üũ
        if (monsterCount >= GetCurrentMaxMonsters() && !allMonstersDefeated)
        {
            allMonstersDefeated = true;
            StartCoroutine(ProceedToNextStage());
        }
    }

    void SpawnMonster()
    {
        // �������� ���� ������ ����
        GameObject selectedMonsterPrefab =
            Random.Range(0, 2) == 0 ? monsterPrefab1 : monsterPrefab2;

        // ���õ� ���� �������� ����
        GameObject monster = Instantiate(
            selectedMonsterPrefab,
            transform.position,
            Quaternion.identity
        );

        // ������ ü���� ���� ���������� �°� ����
        LHG_Monster monsterScript = monster.GetComponent<LHG_Monster>();
        if (monsterScript != null)
        {
            monsterScript.health = GetCurrentMonsterHealth(); // ���� ���������� �´� ü�� ����
        }

        monsterCount++; // ���� �� ����
    }

    private int GetCurrentMaxMonsters()
    {
        // ���� ���������� �´� �ִ� ���� �� ��ȯ
        return stageMonsterCounts[currentStage];
    }

    private int GetCurrentMonsterHealth()
    {
        // ���� ���������� �´� ���� ü�� ��ȯ
        return stageHealth[currentStage];
    }

    public void IncreaseStage()
    {
        // �������� ����
        if (currentStage < stageMonsterCounts.Length - 1)
        {
            currentStage++;
        }
        else
        {
            // ��� ���������� ����� ��� ���� ���� �ؽ�Ʈ ǥ��
            ShowGameOverText();
        }
    }

    private IEnumerator ProceedToNextStage()
    {
        // 5�� ���
        yield return new WaitForSeconds(5f);

        // ���� ���������� �����ϴ� ���� �߰�
        Debug.Log("���� ���������� �����մϴ�.");
        IncreaseStage(); // �������� ����
        allMonstersDefeated = false; // ���� ó�� ���� ����
        monsterCount = 0; // ���� �� ����
    }

    private void ShowGameOverText()
    {
        // ���� ���� �ؽ�Ʈ Ȱ��ȭ
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = "���� ����!"; // �ؽ�Ʈ ����
    }
}
