using System;
using UnityEngine;

[Serializable]
public class AgentState<AState>
{
    public AgentState()
    {
        onCell = null;
    }
    public AgentState(HexCell cell, AState state)
    {
        onCell = cell;
        nowState = state;
    }
    
    public HexCell onCell;
    public AState nowState;
}