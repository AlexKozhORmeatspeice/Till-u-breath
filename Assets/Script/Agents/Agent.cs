using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
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
    [Header("Max values")]
    [SerializeField] private int maxStartHP = 100;
    [SerializeField] private int maxStartEnergy = 100;
    [SerializeField] private int maxStartMentality = 100;
    private const int minAttitude = -50;
    private const int maxAttitude = 50;
    [Header("Vars")]
    [SerializeField] private bool addRandomnessInMove = false;

    private bool canDoAction;
    public bool CanDoAction => canDoAction;

    bool isChangingStateOnUpdate;
    protected HexGrid hexGrid;
    public HexGrid HexGrid => hexGrid;
    
    private Dictionary<int, AgentState<AgentAction>> states;
    private AgentState<AgentAction> nowAgentState = new AgentState<AgentAction>();

    public AgentState<AgentAction> NowAgentState => nowAgentState;

    protected Dictionary<AgentAction, BaseAction<AgentAction>> actionStates = new Dictionary<AgentAction, BaseAction<AgentAction>>();
    protected BaseAction<AgentAction> nowAction;
    private AgentState<AgentAction> DoAction()
    {
        if (!canDoAction)
            return nowAgentState;

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
        if (TimeManager.NowTime == 0)
        {
            if (nowAgentState == null)
            {
                nowAgentState = new AgentState<AgentAction>();
            }
            else
            {
                nowAgentState.lastMoveTime = TimeManager.NowTime;
            }
        }
        canDoAction = true;
        isChangingStateOnUpdate = false;

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
        HexCell nowCell = nowAgentState.onCell;
        HexCell moveCell = state.onCell;
        if (nowCell != moveCell)
        {
            int rand = 0;
            if (addRandomnessInMove)
            {
                rand = UnityEngine.Random.Range(0, 2);
            }
            //check if move is valid
            if (HexMath.CanMove(nowCell, moveCell, nowAgentState.lastMoveTime + rand))
            {
                ChangeLocation(state.onCell);
                state.lastMoveTime = TimeManager.NowTime;
                state.Energy -= 1;
            }
            else
            {
                state.onCell = nowCell;
                state.lastMoveTime = nowAgentState.lastMoveTime;
            }
        }
        
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
        isChangingStateOnUpdate = true;
        AgentAction nextStateKey = nowAction.GetNextActionOnFrameUpdate();
        if (!nextStateKey.Equals(nowAction.StateKey))
        {
            nowAction.Exit();
            nowAction = actionStates[nextStateKey];
            nowAction.Start();
        }
        isChangingStateOnUpdate = false;
    }

    public void ChangeHP(int points)
    {
        nowAgentState.HP = (short)(nowAgentState.HP - points);

        if (nowAgentState.HP < 0)
            Die();

        if (nowAgentState.HP > maxStartHP)
            nowAgentState.HP = (short)maxStartHP;
    }

    public void ChangeEnergy(int points)
    {
        nowAgentState.Energy = (short)(nowAgentState.Energy - points);

        if (nowAgentState.Energy <= 0)
        {
            canDoAction = false;
            nowAgentState.Energy = 0;
        }
        else
        {
            canDoAction = true;
        }

        if (nowAgentState.Energy > maxStartEnergy)
            nowAgentState.Energy = (short)maxStartEnergy;
    }

    public void ChangeInsanity(int points)
    {
        nowAgentState.Insanity = (short)(nowAgentState.Insanity - points);
        
        if (nowAgentState.Insanity > maxStartMentality)
        {
            nowAgentState.InsanityPoints++;
            nowAgentState.Insanity = 0;
        }
            
    }

    public void ChangeAttitude(IAgent agent, int points)
    {
        nowAgentState.attitudeTo[agent] += (short)points;

        if (nowAgentState.attitudeTo[agent] < minAttitude)
            nowAgentState.attitudeTo[agent] = minAttitude;

        if (nowAgentState.attitudeTo[agent] > maxAttitude)
            nowAgentState.attitudeTo[agent] = maxAttitude;
    }

    public virtual void SaveState(BinaryWriter writer, int time)
    {
        AgentState<AgentAction> state = states[time];

        writer.Write(hexGrid.GetCellID(state.onCell));
        writer.Write((byte)state.Energy);
        writer.Write((byte)state.XP);
        writer.Write((byte)state.Insanity);
        //writer.Write((int)state.AttitudeToHero);
    }

    public virtual void LoadState(BinaryReader reader, int time)
    {
        HexCell cell = hexGrid.GetCellByID(reader.ReadInt32());
        short energy = reader.ReadByte();
        short XP = reader.ReadByte();
        short insanity = reader.ReadByte();
      //short AttitudeToHero = (short)reader.ReadInt32();

        AgentState<AgentAction> state = new AgentState<AgentAction>
                                            (
                                            cell, 
                                            nowAgentState.nowAction
                                            );
        state.Energy = energy;
        state.XP = XP;
        state.Insanity = insanity;
      //state.AttitudeToHero = AttitudeToHero;
        states[time] = state;
    }
}
