using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AgentsBitmask
{
    private Int64 bitmask;
    public Int64 GetBitmask => bitmask;

    public List<AgentName> agents;

    public AgentsBitmask(List<AgentName> agents)
    {
        bitmask = 0;
        foreach(var name in agents)
        {
            bitmask |= (Int64)(1 << (int)name);
        }
    }

    public AgentsBitmask(Int64 _bitmask)
    {
        bitmask = _bitmask;
    }

    public bool HasName(AgentName name)
    {
        Int64 checkBitmask = (Int64)(1 << (int)name);

        return (bitmask & checkBitmask) != 0;
    }

    public void AddName(AgentName name)
    {
        Int64 nameBitmask = (Int64)(1 << (int)name);
        bitmask |= nameBitmask;
    }

    public AgentsBitmask Inverse()
    {
        Int64 invBitmask = ~bitmask;

        return new AgentsBitmask(invBitmask);
    }
}
