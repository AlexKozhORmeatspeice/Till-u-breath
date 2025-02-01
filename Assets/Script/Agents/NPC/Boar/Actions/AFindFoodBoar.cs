using Script;
using UnityEngine;

class AFindFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    
    public override void Start()
    {
        boar = agent.gameObject.GetComponent<Boar>();

        boar.searchFood = HexMath.FindFoodInRadius(boar.SearchFoodRadius, agent.nowAgentState.onCell);
    }

    public override void Update()
    {

    }

    public override Boar.BoarActions GetNextAction()
    {
        if(agent.nowAgentState.HP == agent.MaxStartHP && agent.nowAgentState.Energy == agent.MaxStartEnergy)
        {
            boar.searchFood = null;
            return Boar.BoarActions.walk;
        }

        if(boar.searchFood != null)
        {
            return Boar.BoarActions.walkForFood;
        }
        else
        {
            return Boar.BoarActions.digForFood;
        }
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
    public AFindFoodBoar(Boar.BoarActions key, Agent<Boar.BoarActions> nowAgent) : base(key, nowAgent)
    {
    }
}

