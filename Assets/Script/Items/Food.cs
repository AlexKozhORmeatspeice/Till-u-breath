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
    public override void OnObjectSpawn()
    {
        randInLiveTime = Random.Range(Mathf.Min(BordRandInLiveTime.x, BordRandInLiveTime.y),
                                      Mathf.Max(BordRandInLiveTime.x, BordRandInLiveTime.y));

        spawnTime = TimeManager.NowTime;
    }
    public override bool Use(IAgent agent)
    {
        if(OnCell != null && HexMath.Distance(agent.GetCell(), OnCell) > 1)
        {
            return false;
        }

        agent.ChangeHP(GetHP);
        agent.ChangeEnergy(GetEN);

        gameObject.SetActive(false);
        return true;
    }

    public override bool Use(HexCell cell)
    {
        Drop(cell);
        return true;
    }


    private void Update()
    {
        if(TimeManager.NowTime - spawnTime > liveTime)
        {
            gameObject.SetActive(false);
        }
    }
}
