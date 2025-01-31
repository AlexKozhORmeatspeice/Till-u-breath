using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        InsanityPoints = 0;
        attitudeTo = new Dictionary<IAgent, short>();
    }

    public AgentState(AgentState<AgentAction> otherAgent)
    {
        onCell = otherAgent.onCell;
        actionState = otherAgent.actionState;

        lastMoveTime = otherAgent.lastMoveTime;
        HP = otherAgent.HP;
        Energy = otherAgent.Energy;
        Insanity = otherAgent.Insanity;
        XP = otherAgent.XP;
        InsanityPoints = otherAgent.InsanityPoints;
        attitudeTo = new Dictionary<IAgent, short>(otherAgent.attitudeTo);
    }

    public AgentState(HexCell cell, AgentAction state)
    {
        onCell = cell;
        actionState = state;

        lastMoveTime = TimeManager.NowTime;
        HP = 100;
        Energy = 100;
        Insanity = 100;
        XP = 0;
        InsanityPoints = 0;
        attitudeTo = new Dictionary<IAgent, short>();
    }
    
    public HexCell onCell;
    public AgentAction actionState;

    public int lastMoveTime;

    public short HP;
    public short Energy;
    public short Insanity;
    public short InsanityPoints;
    public Dictionary<IAgent, short> attitudeTo;
    public short XP;
}