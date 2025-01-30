using Script;
using UnityEngine;

class AFindFoodBoar : BaseAction<Boar.BoarActions>
{
    Boar boar;
    
    public override void Start()
    {
        boar = agent.gameObject.GetComponent<Boar>();
    }

    public override AgentState<Boar.BoarActions> Update()
    {
        HexDirection dir = (HexDirection)(Random.Range(0, 5));
        HexCell moveCell = agent.NowAgentState.onCell.GetNeighbor(dir);
           
        if(moveCell.CellType == boar.CellMoveType)
        {
            AgentState<Boar.BoarActions> newState = new AgentState<Boar.BoarActions>(moveCell, agent.NowAgentState.nowAction); ;
            return newState;
        }

        return agent.NowAgentState;
    }

    public override Boar.BoarActions GetNextAction()
    {
        return agent.NowAgentState.nowAction;
    }

    public override Boar.BoarActions GetNextActionOnFrameUpdate()
    {
        return agent.NowAgentState.nowAction;
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

