using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//in children classes should be defined groups of actions
public abstract class Agent<AState> : MonoBehaviour, IAgent where AState : Enum
{
    protected HexGrid hexGrid;
    public HexGrid HexGrid => hexGrid;
    
    private Stack<AgentState<AState>> states;
    private AgentState<AState> nowAgentState = new AgentState<AState>();

    public AgentState<AState> NowAgentState => nowAgentState;

    protected Dictionary<AState, BaseAction<AState>> actionStates = new Dictionary<AState, BaseAction<AState>>();
    protected BaseAction<AState> nowAction;

    private AgentState<AState> DoAction()
    {
        AState nextStateKey = nowAction.GetNextState();
        Debug.Log(1);
        if (!nextStateKey.Equals(nowAction.StateKey))
        {
            Debug.Log(2);
            nowAction.Exit();
            Debug.Log(3);
            nowAction = actionStates[nextStateKey];
            Debug.Log(4);
            nowAction.Start();
        }
        Debug.Log(5);
        return nowAction.Update();
    }
    public void UpdateStateFuture()
    {
        states.Push(nowAgentState);
        nowAgentState = DoAction();
        Debug.Log(6);
        ChangeState(nowAgentState);
    }
    public void UpdateStatePast()
    {
        if (states.Count != 0)
        {
            nowAgentState = states.Pop();
        }

        ChangeState(nowAgentState);
    }

    protected virtual void Start() // if you override Start() remember to put base.Start() LAST and ONLY THE LAST 
    {
        if(nowAgentState == null)
            nowAgentState = new AgentState<AState>();
        
        TimeManager.AddAgent(this);
        states = new Stack<AgentState<AState>>();
        
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

    private void ChangeState(AgentState<AState> state)
    {
        transform.localPosition = nowAgentState.onCell.Position;
        nowAction = actionStates[nowAgentState.nowState];
    }

    private void LoadStates()
    {
        
    }

    private void SaveState()
    {
        
    }
}
