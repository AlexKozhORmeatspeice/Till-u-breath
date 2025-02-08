using Script;
using System.Linq.Expressions;
using UnityEngine;

class AWalkForFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    CellRoad road;

    public override void Start()
    {
        road = null;
        boar = agent.gameObject.GetComponent<Boar>();
    }

    public override void Update()
    {
        HexCell nowCell = agent.nowAgentState.onCell;

        road = HexMath.FindPath(nowCell, boar.searchFood.OnCell, boar.CellsToMoveBitmask.Inverse());

        if (road == null)
        {
            return;
        }

        HexCell newCell = road.Pop();
        agent.nowAgentState.onCell = newCell;
    }

    public override Boar.BoarActions GetNextAction()
    {
        if(boar.searchFood == null || !boar.searchFood.isActiveAndEnabled)
        {
            boar.searchFood = null;
            return Boar.BoarActions.findFood;
        }

        if(boar.searchFood.Use(agent))
        {
            return Boar.BoarActions.findFood;
        }

        if (boar.SeeAgents.IsSeeAgents() &&
           agent.nowAgentState.HP > agent.MaxStartHP / 2 &&
           agent.nowAgentState.Energy > agent.MaxStartEnergy / 4)
        {
            return Boar.BoarActions.runFromAgents;
        }

        return Boar.BoarActions.walkForFood;
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

    public AWalkForFoodBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}

