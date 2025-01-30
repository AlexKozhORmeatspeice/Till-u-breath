using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AgentState<AgentAction>
{
    public AgentState()
    {
        onCell = null;
        
        lastMoveTime = TimeManager.NowTime;
        HP = 100;
        Energy = 100;
        Insanity = 100;
        XP = 0;
    }
    public AgentState(HexCell cell, AgentAction state)
    {
        onCell = cell;
        nowAction = state;

        lastMoveTime = TimeManager.NowTime;
        HP = 100;
        Energy = 100;
        Insanity = 100;
        XP = 0;
    }
    
    public HexCell onCell;
    public AgentAction nowAction;

    public int lastMoveTime;

    public short HP;
    public short Energy;
    public short Insanity;
    public short InsanityPoints;
    public Dictionary<IAgent, short> attitudeTo;
    public short XP;
}