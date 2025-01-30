using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Food : Item
{
    [SerializeField] private int GetHP = 1;
    [SerializeField] private int GetEN = 1;
    [SerializeField] private int liveTime = 20;
    private int spawnTime;
    public override void Use(IAgent agent)
    {
        agent.ChangeHP(GetHP);
        agent.ChangeEnergy(GetEN);
    }

    public override void Use(HexCell cell)
    {
        Drop(cell);
    }

    private void Start()
    {
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
