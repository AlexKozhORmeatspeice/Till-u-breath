using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private int endTime = 10;
    [SerializeField] private int startTime = 0;
    [SerializeField] private TMP_Text txt;
    private static int nowTime;
    public static int NowTime => nowTime;
    
    private static List<IAgent> agents = new List<IAgent>();

    public void BackToFuture(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if(nowTime + 1 > endTime)
                return;

            nowTime++;
            txt.text = nowTime.ToString();
            UpdateAgentsFuture();    
        }
    }
    
    public void BackToPast(int n)
    {
        for (int i = 0; i < n; i++)
        {
            if (nowTime - 1 < startTime)
                return;

            nowTime--;
            txt.text = nowTime.ToString();
            UpdateAgentsPast();
        } 
    }
    
    private void UpdateAgentsFuture()
    {
        foreach (var agent in agents)
        {
            agent.UpdateStateFuture();
        }
    }
    
    private void UpdateAgentsPast()
    {
        foreach (var agent in agents)
        {
            agent.UpdateStatePast();
        }
    }
    public static void AddAgent(IAgent agent)
    {
        agents.Add(agent);
    }

    public static void RemoveAgent(IAgent agent)
    {
        agents.Remove(agent);
    }
    
    private void Awake()
    {
        nowTime = 0;
        if (agents.Count != 0)
            agents.Clear();
    }
}
