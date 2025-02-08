using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct AgentState<AgentAction>
{

    public AgentState(HexCell cell, AgentAction state)
    {
        if (cell == null)
        {
            cellInd = short.MaxValue;
        }
        else
        {
            cellInd = (short)HexGrid.Instance.GetCellID(cell);
        }

        actionState = state;

        lastMoveTime = TimeManager.NowTime;
        HP = 100;
        Energy = 100;
        Insanity = 0;
        XP = 0;
        InsanityPoints = 0;
    }

    public AgentState(AgentState<AgentAction> otherAgent)
    {
        if (otherAgent.onCell == null)
        {
            Debug.Log(1);
            cellInd = short.MaxValue;
        }
        else
        {
            cellInd = (short)HexGrid.Instance.GetCellID(otherAgent.onCell);
        }

        actionState = otherAgent.actionState;

        lastMoveTime = otherAgent.lastMoveTime;
        HP = otherAgent.HP;
        Energy = otherAgent.Energy;
        Insanity = otherAgent.Insanity;
        XP = otherAgent.XP;
        InsanityPoints = otherAgent.InsanityPoints;
    }

    private short cellInd;

    public AgentAction actionState;

    public int lastMoveTime;

    public byte HP;
    public byte Energy;
    public byte Insanity;
    public byte InsanityPoints;
    public byte XP;

    public HexCell onCell
    {
        get
        {
            if (cellInd == short.MaxValue)
                return null;

            return HexGrid.Instance.GetCellByID(cellInd);
        }
        set
        {
            if (value == null)
            {
                cellInd = short.MaxValue;
                return;
            }

            cellInd = (short)HexGrid.Instance.GetCellID(value);
        }
    }
}