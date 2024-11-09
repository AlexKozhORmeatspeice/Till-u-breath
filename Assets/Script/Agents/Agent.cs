using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//in children classes should be defined groups of actions

/// <summary>
/// To set agent:
/// 1. Create new class instance
/// 2. Override Start() and don't forget to set all actions AND BASE AGENT STATE (var nowAgentState)
/// 3. If Agent got some specific State Var don't forget to override ChangeState() with base.ChangeState()
/// 4. You're awesome!
/// </summary>
public abstract class Agent<AgentAction> : MonoBehaviour, IAgent where AgentAction : Enum
{
    bool changeStateOnUpdate;
    protected HexGrid hexGrid;
    public HexGrid HexGrid => hexGrid;
    
    private Dictionary<int, AgentState<AgentAction>> states;
    private AgentState<AgentAction> nowAgentState = new AgentState<AgentAction>();

    public AgentState<AgentAction> NowAgentState => nowAgentState;

    protected Dictionary<AgentAction, BaseAction<AgentAction>> actionStates = new Dictionary<AgentAction, BaseAction<AgentAction>>();
    protected BaseAction<AgentAction> nowAction;
    private AgentState<AgentAction> DoAction()
    {
        AgentAction nextStateKey = nowAction.GetNextAction();
        if (!nextStateKey.Equals(nowAction.StateKey))
        {
            nowAction.Exit();
            nowAction = actionStates[nextStateKey];
            nowAction.Start();
        }
        return nowAction.Update();
    }
    public void UpdateStateFuture()
    {
        states[TimeManager.NowTime - 1] = nowAgentState;
        AgentState<AgentAction> state = DoAction();
        ChangeState(state);
    }
    public void UpdateStatePast()
    {
        if (states.Count != 0)
        {
            nowAgentState = states[TimeManager.NowTime];
        }

        ChangeState(nowAgentState);
    }

    protected virtual void Start() // if you override Start() remember to put base.Start() LAST and ONLY THE LAST 
    {
        changeStateOnUpdate = false;

        if (nowAgentState == null)
            nowAgentState = new AgentState<AgentAction>();

        TimeManager.AddAgent(this);
        states = new Dictionary<int, AgentState<AgentAction>>();
        
        nowAction.Start();
    }
    
    public void Die()
    {
        nowAgentState.onCell.Unit = null;
        Destroy(gameObject);
    }

    public void SetGrid(HexGrid grid)
    {
        hexGrid = grid;
    }

    public void ChangeLocation(HexCell cell)
    {
        HexCell cellNow = nowAgentState.onCell;
        if(cellNow != null)
            cellNow.Unit = null;
        
        nowAgentState.onCell = cell;

        transform.localPosition = cell.Position;
        cell.Unit = this;
    }

    public void ValidateLocation()
    {
        transform.localPosition = nowAgentState.onCell.Position;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void SetState(AgentState<AgentAction> state)
    {
        ChangeState(state);
    }
    protected virtual void ChangeState(AgentState<AgentAction> state)
    {
        ChangeLocation(state.onCell);
        
        nowAction = actionStates[state.nowAction];
        nowAgentState = state;
    }

    private void Update()
    {
        nowAction.OnFrameUpdate();
        OnFrameUpdateAction();
    }

    private void OnFrameUpdateAction()
    {
        changeStateOnUpdate = true;
        AgentAction nextStateKey = nowAction.GetNextActionOnFrameUpdate();
        if (!nextStateKey.Equals(nowAction.StateKey))
        {
            nowAction.Exit();
            nowAction = actionStates[nextStateKey];
            nowAction.Start();
        }
        changeStateOnUpdate = false;
    }
    public virtual void SaveState(BinaryWriter writer, int time)
    {
        AgentState<AgentAction> state = states[time];

        writer.Write(hexGrid.GetCellID(state.onCell));
    }

    public virtual void LoadState(BinaryReader reader, int time)
    {
        HexCell cell = hexGrid.GetCellByID(reader.ReadInt32());

        AgentState<AgentAction> state = new AgentState<AgentAction>
                                            (
                                            cell, 
                                            nowAgentState.nowAction
                                            );

        states[time] = state;
    }

}
