using System;
using UnityEngine;

[Serializable]
public struct AgentState
{
    public AgentState(HexCell cell)
    {
        onCell = cell;
    }
    
    public HexCell onCell;
}