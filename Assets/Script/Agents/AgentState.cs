using System;
using UnityEngine;

[Serializable]
public class AgentState<AgentAction>
{
    public AgentState()
    {
        onCell = null;
    }
    public AgentState(HexCell cell, AgentAction state)
    {
        onCell = cell;
        nowAction = state;
    }
    
    public HexCell onCell;
    public AgentAction nowAction;
}