using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//in children classes should be defined groups of actions and their transitions only
public abstract class Agent<AState> : MonoBehaviour, IAgent where AState : Enum
{
    private Stack<AgentState> states;
    private AgentState nowAgentState;
    public AgentState NowAgentState => nowAgentState;

    protected Dictionary<AState, BaseAction<AState>> actionStates = new Dictionary<AState, BaseAction<AState>>();
    protected BaseAction<AState> nowAction;

    public AgentState DoAction()
    {
        AState nextStateKey = nowAction.GetNextState();

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
        states.Push(nowAgentState);
        nowAgentState = DoAction();

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

    protected virtual void Start()
    {
        TimeManager.AddAgent(this);
        states = new Stack<AgentState>();
        
        nowAction.Start();
    }

    public void Die()
    {
        nowAgentState.onCell.Unit = null;
        Destroy(gameObject);
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

    private void ChangeState(AgentState state)
    {
        transform.localPosition = nowAgentState.onCell.Position;
    }
    
    private void LoadStates()
    {
        
    }

    private void SaveState()
    {
        
    }
}
