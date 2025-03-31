using System.Collections.Generic;
using UnityEngine;

public class LYJ_SpawnManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> enemies;
    List<Transform> spawnPoints;
    float waveInterval = 30f;
    int currentWave = 0;
    public int CurrentWave => currentWave;

    void Awake()
    {
        spawnPoints = new List<Transform>();
        foreach (var item in enemies)
        {
            PoolManager.Instance.CreatePool(item, 10);
        }
        for (int i = 0; i < transform.childCount; ++i)
        {
            spawnPoints.Add(transform.GetChild(i));
        }
        StartNewWave();
    }
    void Update()
    {
        if (currentWave > 5) { return; }
        if (GameManager.Instance.CurrentStageTime % waveInterval == 0)
        {
            StartNewWave();
        }
        if (CurrentWave > 5 && GameObject.FindGameObjectWithTag("Monster") == null)
        {
            GameManager.Instance.ClearScene();
        }
    }

    void StartNewWave()
    {
        for (int i = 0; i < 5+(3*currentWave); ++i)
        {
            int randomPointNo = Random.Range(0, spawnPoints.Count);
            GameObject enemy = PoolManager.Instance.GetGameObject(enemies[i]);
            enemy.transform.position = spawnPoints[randomPointNo].position;
        }
        currentWave++;
    }
}
