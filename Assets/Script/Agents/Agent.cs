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
    [SerializeField] private int maxStartInsanity = 100;
    [SerializeField] private float maxStartSpeed = 1.0f;
    private const int minAttitude = -50;
    private const int maxAttitude = 50;
    [Header("Vars")]
    [SerializeField] private bool addRandomnessInMove = false;

    private bool canDoAction;

    bool isChangingStateOnUpdate;
    protected HexGrid hexGrid;
    
    private AgentState<AgentAction>[] states;
    private AgentState<AgentAction> prevAgentState = new AgentState<AgentAction>();
    [NonSerialized] public AgentState<AgentAction> nowAgentState = new AgentState<AgentAction>();

    protected Dictionary<AgentAction, BaseAction<AgentAction>> actionStates = new Dictionary<AgentAction, BaseAction<AgentAction>>();
    private BaseAction<AgentAction> nowAction;

    public bool CanDoAction => canDoAction;
    public HexGrid HexGrid => hexGrid;
    public int MaxStartHP => maxStartHP;
    public int MaxStartEnergy => maxStartEnergy;
    public int MaxStartInsanity => maxStartInsanity;
    public float MaxStartSpeed => maxStartSpeed;

    private void DoAction()
    {
        if (!canDoAction)
            return;

        AgentAction nextStateKey = nowAction.GetNextAction();
        int c = 0;

        while(!nextStateKey.Equals(nowAgentState.actionState)) //to transition in one turn between multiple states
        {
            nowAction.Exit();
            nowAgentState.actionState = nextStateKey;
            nowAction = actionStates[nowAgentState.actionState];
            nowAction.Start();

            if (c > 100)
            {
                Debug.LogError(gameObject.name + " is looping with state transition: \"" +
                                                 nextStateKey.ToString() +
                                                 "\" to \"" +
                                                 nowAction.GetNextAction().ToString() + "\"");
                break;
            }

            nextStateKey = nowAction.GetNextAction();
            c++;
        }

        nowAction.Update();
    }
    public void UpdateStateFuture()
    { 
        states[TimeManager.NowTime - 1] = prevAgentState;

        AgentState<AgentAction> prev = prevAgentState;
        prevAgentState = nowAgentState;

        nowAgentState = new AgentState<AgentAction>(prev);
        DoAction();

        ChangeState(nowAgentState);
    }
    public void UpdateStatePast()
    {
        if (TimeManager.NowTime != 0)
        {
            nowAgentState = states[TimeManager.NowTime - 1];
            if(TimeManager.NowTime != 1)
            {
                prevAgentState = states[TimeManager.NowTime - 2];
            }
            else
            {
                prevAgentState = nowAgentState;
            }
        }

        ChangeState(nowAgentState);
    }

    private void Start()
    {
        AgentStart();


        if (TimeManager.NowTime == 0)
        {
            nowAgentState.lastMoveTime = TimeManager.NowTime;
            nowAgentState.Energy = (byte)maxStartEnergy;
            nowAgentState.HP = (byte)maxStartHP;
            nowAgentState.Insanity = (byte)maxStartInsanity;
            nowAgentState.InsanityPoints = 0;
            nowAgentState.XP = 0;

            prevAgentState = nowAgentState;
        }
        else
        {
            nowAgentState = states[TimeManager.NowTime];
            prevAgentState = states[TimeManager.NowTime - 1];
        }

        

        canDoAction = true;
        isChangingStateOnUpdate = false;
        states = new AgentState<AgentAction>[TimeManager.EndTime]; //total days count

        TimeManager.AddAgent(this);

        nowAction = actionStates[nowAgentState.actionState];
        nowAction.Start();
    }

    protected abstract void AgentStart();
    
    public void Die()
    {
        prevAgentState.onCell.Unit = null;
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

        if (TimeManager.NowTime == 0)
            prevAgentState = nowAgentState;

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

    public HexCell GetCell()
    {
        return nowAgentState.onCell;
    }

    public void SetState(AgentState<AgentAction> state)
    {
        ChangeState(state);
    }

    protected virtual void ChangeState(AgentState<AgentAction> state)
    {
        HexCell nowCell = prevAgentState.onCell;
        HexCell moveCell = state.onCell;
        if (nowCell != moveCell)
        {
            int rand = 0;
            if (addRandomnessInMove)
            {
                rand = UnityEngine.Random.Range(0, 2);
            }
            //check if move is valid
            if (HexMath.CanMove(nowCell, moveCell, (int)Mathf.Ceil((prevAgentState.lastMoveTime + rand) * maxStartSpeed)) )
            {
                ChangeLocation(state.onCell);
                state.lastMoveTime = TimeManager.NowTime;

                if (state.Energy != 0)
                    state.Energy--;
            }
            else
            {
                state.onCell = nowCell;
                state.lastMoveTime = prevAgentState.lastMoveTime;
            }
        }

        canDoAction = (state.Energy != 0);

        if (state.HP == 0)
            Die();

        nowAction = actionStates[state.actionState];
        prevAgentState = state;
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
        int nowHP = (int)nowAgentState.HP + points;

        if (nowHP < 0)
            Die();

        if (nowHP > maxStartHP)
            nowHP = maxStartHP;
        
        nowAgentState.HP = (byte)(nowHP);
    }

    public void ChangeEnergy(int points)
    {
        int nowEn = (int)(nowAgentState.Energy) + points;
        Debug.Log(nowEn);

        if (nowEn <= 0)
        {
            canDoAction = false;
            nowEn = 0;
        }
        else
        {
            canDoAction = true;
        }

        if (nowEn > maxStartEnergy)
            nowEn = maxStartEnergy;
        
        nowAgentState.Energy = (byte)(nowEn);
    }

    public void ChangeInsanity(int points)
    {
        int nowIns = (int)nowAgentState.Insanity + points;
        
        if (nowIns > maxStartInsanity)
        {
            nowAgentState.InsanityPoints++;
            nowIns = 0;
        }
            
        nowAgentState.Insanity = (byte)(nowIns);
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
        byte energy = reader.ReadByte();
        byte XP = reader.ReadByte();
        byte insanity = reader.ReadByte();

        AgentState<AgentAction> state = new AgentState<AgentAction>
                                            (
                                            cell, 
                                            nowAgentState.actionState
                                            );

        state.Energy = energy;
        state.XP = XP;
        state.Insanity = insanity;
        states[time] = state;
    }
}
