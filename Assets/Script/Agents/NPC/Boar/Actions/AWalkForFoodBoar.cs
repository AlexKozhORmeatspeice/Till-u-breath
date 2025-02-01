using Script;
using UnityEngine;

class AWalkForFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    CellRoad road;

    public override void Start()
    {
        road = null;
        boar = agent.gameObject.GetComponent<Boar>();

        Debug.Log("Walking for food");
    }

    public override void Update()
    {
        HexCell nowCell = agent.nowAgentState.onCell;

        road = HexMath.FindPath(nowCell, boar.searchFood.OnCell);

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
            return Boar.BoarActions.findFood;
        }

        if(agent.nowAgentState.onCell == boar.searchFood.OnCell)
        {
            boar.searchFood.Use(agent);
            return Boar.BoarActions.findFood;
        }

        return agent.nowAgentState.actionState;
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

