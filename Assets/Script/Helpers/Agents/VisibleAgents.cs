using Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VisibleAgents 
{
    IAgent eyeAgent;

    private List<IAgent> visibleAgents;

    private AgentsBitmask enemyAgentsBitmask;
    private AgentsBitmask friendAgentsBitmask;

    int seeDist;

    private int lastTimeCheck;

    public List<IAgent> VisibleAgentsList => visibleAgents;

    public VisibleAgents(IAgent agent, AgentsBitmask _enemyAgentsBitmask, int seeDistance)
    {
        lastTimeCheck = 0;
        visibleAgents = new List<IAgent>();

        eyeAgent = agent;

        enemyAgentsBitmask = _enemyAgentsBitmask; 
        friendAgentsBitmask = _enemyAgentsBitmask.Inverse();
        
        seeDist = seeDistance;
        UpdateVisibleAgents();
    }

    public bool IsSeeAgents()
    {
        return UpdateVisibleAgents();
    }


    /*In this function I inverse vector distances,
     because it's sound logical to me if on a runaway dir
     influence more agents that are closer to us*/
    public Vector3 GetRunawayDir()
    {
        if (!UpdateVisibleAgents())
            return Vector3.zero;


        Vector3 runawayDir = Vector3.zero;
        float maxDist = 0.0f;
        float minDist = float.MaxValue;

        //find max-min dists
        foreach (IAgent agent in visibleAgents)
        {
            float dist = Vector3.Distance(agent.GetCell().Position, eyeAgent.GetCell().Position);

            maxDist = Mathf.Max(maxDist, dist);
            minDist = Mathf.Min(minDist, dist);
        }
        
        if(Mathf.Abs(minDist - maxDist) < 0.001f)
        {
            minDist = 0.0f;
            maxDist = 1.0f;
        }

        //get runaway dir
        foreach (IAgent agent in visibleAgents)
        {
            Vector3 lookDir = eyeAgent.GetCell().Position - agent.GetCell().Position;

            float dist = lookDir.magnitude;
            float normDist = (dist - minDist) / (maxDist - minDist);
            float invNormDist = 1.0f - normDist;

            Vector3 invNormDir = lookDir.normalized * invNormDist;
            runawayDir += invNormDir;
        }
        runawayDir.Normalize();

        return runawayDir;
    }

    public IAgent GetNearestAgent()
    {
        if (!UpdateVisibleAgents())
            return null;

        IAgent nearestAgent = null;
        float minDist = float.MaxValue;

        //find max-min dists
        foreach (IAgent agent in visibleAgents)
        {
            float dist = Vector3.Distance(agent.GetCell().Position, eyeAgent.GetCell().Position);
            
            if(dist < minDist)
            {
                minDist = dist;
                nearestAgent = agent;
            }
        }

        return nearestAgent;
    }

    private bool UpdateVisibleAgents()
    {
        if(TimeManager.NowTime != lastTimeCheck)
        {
            visibleAgents = HexMath.FindAgentsInRadius(eyeAgent.GetCell(),
                                                   seeDist,
                                                   friendAgentsBitmask);

            lastTimeCheck = TimeManager.NowTime;
        }

        return visibleAgents != null && visibleAgents.Count != 0;
    }
}
