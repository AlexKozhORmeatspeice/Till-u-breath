using Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField][Range(40, 200)] private int timeToGrowFood = 50;
    [SerializeField][Range(1, 30)] private int randomTimePlus;
    [SerializeField][Range(0.0f, 1.0f)] private float chanceOfBecomeSpawner = 0.1f;
    [SerializeField][Range(1, 6)] private int maxFoodOnSpawner; 
    [Header("Prefabs")]
    [SerializeField] private PoolObjName foodName;
    
    
    private bool isSpawner;
    private HexCell spawnCell;
    private int lastTimeSpawn;
    // Start is called before the first frame update
    void Start()
    {
        float chance = Random.Range(0.0f, 1.0f);
        isSpawner = chance < chanceOfBecomeSpawner;

        HexGrid hexGrid = GetComponentInParent<HexGrid>();
        spawnCell = hexGrid.GetCell(transform.position);

        lastTimeSpawn = TimeManager.NowTime;
        timeToGrowFood += Random.Range(-randomTimePlus, randomTimePlus);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSpawner)
            return;

        if(TimeManager.NowTime - lastTimeSpawn > timeToGrowFood)
        {
            Food food = Pooler.Instance.SpawnPoolObject(foodName, Vector3.zero, Quaternion.identity).GetComponent<Food>();

            food.Drop(spawnCell);
            lastTimeSpawn = TimeManager.NowTime;
        }
    }
}
