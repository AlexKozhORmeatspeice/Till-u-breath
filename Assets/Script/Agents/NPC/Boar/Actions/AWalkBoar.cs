using Script;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class AWalkBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    public override void Start()
    {
        boar = agent.gameObject.GetComponent<Boar>();
    }

    public override void Update()
    {
        HexDirection dir = (HexDirection)(Random.Range(0, 6));
        
        HexCell nowCell  = agent.nowAgentState.onCell;
        HexCell moveCell = agent.nowAgentState.onCell.GetNeighbor(dir);
        
        if(moveCell != null && HexMath.FindPath(nowCell, moveCell, boar.CellsToMoveBitmask.Inverse()) != null)
        {
            agent.nowAgentState.onCell = moveCell;
        }
    }

    public override Boar.BoarActions GetNextAction()
    {
        if(agent.nowAgentState.Energy <= agent.MaxStartEnergy / 2 || agent.nowAgentState.HP < agent.MaxStartHP)
        {
            return Boar.BoarActions.findFood;
        }

        if(boar.SeeAgents.IsSeeAgents())
        {
            return Boar.BoarActions.runFromAgents;
        }

        return Boar.BoarActions.walk;
    }

    public override Boar.BoarActions GetNextActionOnFrameUpdate()
    {
        return agent.nowAgentState.actionState;
    }

    public override void OnFrameUpdate()
    {
        //
    }

    public override void Exit()
    {
        //
    }

    public AWalkBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}

