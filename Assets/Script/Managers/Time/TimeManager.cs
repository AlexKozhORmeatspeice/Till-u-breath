using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] private float timeBetweenMinuts = 0.2f;
    [SerializeField] private int endTime = 50;
    [SerializeField] private int startTime = 0;
    [SerializeField] private TMP_Text txt;
    private static int nowTime;
    public static int NowTime => nowTime;
    
    private static List<IAgent> agents = new List<IAgent>();

    private bool timeIsGoing;
    private bool isInCoroutine;

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
        if (timeIsGoing)
            return;

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

    private void LateUpdate()
    {
        //activate time
        if(Input.GetKeyDown(KeyCode.Space))
        {
            timeIsGoing = !timeIsGoing;
        }

        //manipualtion with time
        if(timeIsGoing && !isInCoroutine)
        {
            StartCoroutine(UpdateTime());
        }

        if(!timeIsGoing)
        {
            isInCoroutine = false;
            StopCoroutine(UpdateTime());
        }
    }

    IEnumerator UpdateTime()
    {
        isInCoroutine = true;

        BackToFuture(1);
        yield return new WaitForSeconds(timeBetweenMinuts);
        isInCoroutine = false;
    }

    private void Awake()
    {
        timeIsGoing = false;
        nowTime = 0;

        if (agents.Count != 0)
            agents.Clear();
    }
}
