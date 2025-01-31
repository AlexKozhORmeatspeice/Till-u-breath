using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Food : Item
{
    [Header("Vals")]
    [SerializeField] private int GetHP = 1;
    [SerializeField] private int GetEN = 1;
    [SerializeField] private int liveTime = 20;
    [Header("Randomness")]
    [SerializeField] private Vector2Int BordRandInLiveTime = Vector2Int.zero; 
    private int spawnTime;
    private int randInLiveTime = 0;
    public override void Use(IAgent agent)
    {
        agent.ChangeHP(GetHP);
        agent.ChangeEnergy(GetEN);

        Destroy(this);
    }

    public override void Use(HexCell cell)
    {
        Drop(cell);
    }

    public override void OnObjectSpawn()
    {
        randInLiveTime = Random.Range(Mathf.Min(BordRandInLiveTime.x, BordRandInLiveTime.y),
                                      Mathf.Max(BordRandInLiveTime.x, BordRandInLiveTime.y));

        spawnTime = TimeManager.NowTime;
    }

    private void Update()
    {
        if(TimeManager.NowTime - spawnTime > liveTime)
        {
            Destroy(gameObject);
        }
    }
}
